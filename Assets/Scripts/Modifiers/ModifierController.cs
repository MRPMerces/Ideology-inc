using UnityEngine;

public class ModifierController : MonoBehaviour {

    public static ModifierController modifierController;

    private void OnEnable() {
        modifierController = this;
        constructionCost = new Modifier(ModifierType.ConstructionCost);
        operatingIncome = new Modifier(ModifierType.OperatingIncome);
        buildTime = new Modifier(ModifierType.BuildTime);
    }

    public Modifier constructionCost { get; protected set; }
    public Modifier operatingIncome { get; protected set; }
    public Modifier buildTime { get; protected set; }

    void resetModifiers() {
        constructionCost.value = 1f;
        operatingIncome.value = 1f;
        buildTime.value = 1f;
    }

    public void update_modifier(Modifier[] modifiers) {
        foreach (Modifier modifier in modifiers) {
            actuallyUpdate_modifier(modifier);
        }

        checkModifiers();
    }

    public void update_modifier(Modifier modifier) {
        actuallyUpdate_modifier(modifier);
        checkModifiers();
    }

    void actuallyUpdate_modifier(Modifier modifier) {
        switch (modifier.type) {
            case ModifierType.ConstructionCost:
                constructionCost.value += modifier.value;
                break;

            case ModifierType.OperatingIncome:
                operatingIncome.value += modifier.value;
                break;

            case ModifierType.BuildTime:
                buildTime.value += modifier.value;

                /// update global builtimes?
                break;

            default:
                Debug.LogError("Unknown modifier!");
                break;
        }
    }

    void checkModifiers() {
        if (constructionCost.value < 0) {
            constructionCost.value = 0;
        }
    }
}
