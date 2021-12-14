using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuyUpgtrades : MonoBehaviour
{
    public static BuyUpgtrades buyUpgtrades;

    public int[] table { get; set; }
    public int[] followerTable { get; set; }



    public Text buildSpeedTxt;
    public double buildSpeed;

    public Text MoneyTxt;
    public double money;
    public double moneyAmp;


    public Text clickBuildUpgradeText;
    public Text clickMoneyUpgradeText;
    public Text clickFollowersUpgradeText;
    public Text clickCheaperUpgradeText;

    public double clickBuildUpgradeCost;
    public int clickBuildUpgradeLvl;

    public double moneyInc;
    public double moneyPerSec;
    public double clickMoneyUpgradeCost;
    public int clickMoneyUpgradeLvl;

    public double clickCheaperUpgradeCost;
    public int clickCheaperUpgradeLvl;


    public double clickFollowersUpgradeCost;
    public int clickFollowersUpgradeLvl;


    public Text followerText;
    public int total;
    public int randomNum;
    public int lvlCap = 5;
    public int followers = 1;
    public int T1;
    public int T2;
    public int T3;
    public int T4;
    float time;
    float time2;


    public void Start()
    {
        buyUpgtrades = this;

        table = new int[] { 80, 14, 5, 1 };
        followerTable = new int[] { 100, 2, 3, 10 };

        lvlCap = 5;
        followers = 1;
        T1 = 1;
        T2 = 2;
        T3 = 3;
        T4 = 10;
        time = 0.0f;
        time2 = 0.0f;


        moneyInc = 1;
        clickMoneyUpgradeCost = 25;
        clickMoneyUpgradeLvl = 0;

        buildSpeed = 1;
        clickBuildUpgradeCost = 10;
        clickBuildUpgradeLvl = 0;

        clickFollowersUpgradeCost = 50;
        clickFollowersUpgradeLvl = 0;

        clickCheaperUpgradeCost = 40;
        clickCheaperUpgradeLvl = 0;


    }

    public void Update()
    {

        //slightly increase money made each time based on the upgrade and amount of followers
        moneyAmp = 0.05 * followers;
        moneyPerSec = moneyInc + moneyAmp;

        //write over text objects to update and display updated and correct information
        MoneyTxt.text = "Money: " + money.ToString("F2") + "$";


        buildSpeedTxt.text = "Build speed: " + buildSpeed;

        //updateing text to show new prices which level the upgrade is
        clickBuildUpgradeText.text = "Cost: " + clickBuildUpgradeCost.ToString("F0") + "$ Level: " + clickBuildUpgradeLvl + "/5";
        clickMoneyUpgradeText.text = "Cost: " + clickMoneyUpgradeCost.ToString("F0") + "$ Level:" + clickMoneyUpgradeLvl + "/5";
        clickFollowersUpgradeText.text = "Cost: " + clickFollowersUpgradeCost.ToString("F0") + "$ Level:" + clickFollowersUpgradeLvl + "/5";
        clickCheaperUpgradeText.text = "Cost: " + clickCheaperUpgradeCost.ToString("F0") + "$ Level:" + clickCheaperUpgradeLvl + "/5";


        time += Time.deltaTime;
        time2 += Time.deltaTime;

        if (time >= 1f)
        {
            money += moneyPerSec;
            time -= 1f;

        }
        if (time2 >= 5f)
        {
            incFollowers();
            followerText.text = "Followers: " + followers;
            time2 -= 5f;

        }



    }


    public void BuyBuildSpeed()
    {
        if (clickBuildUpgradeLvl < lvlCap)
        {
            if (money >= clickBuildUpgradeCost)
            {
                clickBuildUpgradeLvl++;
                money -= clickBuildUpgradeCost;
                clickBuildUpgradeCost *= 1.85;
                buildSpeed *= 1.1;
            }
        }
    }

    public void BuyMoney()
    {
        if (clickMoneyUpgradeLvl < lvlCap)
        {
            if (money >= clickMoneyUpgradeCost)
            {

                clickMoneyUpgradeLvl++;
                moneyInc += moneyInc + (clickMoneyUpgradeLvl * 0.2);
                money -= clickMoneyUpgradeCost;
                clickMoneyUpgradeCost *= 1.90;
            }
        }
    }

    public void BuyFollowersUpgrade()
    {
        if (clickFollowersUpgradeLvl < lvlCap)
        {
            if (money >= clickFollowersUpgradeCost)
            {
                clickFollowersUpgradeLvl++;
                money -= clickFollowersUpgradeCost;
                clickFollowersUpgradeCost *= 1.47;
                T1 += 1; T2 += 1; T3 += 1;
                moneyInc *= 0.80;
            }
        }

    }

    public void BuyCheaperUpgrade()
    {
        if (clickCheaperUpgradeLvl < lvlCap)
        {
            if (money >= clickCheaperUpgradeCost)
            {
                clickCheaperUpgradeLvl++;
                money -= clickCheaperUpgradeCost;
                clickCheaperUpgradeCost *= 1.63;

                if (clickCheaperUpgradeCost >= 10 && clickFollowersUpgradeCost >= 10 && clickMoneyUpgradeCost >= 10 && clickBuildUpgradeCost >= 10)
                {
                    clickCheaperUpgradeCost *= 0.95;
                    clickFollowersUpgradeCost *= 0.95;
                    clickMoneyUpgradeCost *= 0.95;
                    clickBuildUpgradeCost *= 0.95;
                    T1 -= 1; T2 -= 1; T3 -= 1;
                }


            }
        }

    }



    public void incFollowers()
    {
        randomNum = 0;
        total = 0;
        followerTable[0] = T1;
        followerTable[1] = T2;
        followerTable[2] = T3;
        followerTable[3] = T4;



        foreach (var item in table)
        {
            total += item;
        }

        randomNum = Random.Range(0, total);

        for (var i = 0; i < table.Length; i++)
        {
            if (randomNum <= table[i])
            {

                followers += followerTable[i];
                return;
            }
            else
            {
                randomNum -= table[i];
            }
        }
        T1++; T2++; T3++; T4++;


    }



}
