using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public enum RoomType { Bathroom, Bedroom, Office }
public class Room : IXmlSerializable {

    public List<Tile> tiles { get; protected set; }

    public bool hasReqirements { get; protected set; }

    public RoomType roomType;

    public GameObject gameObject;

    public Room(List<Tile> tiles, RoomType roomType) {
        this.tiles = new List<Tile>();
        this.roomType = roomType;

        AssignTiles(tiles);
        checkFurniture();
    }

    public void AssignTiles(List<Tile> tilesToAdd) {
        foreach (Tile tile in tilesToAdd) {
            if (tiles.Contains(tile) && tile.room != null) {
                // This tile is already in this room or allready assigned to a room.
                continue;
            }

            tile.room = this;
            tiles.Add(tile);
        }

        checkFurniture();
    }

    public void unAssignTiles(List<Tile> tilesToRemove, Tile tileToRemove = null) {
        if (tileToRemove == null) {
            foreach (Tile tile in tilesToRemove) {
                if (!tiles.Contains(tile)) {
                    // This tile already in this room.
                    continue;
                }

                tile.room = null;
                tiles.Remove(tile);
            }

            if (tiles.Count == 0) {
                RoomController.roomController.removeRoom(this);
                return;
            }
        }

        checkFurniture();
    }

    public void checkFurniture() {
        //List<Furniture> furnitures = new List<Furniture>();
        //foreach (Tile tile in tiles) {
        //    if (tile.furniture != null) {
        //        furnitures.Add(tile.furniture);
        //    }
        //}
    }

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
    }

    public void ReadXml(XmlReader reader) {
        // Read gas info
        if (reader.ReadToDescendant("Param")) {
            do {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
            } while (reader.ReadToNextSibling("Param"));
        }
    }
}
