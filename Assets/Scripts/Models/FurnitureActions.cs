using UnityEngine;
using System.Linq;

public static class FurnitureActions {
    // This file contains code which will likely be completely moved to
    // some LUA files later on and will be parsed at run-time.

    public static void Door_UpdateAction(Furniture furniture, float deltaTime) {
        //Debug.Log("Door_UpdateAction: " + furn.furnParameters["openness"]);

        if (furniture.GetParameter("is_opening") >= 1) {
            furniture.ChangeParameter("openness", deltaTime * 4);    // FIXME: Maybe a door open speed parameter?
            if (furniture.GetParameter("openness") >= 1) {
                furniture.SetParameter("is_opening", 0);
            }
        }

        else {
            furniture.ChangeParameter("openness", deltaTime * -4);
        }

        furniture.SetParameter("openness", Mathf.Clamp01(furniture.GetParameter("openness")));

        furniture.cbOnChanged?.Invoke(furniture);
    }

    public static ENTERABILITY Door_IsEnterable(Furniture furn) {
        //Debug.Log("Door_IsEnterable");
        furn.SetParameter("is_opening", 1);

        if (furn.GetParameter("openness") >= 1) {
            return ENTERABILITY.Yes;
        }

        return ENTERABILITY.Soon;
    }

    public static void JobComplete_FurnitureBuilding(Job job) {
        World.world.PlaceFurniture(job.jobObjectType, job.tile);

        // FIXME: I don't like having to manually and explicitly set
        // flags that preven conflicts. It's too easy to forget to set/clear them!
        job.tile.pendingFurnitureJob = null;
    }

    public static Inventory[] Stockpile_GetItemsFromFilter() {
        // TODO: This should be reading from some kind of UI for this
        // particular stockpile

        // Since jobs copy arrays automatically, we could already have
        // an Inventory[] prepared and just return that (as a sort of example filter)

        return new Inventory[1] { new Inventory("Steel Plate", 50, 0) };
    }

    public static void Stockpile_UpdateAction(Furniture furniture, float deltaTime) {
        // We need to ensure that we have a job on the queue
        // asking for either:
        //  (if we are empty): That ANY loose inventory be brought to us.
        //  (if we have something): Then IF we are still below the max stack size,
        //						    that more of the same should be brought to us.

        // TODO: This function doesn't need to run each update.  Once we get a lot
        // of furniture in a running game, this will run a LOT more than required.
        // Instead, it only really needs to run whenever:
        //		-- It gets created
        //		-- A good gets delivered (at which point we reset the job)
        //		-- A good gets picked up (at which point we reset the job)
        //		-- The UI's filter of allowed items gets changed


        if (furniture.tile.inventory != null && furniture.tile.inventory.stackSize >= furniture.tile.inventory.maxStackSize) {
            // We are full!
            furniture.CancelJobs();
            return;
        }

        // Maybe we already have a job queued up?
        if (furniture.JobCount() > 0) {
            // Cool, all done.
            return;
        }

        // We currently are NOT full, but we don't have a job either.
        // Two possibilities: Either we have SOME inventory, or we have NO inventory.

        // Third possibility: Something is WHACK
        if (furniture.tile.inventory != null && furniture.tile.inventory.stackSize == 0) {
            Debug.LogError("Stockpile has a zero-size stack. This is clearly WRONG!");
            furniture.CancelJobs();
            return;
        }

        // TODO: In the future, stockpiles -- rather than being a bunch of individual
        // 1x1 tiles -- should manifest themselves as single, large objects.  This
        // would respresent our first and probably only VARIABLE sized "furniture" --
        // at what happenes if there's a "hole" in our stockpile because we have an
        // actual piece of furniture (like a cooking stating) installed in the middle
        // of our stockpile?
        // In any case, once we implement "mega stockpiles", then the job-creation system
        // could be a lot smarter, in that even if the stockpile has some stuff in it, it
        // can also still be requestion different object types in its job creation.

        Inventory[] itemsDesired;

        if (furniture.tile.inventory == null) {
            Debug.Log("Creating job for new stack.");
            itemsDesired = Stockpile_GetItemsFromFilter();
        }

        else {
            Debug.Log("Creating job for existing stack.");
            Inventory desInv = furniture.tile.inventory.Clone();
            desInv.maxStackSize -= desInv.stackSize;
            desInv.stackSize = 0;

            itemsDesired = new Inventory[] { desInv };
        }

        Job job = new Job(furniture.tile, null, null, 0, itemsDesired);

        // TODO: Later on, add stockpile priorities, so that we can take from a lower
        // priority stockpile for a higher priority one.
        job.canTakeFromStockpile = false;

        job.RegisterJobWorkedCallback(Stockpile_JobWorked);
        furniture.AddJob(job);
    }

    static void Stockpile_JobWorked(Job job) {
        Debug.Log("Stockpile_JobWorked");
        job.CancelJob();

        // TODO: Change this when we figure out what we're doing for the all/any pickup job.
        foreach (Inventory inventory in job.inventoryRequirements.Values) {
            if (inventory.stackSize > 0) {
                World.world.inventoryManager.PlaceInventory(job.tile, inventory);

                return;  // There should be no way that we ever end up with more than on inventory requirement with stackSize > 0
            }
        }
    }

    public static void SteelMill_UpdateAction(Furniture furniture, float deltaTime) {

        Tile spawnSpot = furniture.GetSpawnSpotTile();

        if (furniture.JobCount() > 0) {

            // Check to see if the Metal Plate destination tile is full.
            if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
                // We should stop this job, because it's impossible to make any more items.
                furniture.CancelJobs();
            }

            return;
        }

        // If we get here, then we have no current job. Check to see if our destination is full.
        if (spawnSpot.inventory != null && spawnSpot.inventory.stackSize >= spawnSpot.inventory.maxStackSize) {
            // We are full! Don't make a job!
            return;
        }

        // If we get here, we need to CREATE a new job.

        Tile tile = furniture.GetJobSpotTile();

        if (tile.inventory != null && (tile.inventory.stackSize >= tile.inventory.maxStackSize)) {
            // Our drop spot is already full, so don't create a job.
            return;
        }

        furniture.AddJob(new Job(tile, null, SteelMill_JobComplete, 1f, null, true));
    }

    public static void SteelMill_JobComplete(Job job) {
        if (FinancialController.financialController.canAffordConstructionCost(100)) {
            Debug.Log("hei");
            FinancialController.financialController.constructionCost(100);
            World.world.inventoryManager.PlaceInventory(job.furniture.GetSpawnSpotTile(), new Inventory("Steel Plate", 50, 1));
        }
    }
}
