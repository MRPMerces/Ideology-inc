using System;
using System.Collections.Generic;

public class Job {

    // This class holds info for a queued up job, which can include
    // things like placing furniture, moving stored inventory,
    // working at a desk, and maybe even fighting enemies.

    public Tile tile;
    public float jobTime { get; protected set; }

    protected float jobTimeRequired;

    protected bool jobRepeats = false;

    public string jobObjectType {
        get; protected set;
    }

    public Furniture furniturePrototype;

    public Furniture furniture; // The piece of furniture that owns this job. Frequently will be null.

    public bool acceptsAnyInventoryItem = false;

    Action<Job> cbJobCompleted; // We have finished the work cycle and so things should probably get built or whatever. 
    Action<Job> cbJobStopped;   // The job has been stopped, either because it's non-repeating or was cancelled.
    Action<Job> cbJobWorked;    // Gets called each time some work is performed -- maybe update the UI?

    public bool canTakeFromStockpile = true;

    public Dictionary<string, Inventory> inventoryRequirements;

    public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime, Inventory[] inventoryRequirements, bool jobRepeats = false) {
        this.tile = tile;
        this.jobObjectType = jobObjectType;
        cbJobCompleted += cbJobComplete;
        jobTimeRequired = this.jobTime = jobTime; // * ModifierController.modifierController.buildTime.value;
        this.jobRepeats = jobRepeats;

        this.inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null) {
            foreach (Inventory inventory in inventoryRequirements) {
                this.inventoryRequirements[inventory.objectType] = inventory.Clone();
            }
        }
    }

    protected Job(Job other) {
        tile = other.tile;
        jobObjectType = other.jobObjectType;
        cbJobCompleted = other.cbJobCompleted;
        jobTime = other.jobTime;

        inventoryRequirements = new Dictionary<string, Inventory>();
        if (inventoryRequirements != null) {
            foreach (Inventory inventory in other.inventoryRequirements.Values) {
                inventoryRequirements[inventory.objectType] = inventory.Clone();
            }
        }
    }

    virtual public Job Clone() {
        return new Job(this);
    }

    public void RegisterJobCompletedCallback(Action<Job> cb) {
        cbJobCompleted += cb;
    }

    public void RegisterJobStoppedCallback(Action<Job> cb) {
        cbJobStopped += cb;
    }

    public void UnregisterJobCompletedCallback(Action<Job> cb) {
        cbJobCompleted -= cb;
    }

    public void UnregisterJobStoppedCallback(Action<Job> cb) {
        cbJobStopped -= cb;
    }

    public void RegisterJobWorkedCallback(Action<Job> cb) {
        cbJobWorked += cb;
    }

    public void UnregisterJobWorkedCallback(Action<Job> cb) {
        cbJobWorked -= cb;
    }

    public void DoWork(float workTime) {
        // Check to make sure we actually have everything we need. 
        // If not, don't register the work time.
        if (!HasAllMaterial()) {
            //Debug.LogError("Tried to do work on a job that doesn't have all the material.");

            // Job can't actually be worked, but still call the callbacks
            // so that animations and whatnot can be updated.
            cbJobWorked?.Invoke(this);

            return;
        }

        jobTime -= workTime * ModifierController.modifierController.workSpeed.value;

        cbJobWorked?.Invoke(this);

        if (jobTime <= 0) {
            // Do whatever is supposed to happen with a job cycle completes.
            cbJobCompleted?.Invoke(this);

            if (!jobRepeats) {
                // Let everyone know that the job is officially concluded
                cbJobStopped?.Invoke(this);
            }

            else {
                // This is a repeating job and must be reset.
                jobTime += jobTimeRequired;
            }
        }
    }

    public void CancelJob() {
        cbJobStopped?.Invoke(this);

        World.world.jobQueue.Remove(this);
    }

    public bool HasAllMaterial() {
        foreach (Inventory inventory in inventoryRequirements.Values) {
            if (inventory.maxStackSize > inventory.stackSize) {
                return false;
            }
        }

        return true;
    }

    public int DesiresInventoryType(Inventory inventory) {
        if (acceptsAnyInventoryItem) {
            return inventory.maxStackSize;
        }

        if (!inventoryRequirements.ContainsKey(inventory.objectType)) {
            return 0;
        }

        if (inventoryRequirements[inventory.objectType].stackSize >= inventoryRequirements[inventory.objectType].maxStackSize) {
            // We already have all that we need!
            return 0;
        }

        // The inventory is of a type we want, and we still need more.
        return inventoryRequirements[inventory.objectType].maxStackSize - inventoryRequirements[inventory.objectType].stackSize;
    }

    public Inventory GetFirstDesiredInventory() {
        foreach (Inventory inventory in inventoryRequirements.Values) {
            if (inventory.maxStackSize > inventory.stackSize)
                return inventory;
        }

        return null;
    }
}
