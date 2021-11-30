using System.Collections.Generic;
using UnityEngine;

public class InventoryManager {

    // This is a list of all "live" inventories.
    // Later on this will likely be organized by rooms instead
    // of a single master list. (Or in addition to.)
    public Dictionary<string, List<Inventory>> inventories;

    public InventoryManager() {
        inventories = new Dictionary<string, List<Inventory>>();
    }

    void CleanupInventory(Inventory inventory) {
        if (inventory.stackSize == 0) {
            if (inventories.ContainsKey(inventory.objectType)) {
                inventories[inventory.objectType].Remove(inventory);
            }

            if (inventory.tile != null) {
                inventory.tile.inventory = null;
                inventory.tile = null;
            }

            if (inventory.character != null) {
                inventory.character.inventory = null;
                inventory.character = null;
            }
        }
    }

    public bool PlaceInventory(Tile tile, Inventory inventory) {

        bool tileWasEmpty = tile.inventory == null; // smiplify??

        if (!tile.PlaceInventory(inventory)) {
            // The tile did not accept the inventory for whatever reason, therefore stop.
            return false;
        }

        CleanupInventory(inventory);

        // We may also created a new stack on the tile, if the tile was previously empty.
        if (tileWasEmpty) {
            if (inventories.ContainsKey(tile.inventory.objectType) == false) {
                inventories[tile.inventory.objectType] = new List<Inventory>();
            }

            inventories[tile.inventory.objectType].Add(tile.inventory);

            World.world.OnInventoryCreated(tile.inventory);
        }

        return true;
    }

    public bool PlaceInventory(Job job, Inventory inventory) {
        if (job.inventoryRequirements.ContainsKey(inventory.objectType) == false) {
            Debug.LogError("Trying to add inventory to a job that it doesn't want.");
            return false;
        }

        job.inventoryRequirements[inventory.objectType].stackSize += inventory.stackSize;

        if (job.inventoryRequirements[inventory.objectType].maxStackSize < job.inventoryRequirements[inventory.objectType].stackSize) {
            inventory.stackSize = job.inventoryRequirements[inventory.objectType].stackSize - job.inventoryRequirements[inventory.objectType].maxStackSize;
            job.inventoryRequirements[inventory.objectType].stackSize = job.inventoryRequirements[inventory.objectType].maxStackSize;
        }

        else {
            inventory.stackSize = 0;
        }

        CleanupInventory(inventory);

        return true;
    }

    public bool PlaceInventory(Character character, Inventory sourceInventory, int amount = -1) {
        if (amount < 0) {
            amount = sourceInventory.stackSize;
        }

        else {
            amount = Mathf.Min(amount, sourceInventory.stackSize);
        }

        if (character.inventory == null) {
            character.inventory = sourceInventory.Clone();
            character.inventory.stackSize = 0;
            inventories[character.inventory.objectType].Add(character.inventory);
        }

        else if (character.inventory.objectType != sourceInventory.objectType) {
            Debug.LogError("Character is trying to pick up a mismatched inventory object type.");
            return false;
        }

        character.inventory.stackSize += amount;

        if (character.inventory.maxStackSize < character.inventory.stackSize) {
            sourceInventory.stackSize = character.inventory.stackSize - character.inventory.maxStackSize;
            character.inventory.stackSize = character.inventory.maxStackSize;
        }

        else {
            sourceInventory.stackSize -= amount;
        }

        CleanupInventory(sourceInventory);

        return true;
    }

    /// <summary>
    /// Gets the type of the closest inventory of.
    /// </summary>
    /// <returns>The closest inventory of type.</returns>
    /// <param name="objectType">Object type.</param>
    /// <param name="tile">T.</param>
    /// <param name="desiredAmount">Desired amount. If no stack has enough, it instead returns the largest</param>
    public Inventory GetClosestInventoryOfType(string objectType, Tile tile, int desiredAmount, bool canTakeFromStockpile) {
        // FIXME:
        //   a) We are LYING about returning the closest item.
        //   b) There's no way to return the closest item in an optimal manner
        //      until our "inventories" database is more sophisticated.
        //		(i.e. seperate tile inventory from character inventory and maybe
        //		 has room content optimization.)

        if (!inventories.ContainsKey(objectType)) {
            Debug.LogError("GetClosestInventoryOfType -- no items of desired type.");
            return null;
        }

        foreach (Inventory inventory in inventories[objectType]) {
            if (inventory.tile != null && (canTakeFromStockpile || inventory.tile.furniture == null || !inventory.tile.furniture.IsStockpile())) {
                return inventory;
            }
        }

        return null;
    }
}
