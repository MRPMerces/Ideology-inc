using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileSpriteController : MonoBehaviour {

    public static TileSpriteController tileSpriteController { get; protected set; }

    public Sprite citySprite;
    public Sprite emptySprite;

    Dictionary<Tile, GameObject> tileGameObjectMap;

    // Start is called before the first frame update
    void Start() {
        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        tileSpriteController = this;

        foreach (Tile tile in World.world.tiles) {
            // This creates a new GameObject and adds it to our scene.
            GameObject gameObjectTile = new GameObject("Tile_" + tile.X + "_" + tile.Y);

            gameObjectTile.transform.position = new Vector3(tile.X, tile.Y, 0);

            gameObjectTile.transform.SetParent(this.transform, true);

            // Add a Sprite Renderer
            // Add a default sprite for empty tiles.
            gameObjectTile.AddComponent<SpriteRenderer>().sortingLayerName = "Tiles";

            // Add our tile/GO pair to the dictionary.
            tileGameObjectMap.Add(tile, gameObjectTile);
            OnTileChanged(tile);
        }

        // Register our callback so that our GameObject gets updated whenever a tile's type changes.
        World.world.RegisterTileChanged(OnTileChanged);
    }

    void DestroyAllTileGameObjects() {
        // This function might get called when we are changing floors/levels.
        // We need to destroy all visual **GameObjects** -- but not the actual tile data!

        while (tileGameObjectMap.Count > 0) {
            Tile tile = tileGameObjectMap.Keys.First();

            // Remove the pair from the map
            tileGameObjectMap.Remove(tile);

            // Unregister the callback!
            tile.UnregisterTileTypeChangedCallback(OnTileChanged);

            // Destroy the visual GameObject
            Destroy(tileGameObjectMap[tile]);
        }

        // Presumably, after this function gets called, we'd be calling another
        // function to build all the GameObjects for the tiles on the new floor/level
    }

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

        switch (tile.type) {
            case TileType.City:
                gameObject.GetComponent<SpriteRenderer>().sprite = citySprite;
                break;

            case TileType.Empty:
                gameObject.GetComponent<SpriteRenderer>().sprite = emptySprite;
                break;

            default:
                Debug.LogError("Unrecognized tile type.");
                return;
        }
    }
}
