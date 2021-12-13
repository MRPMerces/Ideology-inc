using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class World : IXmlSerializable {

    // A two-dimensional array to hold our tile data.
    public Tile[,] tiles{ get; protected set; }
    public List<Character> characters;
    public List<Furniture> furnitures;
    public InventoryManager inventoryManager;

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;

    public Dictionary<string, Furniture> furniturePrototypes;
    public Dictionary<string, Job> furnitureJobPrototypes;

    // The tile width of the world.
    public int width { get; protected set; }

    // The tile height of the world
    public int height { get; protected set; }

    // TODO: Most likely this will be replaced with a dedicated
    // class for managing job queues (plural!) that might also
    // be semi-static or self initializing or some damn thing.
    // For now, this is just a PUBLIC member of World
    public JobQueue jobQueue;

    static public World world { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="World"/> class.
    /// </summary>
    /// <param name="width">Width in tiles.</param>
    /// <param name="height">Height in tiles.</param>
    public World(int width, int height) {
        // Creates an empty world.
        SetupWorld(width, height);

        // Make one character
        CreateCharacter(GetTileAt(this.width / 2, this.height / 2));
    }

    void SetupWorld(int width, int height) {
        jobQueue = new JobQueue();

        // Set the current world to be this world.
        // TODO: Do we need to do any cleanup of the old world?
        world = this;

        this.width = width;
        this.height = height;

        tiles = new Tile[this.width, this.height];

        for (int x = 0; x < this.width; x++) {
            for (int y = 0; y < this.height; y++) {
                tiles[x, y] = new Tile(x, y);
                tiles[x, y].RegisterTileTypeChangedCallback(this.OnTileChanged);
            }
        }

        Debug.Log("World created with " + (width * height) + " tiles.");

        // FIXME: 
        CreateFurniturePrototypes();

        characters = new List<Character>();
        furnitures = new List<Furniture>();
        inventoryManager = new InventoryManager();

    }

    public void Update(float deltaTime) {
        foreach (Character character in characters) {
            character.Update(deltaTime);
        }

        foreach (Furniture furniture in furnitures) {
            furniture.Update(deltaTime);
        }
    }

    public Character CreateCharacter(Tile tile) {
        Debug.Log("CreateCharacter");
        Character character = new Character(tile);

        characters.Add(character);

        cbCharacterCreated?.Invoke(character);

        return character;
    }

    public void SetFurnitureJobPrototype(Job job, Furniture furniture) {
        furnitureJobPrototypes[furniture.objectType] = job;
    }

    void CreateFurniturePrototypes() {
        furniturePrototypes = new Dictionary<string, Furniture>();
        furnitureJobPrototypes = new Dictionary<string, Job>();

        // READ FURNITURE PROTOTYPE XML FILE HERE
        // In the future, instead of using the Unity Resources system,
        // we will be reading from a regular file on the hard drive -- and
        // hopefully we'll just get passed some kind of data stream instead
        // of hard-coding in a path here.
        TextAsset furnText = Resources.Load<TextAsset>("Data/Furniture");

        XmlTextReader reader = new XmlTextReader(new StringReader(furnText.text));

        int furnCount = 0;
        if (reader.ReadToDescendant("Furnitures")) {
            if (reader.ReadToDescendant("Furniture")) {
                do {
                    furnCount++;

                    Furniture furniture = new Furniture();
                    furniture.ReadXmlPrototype(reader);

                    furniturePrototypes[furniture.objectType] = furniture;



                } while (reader.ReadToNextSibling("Furniture"));
            }

            else {
                Debug.LogError("The furniture prototype definition file doesn't have any 'Furniture' elements.");
            }
        }

        else {
            Debug.LogError("Did not find a 'Furnitures' element in the prototype definition file.");
        }

        Debug.Log("Furniture prototypes read: " + furnCount.ToString());

        furniturePrototypes["Door"].RegisterUpdateAction(FurnitureActions.Door_UpdateAction);
        furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;

        furniturePrototypes["Steel Mill"].jobSpotOffset = new Vector2(1, 0);
        furniturePrototypes["Steel Mill"].RegisterUpdateAction(FurnitureActions.SteelMill_UpdateAction);

}

    public void SetupPathfindingExample() {
        Debug.Log("SetupPathfindingExample");

        // Make a set of floors/walls to test pathfinding with.

        int l = width / 2 - 5;
        int b = height / 2 - 5;

        for (int x = l - 5; x < l + 15; x++) {
            for (int y = b - 5; y < b + 15; y++) {
                tiles[x, y].Type = TileType.Floor;

                if (x == l || x == (l + 9) || y == b || y == (b + 9)) {
                    if (x != (l + 9) && y != (b + 4)) {
                        PlaceFurniture("furn_SteelWall", tiles[x, y]);
                    }
                }
            }
        }

    }

    /// <summary>
    /// Gets the tile data at x and y.
    /// </summary>
    /// <returns>The <see cref="Tile"/>.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile GetTileAt(int x, int y) {
        if (x >= width || x < 0 || y >= height || y < 0) {
            return null;
        }

        return tiles[x, y];
    }

    /// <summary>
    /// Gets the tile at the unity-space coordinates
    /// </summary>
    /// <returns>The tile at world coordinate.</returns>
    /// <param name="vector3">Unity World-Space coordinates.</param>
    public Tile GetTileAt(Vector3 vector3) {
        return GetTileAt(Mathf.FloorToInt(vector3.x + 0.5f), Mathf.FloorToInt(vector3.y + 0.5f));
    }

    public Furniture PlaceFurniture(string objectType, Tile tile) {
        //Debug.Log("PlaceInstalledObject");
        // TODO: This function assumes 1x1 tiles -- change this later!

        if (furniturePrototypes.ContainsKey(objectType) == false) {
            Debug.LogError("furniturePrototypes doesn't contain a proto for key: " + objectType);
            return null;
        }

        Furniture furniture = Furniture.PlaceInstance(furniturePrototypes[objectType], tile);

        if (furniture == null) {
            // Failed to place object -- most likely there was already something there.
            return null;
        }

        furniture.RegisterOnRemovedCallback(OnFurnitureRemoved);
        furnitures.Add(furniture);

        // Do we need to recalculate our rooms?
        if (tile.room != null && !tile.room.hasReqirements) {
            tile.room.checkFurniture();
        }

        if (cbFurnitureCreated != null) {
            cbFurnitureCreated(furniture);

            if (furniture.movementCost != 1) {
                // Since tiles return movement cost as their base cost multiplied
                // buy the furniture's movement cost, a furniture movement cost
                // of exactly 1 doesn't impact our pathfinding system, so we can
                // occasionally avoid invalidating pathfinding graphs
                InvalidateTileGraph();  // Reset the pathfinding system
            }
        }

        return furniture;
    }

    // This should be called whenever a change to the world
    // means that our old pathfinding info is invalid.
    public void InvalidateTileGraph() {
        tileGraph = null;
    }

    public bool IsFurniturePlacementValid(string furnitureType, Tile tile) {
        return furniturePrototypes[furnitureType].IsValidPosition(tile);
    }

    public Furniture GetFurniturePrototype(string objectType) {
        if (furniturePrototypes.ContainsKey(objectType) == false) {
            Debug.LogError("No furniture with type: " + objectType);
            return null;
        }

        return furniturePrototypes[objectType];
    }

    #region callbacks
    Action<Furniture> cbFurnitureCreated;
    Action<Character> cbCharacterCreated;
    Action<Inventory> cbInventoryCreated;
    Action<Tile> cbTileChanged;

    public void RegisterFurnitureCreated(Action<Furniture> callbackfunc) {
        cbFurnitureCreated += callbackfunc;
    }

    public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc) {
        cbFurnitureCreated -= callbackfunc;
    }

    public void RegisterCharacterCreated(Action<Character> callbackfunc) {
        cbCharacterCreated += callbackfunc;
    }

    public void UnregisterCharacterCreated(Action<Character> callbackfunc) {
        cbCharacterCreated -= callbackfunc;
    }

    public void RegisterInventoryCreated(Action<Inventory> callbackfunc) {
        cbInventoryCreated += callbackfunc;
    }

    public void UnregisterInventoryCreated(Action<Inventory> callbackfunc) {
        cbInventoryCreated -= callbackfunc;
    }

    public void RegisterTileChanged(Action<Tile> callbackfunc) {
        cbTileChanged += callbackfunc;
    }

    public void UnregisterTileChanged(Action<Tile> callbackfunc) {
        cbTileChanged -= callbackfunc;
    }

    public void OnInventoryCreated(Inventory inv) {
        cbInventoryCreated?.Invoke(inv);
    }

    public void OnFurnitureRemoved(Furniture furn) {
        furnitures.Remove(furn);
    }

    // Gets called whenever ANY tile changes
    void OnTileChanged(Tile tile) {
        if (cbTileChanged != null) {
            cbTileChanged(tile);
            InvalidateTileGraph();
        }
    }

    #endregion callbacks

    #region saving and loading
    /// <summary>
    /// Default constructor, used when loading a world from a file.
    /// </summary>
    private World() {

    }

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        // Save info here
        writer.WriteAttributeString("Width", width.ToString());
        writer.WriteAttributeString("Height", height.ToString());

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (tiles[x, y].Type != TileType.Empty) {
                    writer.WriteStartElement("Tile");
                    tiles[x, y].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Furnitures");
        foreach (Furniture furn in furnitures) {
            writer.WriteStartElement("Furniture");
            furn.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        foreach (Character c in characters) {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();

        /*		writer.WriteStartElement("Width");
                writer.WriteValue(Width);
                writer.WriteEndElement();
        */

        //Debug.Log(writer.ToString());

    }

    public void ReadXml(XmlReader reader) {
        Debug.Log("World::ReadXml");
        // Load info here

        width = int.Parse(reader.GetAttribute("Width"));
        height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(width, height);

        while (reader.Read()) {
            switch (reader.Name) {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }
        }

        // DEBUGGING ONLY!  REMOVE ME LATER!
        // Create an Inventory Item
        Inventory inv = new Inventory("Steel Plate", 50, 50);
        Tile t = GetTileAt(width / 2, height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null) {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory("Steel Plate", 50, 4);
        t = GetTileAt(width / 2 + 2, height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null) {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory("Steel Plate", 50, 3);
        t = GetTileAt(width / 2 + 1, height / 2 + 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null) {
            cbInventoryCreated(t.inventory);
        }
    }

    void ReadXml_Tiles(XmlReader reader) {
        Debug.Log("ReadXml_Tiles");
        // We are in the "Tiles" element, so read elements until
        // we run out of "Tile" nodes.

        if (reader.ReadToDescendant("Tile")) {
            // We have at least one tile, so do something with it.

            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                tiles[x, y].ReadXml(reader);
            } while (reader.ReadToNextSibling("Tile"));

        }

    }

    void ReadXml_Furnitures(XmlReader reader) {
        Debug.Log("ReadXml_Furnitures");

        if (reader.ReadToDescendant("Furniture")) {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("objectType"), tiles[x, y]);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furniture"));
        }
    }

    void ReadXml_Characters(XmlReader reader) {
        Debug.Log("ReadXml_Characters");
        if (reader.ReadToDescendant("Character")) {
            do {
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));

                Character c = CreateCharacter(tiles[x, y]);
                c.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));
        }
    }

    #endregion saving and loading
}


/*
 * TODO:
 * Add modifiercontroller form mt.
 * Add modifiers to everything.
 *
 */