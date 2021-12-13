using System;

// Inventory are things that are lying on the floor/stockpile, like a bunch of metal bars
// or potentially a non-installed copy of furniture (e.g. a cabinet still in the box from Ikea)


public class Inventory {
    public string objectType = "Steel Plate";
    public int maxStackSize = 50;

    protected int _stackSize = 1;
    public int stackSize {
        get { return _stackSize; }
        set {
            if (_stackSize != value) {
                _stackSize = value;
                cbInventoryChanged?.Invoke(this);
            }
        }
    }

    // The function we callback any time our tile's data changes
    Action<Inventory> cbInventoryChanged;

    public Tile tile;
    public Character character;

    public Inventory() {

    }

    public Inventory(string objectType, int maxStackSize, int stackSize) {
        this.objectType = objectType;
        this.maxStackSize = maxStackSize;
        this.stackSize = stackSize;
    }

    protected Inventory(Inventory other) {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        stackSize = other.stackSize;
    }

    public virtual Inventory Clone() {
        return new Inventory(this);
    }

    public int itemsToMaxStackSize() {
        return maxStackSize - stackSize;
    }

    public void RegisterChangedCallback(Action<Inventory> callback) {
        cbInventoryChanged += callback;
    }

    public void UnregisterChangedCallback(Action<Inventory> callback) {
        cbInventoryChanged -= callback;
    }
}
