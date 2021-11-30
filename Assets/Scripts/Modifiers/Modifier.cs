public enum ModifierType { ConstructionCost , OperatingIncome , BuildTime, PopulationGrowth }
public class Modifier {
    /// <summary>
    /// Constructor to create a new modifier.
    /// </summary>
    /// <param name="type">The type of modifier</param>
    /// <param name="value">The modifier value</param>
    public Modifier(ModifierType type, float value = 1f) {
        this.type = type;
        this.value = value;
    }

    public readonly ModifierType type;

    public float value;
}
