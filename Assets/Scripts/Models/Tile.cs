using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

// TileType is the base type of the tile. In some tile-based games, that might be
// the terrain type. For us, we only need to differentiate between empty space
// and floor (a.k.a. the station structure/scaffold). Walls/Doors/etc... will be
// InstalledObjects sitting on top of the floor.
public enum TileType { Empty, Floor, ROAD };

public enum ENTERABILITY { Yes, Never, Soon };

public class Tile : IXmlSerializable {
    private TileType _type = TileType.Empty;
    public TileType Type {
        get { return _type; }
        set {
            TileType oldType = _type;
            _type = value;
            // Call the callback and let things know we've changed.

            if (cbTileChanged != null && oldType != _type) {
                cbTileChanged(this);
            }
        }
    }

    // LooseObject is something like a drill or a stack of metal sitting on the floor
    public Inventory inventory;

    public Room room;

    // Furniture is something like a wall, door, or sofa.
    public Furniture furniture { get; protected set; }

    // FIXME: This seems like a terrible way to flag if a job is pending
    // on a tile.  This is going to be prone to errors in set/clear.
    public Job pendingFurnitureJob;

    public int x { get; protected set; }
    public int y { get; protected set; }

    // FIXME: This is just hardcoded for now.  Basically just a reminder of something we
    // might want to do more with in the future.
    const float baseTileMovementCost = 1;

    public float movementCost {
        get {

            if (Type == TileType.Empty)
                return 0;   // 0 is unwalkable

            if (furniture == null)
                return baseTileMovementCost;

            return baseTileMovementCost * furniture.movementCost;
        }
    }

    // The function we callback any time our tile's data changes
    Action<Tile> cbTileChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tile"/> class.
    /// </summary>
    /// <param name="World.current">A World.current instance.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile(int x, int y) {
        this.x = x;
        this.y = y;
    }

    /// <summary>
    /// Register a function to be called back when our tile type changes.
    /// </summary>
    public void RegisterTileTypeChangedCallback(Action<Tile> callback) {
        cbTileChanged += callback;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterTileTypeChangedCallback(Action<Tile> callback) {
        cbTileChanged -= callback;
    }

    public bool UnplaceFurniture() {
        // Just uninstalling.  FIXME:  What if we have a multi-tile furniture?

        if (furniture == null) {
            return false;
        }

        for (int x_off = x; x_off < (x + furniture.Width); x_off++) {
            for (int y_off = y; y_off < (y + furniture.Height); y_off++) {
                World.world.GetTileAt(x_off, y_off).furniture = null;
            }
        }

        return true;
    }

    public bool PlaceFurniture(Furniture objInstance) {

        if (objInstance == null) {
            return UnplaceFurniture();
        }

        if (objInstance.IsValidPosition(this) == false) {
            Debug.LogError("Trying to assign a furniture to a tile that isn't valid!");
            return false;
        }

        for (int x_off = x; x_off < (x + objInstance.Width); x_off++) {
            for (int y_off = y; y_off < (y + objInstance.Height); y_off++) {
                World.world.GetTileAt(x_off, y_off).furniture = objInstance;
            }
        }

        return true;
    }

    public bool PlaceInventory(Inventory inventory) {
        if (inventory == null) {
            this.inventory = null;
            return true;
        }

        if (this.inventory != null) {
            // There's already inventory here. Maybe we can combine a stack?

            if (this.inventory.objectType != inventory.objectType) {
                Debug.LogError("Trying to assign inventory to a tile that already has some of a different type.");
                return false;
            }

            int numToMove = inventory.stackSize;
            if (this.inventory.stackSize + numToMove > this.inventory.maxStackSize) {
                numToMove = this.inventory.maxStackSize - this.inventory.stackSize;
            }

            this.inventory.stackSize += numToMove;
            inventory.stackSize -= numToMove;

            return true;
        }

        // At this point, we know that our current inventory is actually
        // null.  Now we can't just do a direct assignment, because
        // the inventory manager needs to know that the old stack is now
        // empty and has to be removed from the previous lists.

        this.inventory = inventory.Clone();
        this.inventory.tile = this;
        inventory.stackSize = 0;

        return true;
    }

    // Tells us if two tiles are adjacent.
    public bool IsNeighbour(Tile tile, bool diagOkay = false) {
        // Check to see if we have a difference of exactly ONE between the two
        // tile coordinates.  Is so, then we are vertical or horizontal neighbours.
        return Mathf.Abs(x - tile.x) + Mathf.Abs(y - tile.y) == 1 || (diagOkay && (Mathf.Abs(x - tile.x) == 1 && Mathf.Abs(y - tile.y) == 1));
    }

    /// <summary>
    /// Gets the neighbours.
    /// </summary>
    /// <returns>The neighbours.</returns>
    /// <param name="diagOkay">Is diagonal movement okay?.</param>
    public Tile[] GetNeighbours(bool diagOkay = false) {
        Tile[] tiles;

        if (diagOkay == false) {
            tiles = new Tile[4];   // Tile order: N E S W
        }

        else {
            tiles = new Tile[8];   // Tile order : N E S W NE SE SW NW
        }

        tiles[0] = World.world.GetTileAt(x, y + 1);
        tiles[1] = World.world.GetTileAt(x + 1, y);
        tiles[2] = World.world.GetTileAt(x, y - 1);
        tiles[3] = World.world.GetTileAt(x - 1, y);

        if (diagOkay == true) {
            tiles[4] = World.world.GetTileAt(x + 1, y + 1);
            tiles[5] = World.world.GetTileAt(x + 1, y - 1);
            tiles[6] = World.world.GetTileAt(x - 1, y - 1);
            tiles[7] = World.world.GetTileAt(x - 1, y + 1);
        }

        return tiles;
    }


    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", x.ToString());
        writer.WriteAttributeString("Y", y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader) {
        // X and Y have already been read/processed

        Type = (TileType)int.Parse(reader.GetAttribute("Type"));
    }

    public ENTERABILITY IsEnterable() {
        // This returns true if you can enter this tile right this moment.
        if (movementCost == 0) {
            return ENTERABILITY.Never;
        }

        // Check out furniture to see if it has a special block on enterability
        if (furniture != null && furniture.IsEnterable != null) {
            return furniture.IsEnterable(furniture);
        }

        return ENTERABILITY.Yes;
    }

    public bool hasFurnitureOfType(string type) {
        if (furniture == null) {
            return false;
        }

        return furniture.objectType == type;
    }

    public Tile North() {
        return World.world.GetTileAt(x, y + 1);
    }

    public Tile South() {
        return World.world.GetTileAt(x, y - 1);
    }

    public Tile East() {
        return World.world.GetTileAt(x + 1, y);
    }

    public Tile West() {
        return World.world.GetTileAt(x - 1, y);
    }

    public Vector3 toVector3() {
        return new Vector3(x, y, 0);
    }
}
