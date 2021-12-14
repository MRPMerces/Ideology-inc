using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomSpriteController : MonoBehaviour {

    public static RoomSpriteController roomSpriteController;

    public Sprite Bathroom;
    public Sprite Bedroom;
    public Sprite Office;

    public Font font;

    Dictionary<Tile, GameObject> roomOverlaySprites;
    Dictionary<Room, GameObject> roomNames;

    bool roomOverlayEnabled = false;

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
                gameObject.SetActive(roomOverlayEnabled);

                SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = getSprite(room.roomType);
                spriteRenderer.sortingLayerName = "Furniture";

                roomOverlaySprites.Add(tile, gameObject);

                // TODO: When tiles are removed from rooms, the overlay stays!
                // TODO: Add 1 sprite for each roomtype, and set them dynamicly.
            }
        }

        setText(room);
    }

    Sprite getSprite(RoomType roomType) {
        switch (roomType) {
            case RoomType.BATHROOM:
                return Bathroom;

            case RoomType.BEDROOM:
                return Bedroom;

            case RoomType.OFFICE:
                return Office;

            default:
                return TileSpriteController.tileSpriteController.errorSprite;
        }
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
            gameObject.AddComponent<Canvas>();

            Text text = gameObject.AddComponent<Text>();
            text.text = room.roomType.ToString();
            text.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");

            roomNames.Add(room, gameObject);
        }

        else {
            roomNames[room].transform.position = new Vector3(e - (e - w) / 2, n - (n - s) / 2);
        }
    }

    public void enableRoomOverlay(bool enable = true) {
        if (roomOverlayEnabled != enable) {
            roomOverlayEnabled = enable;
            foreach (GameObject gameObject in roomOverlaySprites.Values) {
                gameObject.SetActive(enable);
            }
        }
    }
}
