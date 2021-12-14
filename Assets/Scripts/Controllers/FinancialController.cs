using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FinancialController : MonoBehaviour {
    public static FinancialController financialController;

    private float actualMoney;
    public int money {
        get {
            return (int)actualMoney;
        }

        protected set {
            Debug.LogWarning("This doesn't do anything!");
        }
    }

    // Start is called before the first frame update
    void Start() {
        financialController = this;
        actualMoney = 100f;
    }

    void add_money(int amount) {
        if (amount > 0) {
            actualMoney += (amount * ModifierController.modifierController.operatingIncome.value);
        }

        if (amount < 0) {
            actualMoney += amount;
        }

        cbMoneyUpdated?.Invoke();
    }

    void add_money(float amount) {
        if (amount > 0) {
            actualMoney += amount * ModifierController.modifierController.operatingIncome.value;

        }

        if (amount < 0) {
            actualMoney += amount;
        }

        cbMoneyUpdated?.Invoke();
    }

    /// <summary>
    /// Subtract construction cost from balance.
    /// Use this if the constructionCost modifier should be applied.
    /// </summary>
    public void constructionCost(int amount) {
        float expendeture = amount * ModifierController.modifierController.constructionCost.value;
        if (actualMoney - expendeture < 0) {
            Debug.LogError("-Player::constructionCost- Insufficient funds!");
            return;
        }

        actualMoney -= expendeture;
        Debug.Log("- " + expendeture + " Money");
    }

    /// <summary>
    /// Subtract construction cost from balance.
    /// Use this if the constructionCost modifier should be applied.
    /// </summary>
    public void constructionCost(float amount) {
        float expendeture = amount * ModifierController.modifierController.constructionCost.value;
        if (actualMoney - expendeture < 0) {
            Debug.LogError("-Player::constructionCost- Insufficient funds!");
            return;
        }

        actualMoney -= expendeture;
        Debug.Log("- " + expendeture + " Money");
    }

    /// <summary>
    /// Method to check whenever a player can afford the construction cost.
    /// </summary>
    /// <param name="amount">The constructioncost to test</param>
    /// <returns>ool can/ can't afford</returns>
    public bool canAffordConstructionCost(int amount) {
        return amount * ModifierController.modifierController.constructionCost.value <= actualMoney;
    }

    /// <summary>
    /// Method to check whenever a player can afford the construction cost.
    /// </summary>
    /// <param name="amount">The constructioncost to test</param>
    /// <returns>ool can/ can't afford</returns>
    public bool canAfford(float amount) {
        return amount * ModifierController.modifierController.constructionCost.value <= actualMoney;
    }

    // The function we callback any time player.human.money updates changes
    Action cbMoneyUpdated;

    /// <summary>
    /// Register a function to be called back when our tile type changes.
    /// </summary>
    public void RegisterMoneyUpdatedCallback(Action callback) {
        cbMoneyUpdated += callback;
    }

    /// <summary>
    /// Unregister a callback.
    /// </summary>
    public void UnregisterMoneyUpdatedCallback(Action callback) {
        cbMoneyUpdated -= callback;
    }
}
