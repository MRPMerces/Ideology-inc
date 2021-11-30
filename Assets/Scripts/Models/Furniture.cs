using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

// InstalledObjects are things like walls, doors, and furniture (e.g. a sofa)

public class Furniture : IXmlSerializable {

    /// <summary>
    /// Custom parameter for this particular piece of furniture.  We are
    /// using a dictionary because later, custom LUA function will be
    /// able to use whatever parameters the user/modder would like.
    /// Basically, the LUA code will bind to this dictionary.
    /// </summary>
    protected Dictionary<string, float> furnParameters;

    /// <summary>
    /// These actions are called every update. They get passed the furniture
    /// they belong to, plus a deltaTime.
    /// </summary>
    protected Action<Furniture, float> updateActions;

    public Func<Furniture, ENTERABILITY> IsEnterable;

    List<Job> jobs;

    // If this furniture gets worked by a person,
    // where is the correct spot for them to stand,
    // relative to the bottom-left tile of the sprite.
    // NOTE: This could even be something outside of the actual
    // furniture tile itself!  (In fact, this will probably be common).
    public Vector2 jobSpotOffset = Vector2.zero;

    // If the job causes some kind of object to be spawned, where will it appear?
    public Vector2 jobSpawnSpotOffset = Vector2.zero;

    public void Update(float deltaTime) {
        updateActions?.Invoke(this, deltaTime);
    }

    // This represents the BASE tile of the object -- but in practice, large objects may actually occupy
    // multile tiles.
    public Tile tile {
        get; protected set;
    }

    // This "objectType" will be queried by the visual system to know what sprite to render for this object
    public string objectType {
        get; protected set;
    }

    private string _Name = null;
    public string Name {
        get {
            if (_Name == null || _Name.Length == 0) {
                return objectType;
            }
            return _Name;
        }

        set {
            _Name = value;
        }
    }

    // This is a multipler. So a value of "2" here, means you move twice as slowly (i.e. at half speed)
    // Tile types and other environmental effects may be combined.
    // For example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
    // would have a total movement cost of (2+3+3 = 8), so you'd move through this tile at 1/8th normal speed.
    // SPECIAL: If movementCost = 0, then this tile is impassible. (e.g. a wall).
    public float movementCost { get; protected set; }

    public bool roomEnclosure { get; protected set; }

    // For example, a sofa might be 3x2 (actual graphics only appear to cover the 3x1 area, but the extra row is for leg room.)
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public Color tint = Color.white;

    public bool linksToNeighbour {
        get; protected set;
    }

    Func<Tile, bool> funcPositionValidation;

    // TODO: Implement larger objects
    // TODO: Implement object rotation

    // Empty constructor is used for serialization
    public Furniture() {
        furnParameters = new Dictionary<string, float>();
        jobs = new List<Job>();
        funcPositionValidation = DEFAULT__IsValidPosition;
    }

    // Copy Constructor -- don't call this directly, unless we never
    // do ANY sub-classing. Instead use Clone(), which is more virtual.
    protected Furniture(Furniture other) {
        objectType = other.objectType;
        Name = other.Name;
        movementCost = other.movementCost;
        roomEnclosure = other.roomEnclosure;
        Width = other.Width;
        Height = other.Height;
        tint = other.tint;
        linksToNeighbour = other.linksToNeighbour;

        jobSpotOffset = other.jobSpotOffset;
        jobSpawnSpotOffset = other.jobSpawnSpotOffset;

        furnParameters = new Dictionary<string, float>(other.furnParameters);
        jobs = new List<Job>();

        if (other.updateActions != null) {
            updateActions = (Action<Furniture, float>)other.updateActions.Clone();
        }

        if (other.funcPositionValidation != null) {
            funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();
        }

        IsEnterable = other.IsEnterable;
    }

    // Make a copy of the current furniture.  Sub-classed should
    // override this Clone() if a different (sub-classed) copy
    // constructor should be run.
    virtual public Furniture Clone() {
        return new Furniture(this);
    }

