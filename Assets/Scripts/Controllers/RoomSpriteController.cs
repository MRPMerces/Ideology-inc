using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomSpriteController : MonoBehaviour {

    public Sprite sprite;

    public static RoomSpriteController roomSpriteController;

    Dictionary<Tile, GameObject> roomOverlaySprites;
    Dictionary<Room, GameObject> roomNames;

    // Start is called before the first frame update
    void Start() {
        roomSpriteController = this;
        roomNames = new Dictionary<Room, GameObject>();
        roomOverlaySprites = new Dictionary<Tile, GameObject>();

        RoomController.roomController.RegisterRoomChanged(onRoomChanged);
    }

    // Update is called once per frame
    void Update() {

    }

    void onRoomChanged(Room room) {

        // Loop throug all the tiles in the room, and create a Tile GameObject pair. Then set the corect sprite.
        foreach (Tile tile in room.tiles) {
            if (!roomOverlaySprites.ContainsKey(tile)) {
                GameObject gameObject = new GameObject("Tile_" + tile.x + "_" + tile.y);
                gameObject.transform.position = new Vector3(tile.x, tile.y, 0);
                gameObject.transform.SetParent(transform, true);

                SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = sprite;
                spriteRenderer.sortingLayerName = "Tiles";

                roomOverlaySprites.Add(tile, gameObject);

                // TODO: When tiles are removed from rooms, the overlay stays!
                // TODO: Add 1 sprite for each roomtype, and set them dynamicly.
            }
        }

        setText(room);
    }

    void setText(Room room) {
        // Find the outer corners of the room.
        int n = room.tiles[0].y;
        int e = room.tiles[0].x;
        int s = room.tiles[0].y;
        int w = room.tiles[0].x;

        foreach (Tile tile in room.tiles) {
            if (tile.y > n) {
                n = tile.y;
            }

            if (tile.x > e) {
                e = tile.x;
            }

            if (tile.y < s) {
                s = tile.y;
            }

            if (tile.x < w) {
                w = tile.x;
            }
        }

        if (!roomNames.ContainsKey(room)) {

            // Create a Room GameObject pair for the room, and display the roomtype.
            GameObject gameObject = new GameObject(room.roomType.ToString());
            gameObject.transform.position = new Vector3(e - (e - w) / 2, n - (n - s) / 2);
            gameObject.transform.SetParent(transform, true);
            gameObject.AddComponent<Text>().text = room.roomType.ToString();

            roomNames.Add(room, gameObject);
        }

        else {
            roomNames[room].transform.position = new Vector3(e - (e - w) / 2, n - (n - s) / 2);
        }
    }
}
