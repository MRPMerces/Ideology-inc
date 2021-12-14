using System.Collections.Generic;
using UnityEngine;

public class TileSpriteController : MonoBehaviour {

    public static TileSpriteController tileSpriteController { get; protected set; }
    // The only tile sprite we have right now, so this it a pretty simple way to handle it.
    public Sprite concrete_foundation;  // FIXME!
    public Sprite grass;  // FIXME!
    public Sprite road;

    // Default sprite for everything.
    public Sprite errorSprite;
    public Sprite tileBorder;  // FIXME!

    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<Tile, GameObject> tileBorderOverlays;

    bool bordersEnabled = false;
    // Use this for initialization
    void Start() {
        tileSpriteController = this;

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        tileBorderOverlays = new Dictionary<Tile, GameObject>();

        // Create a GameObject for each of our tiles.
        foreach (Tile tile in World.world.tiles) {

            // Create a new GameObject.
            GameObject gameObjectTile = new GameObject("Tile_" + tile.x + "_" + tile.y);
            gameObjectTile.transform.position = tile.toVector3();
            gameObjectTile.transform.SetParent(transform, true);

            // Add a Sprite Renderer to the gameObjectTile.
            gameObjectTile.AddComponent<SpriteRenderer>().sortingLayerName = "Tiles";

            // Add our Tile GameObject pair to the dictionary.
            tileGameObjectMap.Add(tile, gameObjectTile);

            // Create a new GameObject.
            GameObject gameObjectBorder = new GameObject("Tile_" + tile.x + "_" + tile.y + "_Border");
            gameObjectBorder.transform.position = tile.toVector3();
            gameObjectBorder.transform.SetParent(transform, true);
            gameObjectBorder.SetActive(bordersEnabled);

            // Add a Sprite Renderer to the gameObjectBorder.
            SpriteRenderer tileBorderRenderer = gameObjectBorder.AddComponent<SpriteRenderer>();
            tileBorderRenderer.sortingLayerName = "Border";
            tileBorderRenderer.sprite = tileBorder;

            // Add our Tile GameObject pair to the dictionary.
            tileBorderOverlays.Add(tile, gameObjectBorder);

            // Call the callback so that 
            OnTileChanged(tile);
        }

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        World.world.RegisterTileChanged(OnTileChanged);
    }

    // This function should be called automatically whenever a tile's data gets changed.
    void OnTileChanged(Tile tile) {

        if (tileGameObjectMap.ContainsKey(tile) == false) {
            Debug.LogError("tileGameObjectMap doesn't contain the tile_data");
            return;
        }

        GameObject gameObject = tileGameObjectMap[tile];

        if (gameObject == null) {
            Debug.LogError("tileGameObjectMap's returned GameObject is null");
            return;
        }

        switch (tile.Type) {
            case TileType.Empty:
                gameObject.GetComponent<SpriteRenderer>().sprite = grass;
                break;

            case TileType.Floor:
                gameObject.GetComponent<SpriteRenderer>().sprite = concrete_foundation;
                break;

            case TileType.ROAD:
                gameObject.GetComponent<SpriteRenderer>().sprite = road;
                break;

            default:
                Debug.LogError("OnTileTypeChanged - Unrecognized tile type.");
                break;
        }
    }

    public void enableBorder(bool enable = true) {
        if (bordersEnabled != enable) {
            bordersEnabled = enable;
            foreach (GameObject gameObject in tileBorderOverlays.Values) {
                gameObject.SetActive(enable);
            }
        }
    }
}