    // Create furniture from parameters -- this will probably ONLY ever be used for prototypes
    public Furniture(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false) {
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.roomEnclosure = roomEnclosure;
        this.Width = width;
        this.Height = height;
        this.linksToNeighbour = linksToNeighbour;

        funcPositionValidation = DEFAULT__IsValidPosition;

        furnParameters = new Dictionary<string, float>();
    }

    static public Furniture PlaceInstance(Furniture furniture, Tile tile) {
        if (furniture.funcPositionValidation(tile) == false) {
            Debug.LogError("PlaceInstance -- Position Validity Function returned FALSE.");
            return null;
        }

        // We know our placement destination is valid.
        Furniture obj = furniture.Clone(); // Simplify??
        obj.tile = tile;

        // FIXME: This assumes we are 1x1!
        if (tile.PlaceFurniture(obj) == false) {
            // For some reason, we weren't able to place our object in this tile.
            // (Probably it was already occupied.)

            // Do NOT return our newly instantiated object.
            // (It will be garbage collected.)
            return null;
        }

        if (obj.linksToNeighbour) {
            // This type of furniture links itself to its neighbours,
            // so we should inform our neighbours that they have a new
            // buddy.  Just trigger their OnChangedCallback.

            foreach (Tile tile1 in tile.GetNeighbours()) {
                if (tile1 != null && tile1.furniture != null && tile1.furniture.cbOnChanged != null && tile1.furniture.objectType == obj.objectType) {
                    tile1.furniture.cbOnChanged(tile1.furniture);
                }
            }
        }

        return obj;
    }

    public bool IsValidPosition(Tile tile) {
        return funcPositionValidation(tile);
    }

