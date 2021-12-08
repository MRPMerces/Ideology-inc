using System.Collections.Generic;
using UnityEngine;

public class TileSpriteController : MonoBehaviour {

    public static TileSpriteController tileSpriteController { get; protected set; }
    // The only tile sprite we have right now, so this it a pretty simple way to handle it.
    public Sprite floorSprite;  // FIXME!
    public Sprite emptySprite;  // FIXME!

    // Default sprite for everything.
    public Sprite errorSprite;

    Dictionary<Tile, GameObject> tileGameObjectMap;

    // Use this for initialization
    void Start() {
        tileSpriteController = this;

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create a GameObject for each of our tiles, so they show visually. (and redunt reduntantly)
        foreach (Tile tile in World.world.tiles) {

            // This creates a new GameObject and adds it to our scene.
            GameObject gameObject = new GameObject("Tile_" + tile.x + "_" + tile.y);

            gameObject.transform.position = new Vector3(tile.x, tile.y, 0);
            gameObject.transform.SetParent(transform, true);

            // Add a Sprite Renderer
            // Add a default sprite for empty tiles.
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = emptySprite;
            spriteRenderer.sortingLayerName = "Tiles";

            // Add our tile/GO pair to the dictionary.
            tileGameObjectMap.Add(tile, gameObject);

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
                gameObject.GetComponent<SpriteRenderer>().sprite = emptySprite;
                break;

            case TileType.Floor:
                gameObject.GetComponent<SpriteRenderer>().sprite = floorSprite;
                break;

            default:
                Debug.LogError("OnTileTypeChanged - Unrecognized tile type.");
                break;
        }
    }
}
