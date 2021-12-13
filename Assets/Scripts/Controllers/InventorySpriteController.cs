using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventorySpriteController : MonoBehaviour {

    public GameObject inventoryUIPrefab;

    Dictionary<Inventory, GameObject> inventoryGameObjectMap;

    Dictionary<string, Sprite> inventorySprites;

    // Use this for initialization
    void Start() {
        LoadSprites();

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        World.world.RegisterInventoryCreated(OnInventoryCreated);

        // Check for pre-existing inventory, which won't do the callback.
        foreach (string objectType in World.world.inventoryManager.inventories.Keys) {
            foreach (Inventory inventory in World.world.inventoryManager.inventories[objectType]) {
                OnInventoryCreated(inventory);
            }
        }


        //c.SetDestination( world.GetTileAt( world.Width/2 + 5, world.Height/2 ) );
    }

    void LoadSprites() {
        inventorySprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Inventory/");

        //Debug.Log("LOADED RESOURCE:");
        foreach (Sprite sprite in sprites) {
            //Debug.Log(s);
            inventorySprites[sprite.name] = sprite;
        }
    }

    public void OnInventoryCreated(Inventory inventory) {
        //Debug.Log("OnInventoryCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject gameObject = new GameObject();

        // Add our tile/GO pair to the dictionary.
        inventoryGameObjectMap.Add(inventory, gameObject);

        gameObject.name = inventory.objectType;
        gameObject.transform.position = new Vector3(inventory.tile.x, inventory.tile.y, 0);
        gameObject.transform.SetParent(transform, true);

        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = inventorySprites[inventory.objectType];
        spriteRenderer.sortingLayerName = "Inventory";

        if (inventory.maxStackSize > 1) {
            // This is a stackable object, so let's add a InventoryUI component
            // (Which is text that shows the current stackSize.)

            GameObject uiGameObject = Instantiate(inventoryUIPrefab);
            uiGameObject.transform.SetParent(gameObject.transform);
            uiGameObject.transform.localPosition = Vector3.zero;
            uiGameObject.GetComponentInChildren<Text>().text = inventory.stackSize.ToString();
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        // FIXME: Add on changed callbacks
        inventory.RegisterChangedCallback(OnInventoryChanged);
    }

    void OnInventoryChanged(Inventory inventory) {

        //Debug.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (inventoryGameObjectMap.ContainsKey(inventory) == false) {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for inventory not in our map.");
            return;
        }

        GameObject gameObject = inventoryGameObjectMap[inventory];
        if (inventory.stackSize > 0) {
            Text text = gameObject.GetComponentInChildren<Text>();
            // FIXME: If maxStackSize changed to/from 1, then we either need to create or destroy the text
            if (text != null) {
                text.text = inventory.stackSize.ToString();
            }
        }

        else {
            // This stack has gone to zero, so remove the sprite!
            Destroy(gameObject);
            inventoryGameObjectMap.Remove(inventory);
            inventory.UnregisterChangedCallback(OnInventoryChanged);
        }
    }
}
