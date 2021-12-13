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

        // Create a GameObject for each of our tiles, so they show visually. (and redunt reduntantly)
        foreach (Tile tile in World.world.tiles) {

            // This creates a new GameObject and adds it to our scene.
            GameObject gameObjectTile = new GameObject("Tile_" + tile.x + "_" + tile.y);
            GameObject gameObjectBorder = new GameObject("Tile_" + tile.x + "_" + tile.y + "Border");

            gameObjectTile.transform.position = tile.toVector3();
            gameObjectBorder.transform.position = tile.toVector3();

            gameObjectTile.transform.SetParent(transform, true);
            gameObjectBorder.transform.SetParent(transform, true);

            // Add a Sprite Renderer
            // Add a default sprite for empty tiles.
            gameObjectBorder.AddComponent<SpriteRenderer>().sortingLayerName = "Border";
            gameObjectBorder.GetComponent<SpriteRenderer>().sprite = tileBorder;
            gameObjectBorder.SetActive(false);

            // Add a Sprite Renderer
            // Add a default sprite for empty tiles.
            SpriteRenderer spriteRenderer = gameObjectTile.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = grass;
            spriteRenderer.sortingLayerName = "Tiles";

            // Add our tile/GO pair to the dictionary.
            tileGameObjectMap.Add(tile, gameObjectTile);
            tileBorderOverlays.Add(tile, gameObjectBorder);

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
