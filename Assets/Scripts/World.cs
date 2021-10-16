using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class World : IXmlSerializable {
    public World(int Width, int Height) {
        setupWorld(Width, Height);
    }

    public static World world { get; protected set; }

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;


    // A two-dimensional array to hold our tiles.
    public Tile[,] tiles { get; protected set; }

    // All the tiles with cities in them.
    public List<Tile> tilesWithCity { get; protected set; }

    public int Width { get; protected set; }
    public int Height { get; protected set; }

    void setupWorld(int Width, int Height) {
        this.Width = Width;
        this.Height = Height;
        world = this;

        tiles = new Tile[Width, Height];

        // Populate Tile array with (Width * Height) tiles
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                tiles[x, y] = new Tile(x, y);
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
            }
        }

        Debug.Log("World created with " + (Width * Height) + " tiles.");
        tilesWithCity = new List<Tile>();

        tileGraph = new Path_TileGraph();

    }

    // Add a tile to the list of tiles with cities.
    public void add_cityToList(Tile t) {
        tilesWithCity.Add(t);
    }

    // Get Tile with cordinates x,y
    public Tile getTileAt(int x, int y) {
        if ((x >= Width || x < 0 || y >= Height || y < 0)) {
            return null;
        }

        return tiles[x, y];
    }

    // Create new stats for a new city
    public CityStats newCityStats() {
        return new CityStats("city" + tilesWithCity.Count.ToString(), 100 * UnityEngine.Random.Range(0, 250));
    }

    #region Callbacks

    Action<Tile> cbTileChanged;

    public void RegisterTileChanged(Action<Tile> callbackfunc) {
        cbTileChanged += callbackfunc;
    }

    public void UnregisterTileChanged(Action<Tile> callbackfunc) {
        cbTileChanged -= callbackfunc;
    }

    // Gets called whenever ANY tile changes
    void OnTileChanged(Tile t) {
        if (cbTileChanged == null) {
            return;
        }

        cbTileChanged(t);
    }

    #endregion Callbacks

    #region Saving and loading

    /// <summary>
    /// Default constructor for saving and loading.
    /// </summary>
    private World() {
    }

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                writer.WriteStartElement("Tile");
                tiles[x, y].WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        writer.WriteEndElement();
    }

    public void ReadXml(XmlReader reader) {

        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));

        setupWorld(Width, Height);

        // We are in the "Tiles" element, so read elements until we run out of "Tile" nodes.
        if (reader.ReadToDescendant("Tile")) {
            // We have at least one tile, so do something with it.

            do {
                //Debug.Log("Name: " + reader.Name);
                int x = int.Parse(reader.GetAttribute("X"));
                int y = int.Parse(reader.GetAttribute("Y"));
                tiles[x, y].ReadXml(reader);

                Debug.Log(reader.NodeType + "Name: " + reader.Name + "value: " + reader.Value);

            } while (reader.ReadToNextSibling("Tile"));
        }
    }

    #endregion Saving and loading
}

// Struct to temp store name and population
public struct CityStats {

    public CityStats(string n, int p) {
        name = n;
        population = p;
    }

    public string name { get; }
    public int population { get; }
}
