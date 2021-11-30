using System.Collections.Generic;
using UnityEngine;

public class FurnitureSpriteController : MonoBehaviour {

    public static FurnitureSpriteController furnitureSpriteController;
    Dictionary<Furniture, GameObject> furnitureGameObjectMap;

    Dictionary<string, Sprite> furnitureSprites;

    // Use this for initialization
    void Start() {
        furnitureSpriteController = this;
        LoadSprites();

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register our callback so that our GameObject gets updated whenever a tile's type changes.
        World.world.RegisterFurnitureCreated(OnFurnitureCreated);

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually
        foreach (Furniture furniture in World.world.furnitures) {
            OnFurnitureCreated(furniture);
        }
    }

    void LoadSprites() {
        furnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furniture/");

        foreach (Sprite s in sprites) {
            furnitureSprites[s.name] = s;
        }
    }

    public void OnFurnitureCreated(Furniture furniture) {
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject gameObject = new GameObject();

        gameObject.name = furniture.objectType + "_" + furniture.tile.x + "_" + furniture.tile.y;
        gameObject.transform.position = new Vector3(furniture.tile.x + ((furniture.Width - 1) / 2f), furniture.tile.y + ((furniture.Height - 1) / 2f), 0);
        gameObject.transform.SetParent(transform, true);

        if (furniture.objectType == "Door") {
            // By default, the door graphic is for walls to the east & west. If we have a wall north/south, then rotate this GO by 90 degrees

            if (furniture.tile.North() != null && furniture.tile.South() != null && furniture.tile.North().hasFurnitureOfType("furn_SteelWall") && furniture.tile.South().hasFurnitureOfType("furn_SteelWall")) {
                gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }

        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSpriteForFurniture(furniture);
        spriteRenderer.sortingLayerName = "Furniture";
        spriteRenderer.color = furniture.tint;

        // Add our tile/GO pair to the dictionary.
        furnitureGameObjectMap.Add(furniture, gameObject);

        // Register our callback so that our GameObject gets updated whenever the object's into changes.
        furniture.RegisterOnChangedCallback(OnFurnitureChanged);
        furniture.RegisterOnRemovedCallback(OnFurnitureRemoved);

    }

    void OnFurnitureRemoved(Furniture furniture) {
        if (furnitureGameObjectMap.ContainsKey(furniture) == false) {
            Debug.LogError("OnFurnitureRemoved -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furniture];
        Destroy(furn_go);
        furnitureGameObjectMap.Remove(furniture);
    }

    void OnFurnitureChanged(Furniture furniture) {
        // Make sure the furniture's graphics are correct.

        if (furnitureGameObjectMap.ContainsKey(furniture) == false) {
            Debug.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furniture];

        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furniture);
        furn_go.GetComponent<SpriteRenderer>().color = furniture.tint;
    }

    public Sprite GetSpriteForFurniture(Furniture furniture) {
        string spriteName = furniture.objectType;

        if (furniture.linksToNeighbour == false) {

            // If this is a DOOR, let's check OPENNESS and update the sprite.
            if (furniture.objectType == "Door") {
                if (furniture.GetParameter("openness") < 0.1f) {
                    // Door is closed
                    spriteName = "Door";
                }

                else if (furniture.GetParameter("openness") < 0.5f) {
                    // Door is a bit open
                    spriteName = "Door_openness_1";
                }

                else if (furniture.GetParameter("openness") < 0.9f) {
                    // Door is a lot open
                    spriteName = "Door_openness_2";
                }

                else {
                    // Door is a fully open
                    spriteName = "Door_openness_3";
                }
            }

            return furnitureSprites[spriteName];
        }

        // Otherwise, the sprite name is more complicated.

        spriteName = furniture.objectType + "_";

        if (furniture.tile.North() != null && furniture.tile.North().hasFurnitureOfType(furniture.objectType)) {
            spriteName += "N";
        }

        if (furniture.tile.East() != null && furniture.tile.East().hasFurnitureOfType(furniture.objectType)) {
            spriteName += "E";
        }

        if (furniture.tile.South() != null && furniture.tile.South().hasFurnitureOfType(furniture.objectType)) {
            spriteName += "S";
        }

        if (furniture.tile.West() != null && furniture.tile.West().hasFurnitureOfType(furniture.objectType)) {
            spriteName += "W";
        }

        // For example, if all four neighbours is of the same type as this object, then the string be:(furniture.objectType)_NESW

        if (furnitureSprites.ContainsKey(spriteName) == false) {
            Debug.LogError("GetSpriteForInstalledObject -- No sprites with name: " + spriteName);
            return null;
        }

        return furnitureSprites[spriteName];
    }

    public Sprite GetSpriteForFurniture(string objectType) {
        if (furnitureSprites.ContainsKey(objectType)) {
            return furnitureSprites[objectType];
        }

        if (furnitureSprites.ContainsKey(objectType + "_")) {
            return furnitureSprites[objectType + "_"];
        }

        Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + objectType);
        return null;
    }
}
