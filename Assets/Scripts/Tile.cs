using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public enum TileType { Empty, City, River };

public class Tile : IXmlSerializable {
    public Tile(int x, int y) {
        X = x;
        Y = y;
    }

    public TileType type { get; protected set; }

    public readonly int X;
    public readonly int Y;

    public void setType(TileType type) {
        if (this.type == type) {
            return;
        }

        if (this.type == TileType.City && type != TileType.City) {
            Debug.LogError("Trying to remove a city_tile, can we do this?");
            return;
        }

        this.type = type;

        // Call the callback and let things know we've changed.
        cbTileChanged?.Invoke(this);
    }

    /// <summary>
    /// Function that finds the neighbours
    /// </summary>
    /// <param name="diagOkay">Do the caller want the diagonale neighbours in the array</param>
    /// <returns>A array of neighbouring tiles NOTE! some tiles migth be null
    /// </returns>
    public Tile[] getNeighbours(bool diagOkay = false) {
        Tile[] tiles;

        if (diagOkay == false) {
            tiles = new Tile[4];   // Tile order: N E S W
        }
        else {
            tiles = new Tile[8];   // Tile order : N E S W NE SE SW NW
        }

        tiles[0] = World.world.getTileAt(X, Y + 1);
        tiles[1] = World.world.getTileAt(X + 1, Y);
        tiles[2] = World.world.getTileAt(X, Y - 1);
        tiles[3] = World.world.getTileAt(X - 1, Y);

        if (diagOkay == true) {
            tiles[4] = World.world.getTileAt(X + 1, Y + 1);
            tiles[5] = World.world.getTileAt(X + 1, Y - 1);
            tiles[6] = World.world.getTileAt(X - 1, Y - 1);
            tiles[7] = World.world.getTileAt(X - 1, Y + 1);
        }

        return tiles;
    }

    #region Callbacks

    Action<Tile> cbTileChanged;

    /// <summary>
    /// Register a function to be called back when our tile type changes.
    /// </summary>
    public void RegisterTileTypeChangedCallback(Action<Tile> callback) {
        cbTileChanged += callback;
    }

    public void UnregisterTileTypeChangedCallback(Action<Tile> callback) {
        cbTileChanged -= callback;
    }

    #endregion Callbacks

    #region Saving and loading

    /// <summary>
    /// Default constructor for saving and loading.
    /// </summary>
    private Tile() { }

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)type).ToString());
    }

    public void ReadXml(XmlReader reader) {
        Debug.Log(reader.NodeType + "Name: " + reader.Name + "value: " + reader.Value);
        type = (TileType)int.Parse(reader.GetAttribute("Type"));

  

        //Debug.Log("Name: " +reader.Name);
        if (reader.ReadToDescendant("City")) {
            Debug.Log("iscity" + "value: " + reader.Value);
            //Debug.Log("Name: " + reader.Name);
        }

        //Debug.Log("Name: " + reader.Name);
    }

    #endregion Saving and loading
}