    protected bool DEFAULT__IsValidPosition(Tile tile1) {
        for (int x_off = tile1.x; x_off < (tile1.x + Width); x_off++) {
            for (int y_off = tile1.y; y_off < (tile1.y + Height); y_off++) {
                Tile tile2 = World.world.GetTileAt(x_off, y_off);

                // Make sure tile is FLOOR and that it doesn't already have furniture
                if (tile2.Type != TileType.Floor && tile2.furniture != null) {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the custom furniture parameter from a string key.
    /// </summary>
    /// <returns>The parameter value (float).</returns>
    /// <param name="key">Key string.</param>
    /// <param name="default_value">Default value.</param>
    public float GetParameter(string key, float default_value = 0) {
        if (!furnParameters.ContainsKey(key)) {
            return default_value;
        }

        return furnParameters[key];
    }

    public void SetParameter(string key, float value) {
        furnParameters[key] = value;
    }

    public void ChangeParameter(string key, float value) {
        if (!furnParameters.ContainsKey(key)) {
            furnParameters[key] = value;
        }

        furnParameters[key] += value;
    }

    public int JobCount() {
        return jobs.Count;
    }

    public void AddJob(Job job) {
        job.furniture = this;
        jobs.Add(job);
        job.RegisterJobStoppedCallback(OnJobStopped);
        World.world.jobQueue.Enqueue(job);
    }

    void OnJobStopped(Job job) {
        RemoveJob(job);
    }

    protected void RemoveJob(Job job) {
        job.UnregisterJobStoppedCallback(OnJobStopped);
        jobs.Remove(job);
        job.furniture = null;
    }

    protected void ClearJobs() {
        foreach (Job job in jobs) {
            RemoveJob(job);
        }
    }

    public void CancelJobs() {
        foreach (Job job in jobs) {
            job.CancelJob();
        }
    }

    public bool IsStockpile() {
        return objectType == "Stockpile";
    }

    public void Deconstruct() {
        Debug.Log("Deconstruct");

        tile.UnplaceFurniture();

        cbOnRemoved?.Invoke(this);

        // Recalculate room
        tile.room.checkFurniture();

        World.world.InvalidateTileGraph();
    }

    public Tile GetJobSpotTile() {
        return World.world.GetTileAt(tile.x + (int)jobSpotOffset.x, tile.y + (int)jobSpotOffset.y);
    }

    public Tile GetSpawnSpotTile() {
        return World.world.GetTileAt(tile.x + (int)jobSpawnSpotOffset.x, tile.y + (int)jobSpawnSpotOffset.y);
    }

    #region callbacks
    public Action<Furniture> cbOnChanged;
    public Action<Furniture> cbOnRemoved;

    public void RegisterOnChangedCallback(Action<Furniture> callbackFunc) {
        cbOnChanged += callbackFunc;
    }

    public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc) {
        cbOnChanged -= callbackFunc;
    }

    public void RegisterOnRemovedCallback(Action<Furniture> callbackFunc) {
        cbOnRemoved += callbackFunc;
    }

    public void UnregisterOnRemovedCallback(Action<Furniture> callbackFunc) {
        cbOnRemoved -= callbackFunc;
    }

    /// <summary>
    /// Registers a function that will be called every Update.
    /// (Later this implementation might change a bit as we support LUA.)
    /// </summary>
    public void RegisterUpdateAction(Action<Furniture, float> a) {
        updateActions += a;
    }

    public void UnregisterUpdateAction(Action<Furniture, float> a) {
        updateActions -= a;
    }

    #endregion callbacks

    #region saving and loading
    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", tile.x.ToString());
        writer.WriteAttributeString("Y", tile.y.ToString());
        writer.WriteAttributeString("objectType", objectType);
        //writer.WriteAttributeString( "movementCost", movementCost.ToString() );

        foreach (string k in furnParameters.Keys) {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", furnParameters[k].ToString());
            writer.WriteEndElement();
        }

    }

    public void ReadXmlPrototype(XmlReader reader_parent) {
        Debug.Log("ReadXmlPrototype");

        objectType = reader_parent.GetAttribute("objectType");

        XmlReader reader = reader_parent.ReadSubtree();


        while (reader.Read()) {
            switch (reader.Name) {
                case "Name":
                    reader.Read();
                    Name = reader.ReadContentAsString();
                    break;

                case "MovementCost":
                    reader.Read();
                    movementCost = reader.ReadContentAsFloat();
                    break;

                case "Width":
                    reader.Read();
                    Width = reader.ReadContentAsInt();
                    break;

                case "Height":
                    reader.Read();
                    Height = reader.ReadContentAsInt();
                    break;

                case "LinksToNeighbours":
                    reader.Read();
                    linksToNeighbour = reader.ReadContentAsBoolean();
                    break;

                case "EnclosesRooms":
                    reader.Read();
                    roomEnclosure = reader.ReadContentAsBoolean();
                    break;

                case "BuildingJob":
                    float jobTime = float.Parse(reader.GetAttribute("jobTime"));

                    List<Inventory> invetories = new List<Inventory>();

                    XmlReader inventory_reader = reader.ReadSubtree();

                    while (inventory_reader.Read()) {
                        if (inventory_reader.Name == "Inventory") {
                            // Found an inventory requirement, so add it to the list!
                            invetories.Add(new Inventory(inventory_reader.GetAttribute("objectType"), int.Parse(inventory_reader.GetAttribute("amount")), 0));
                        }
                    }
                    World.world.SetFurnitureJobPrototype(new Job(null, objectType, FurnitureActions.JobComplete_FurnitureBuilding, jobTime, invetories.ToArray()), this);

                    break;
                case "Params":
                    ReadXmlParams(reader);  // Read in the Param tag
                    break;
            }
        }
    }

    public void ReadXml(XmlReader reader) {
        // X, Y, and objectType have already been set, and we should already
        // be assigned to a tile.  So just read extra data.

        //movementCost = int.Parse( reader.GetAttribute("movementCost") );

        ReadXmlParams(reader);
    }

    public void ReadXmlParams(XmlReader reader) {
        // X, Y, and objectType have already been set, and we should already
        // be assigned to a tile.  So just read extra data.

        //movementCost = int.Parse( reader.GetAttribute("movementCost") );

        if (reader.ReadToDescendant("Param")) {
            do {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
                furnParameters[k] = v;
            } while (reader.ReadToNextSibling("Param"));
        }
    }
    #endregion
}
