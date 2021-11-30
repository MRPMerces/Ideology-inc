using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomController : MonoBehaviour {
    public static RoomController roomController;


    List<Room> rooms;

    // Start is called before the first frame update
    void OnEnable() {
        rooms = new List<Room>();
        roomController = this;
    }

    // Update is called once per frame
    void Update() {

    }


    public void AddRoom(Room room) {
        if (room == null) {
            Debug.LogError("room = null");
            return;
        }

        rooms.Add(room);
        cbRoomChanged?.Invoke(room);
    }

    public void removeRoom(Room room) {
        if (room.tiles.Count != 0) {
            room.unAssignTiles(room.tiles);
        }

        rooms.Remove(room);
        cbRoomChanged?.Invoke(room);
    }

    //    writer.WriteStartElement("Rooms");
    //        foreach (Room r in rooms) {

    //            if (GetOutsideRoom() == r)
    //                continue;   // Skip the outside room. Alternatively, should SetupWorld be changed to not create one?

    //            writer.WriteStartElement("Room");
    //            r.WriteXml(writer);
    //            writer.WriteEndElement();
    //        }
    //writer.WriteEndElement();

    //void ReadXml_Rooms(XmlReader reader) {
    //    Debug.Log("ReadXml_Rooms");

    //    if (reader.ReadToDescendant("Room")) {
    //        do {
    //            /*int x = int.Parse( reader.GetAttribute("X") );
    //int y = int.Parse( reader.GetAttribute("Y") );

    //Furniture furn = PlaceFurniture( reader.GetAttribute("objectType"), tiles[x,y], false );*/

    //            //furn.ReadXml(reader);

    //            Room r = new Room();
    //            rooms.Add(r);
    //            r.ReadXml(reader);
    //        } while (reader.ReadToNextSibling("Room"));

    //    }

    //}



    Action<Room> cbRoomChanged;

    public void RegisterRoomChanged(Action<Room> callbackfunc) {
        cbRoomChanged += callbackfunc;
    }

    public void UnregisterRoomChanged(Action<Room> callbackfunc) {
        cbRoomChanged -= callbackfunc;
    }
}
