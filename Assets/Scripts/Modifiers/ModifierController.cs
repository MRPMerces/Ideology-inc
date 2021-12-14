using UnityEngine;

public class ModifierController : MonoBehaviour {

    public static ModifierController modifierController;

    public Modifier constructionCost { get; protected set; }
    public Modifier operatingIncome { get; protected set; }
    public Modifier workSpeed { get; protected set; }

    private void OnEnable() {
        modifierController = this;
        constructionCost = new Modifier(ModifierType.ConstructionCost);
        operatingIncome = new Modifier(ModifierType.OperatingIncome);
        workSpeed = new Modifier(ModifierType.WorkSpeed);
    }

    void resetModifiers() {
        constructionCost.value = 1f;
        operatingIncome.value = 1f;
        workSpeed.value = 1f;
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

            case ModifierType.WorkSpeed:
                workSpeed.value += modifier.value;

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
