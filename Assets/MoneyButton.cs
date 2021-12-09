using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class MoneyButton : MonoBehaviour
{
    public GameObject notebook, finance;
    public TabManager manager;

    public Text amounttext;
    public Text but1inc, but2inc, but1exp, but2exp, totinc, totexp;

    int amountbut1inc = 0;
    int amountbut2inc = 0;
    int amountbut1exp = 0;
    int amountbut2exp = 0;
    int amountmoney = 0;
    
    int amounttotinc = 0;
    int amounttotexp = 0;

    void Start()
    {
        amounttext.text = "$  " + amountmoney.ToString();
        but1inc.text = amountbut1inc.ToString();
        but2inc.text = amountbut2inc.ToString();
        but1exp.text = amountbut1exp.ToString();
        but2exp.text = amountbut2exp.ToString();
        totinc.text = amounttotinc.ToString();
        totexp.text = amounttotexp.ToString();
    }



    public void OpenFinance()
    {
        notebook.SetActive(true);
        manager.SetActiveTab(finance);
    }

    public void Sell1()
    {
        int number = Random.Range(0,50);
        amounttext.text = "$  " + (amountmoney += number).ToString();

        but1inc.text = "  " + (amountbut1inc += number).ToString();
        
        amounttotinc += number;
        totinc.text = amounttotinc.ToString();
    }

    public void Buy1()
    {
        int number = Random.Range(0,50);
        amounttext.text = "$  " + (amountmoney -= number).ToString();
        
        but1exp.text = "-  " + (amountbut1exp += number).ToString();

        amounttotexp += number;
        totexp.text = amounttotexp.ToString();
    }

    public void Sell2()
    {
        int number = Random.Range(0,50);
        amounttext.text = "$  " + (amountmoney += number).ToString();
        
        but2inc.text = "  " + (amountbut2inc += number).ToString();
        
        amounttotinc += number;
        totinc.text = amounttotinc.ToString();
    }

    public void Buy2()
    {
        int number = Random.Range(0,50);
        amounttext.text = "$  " + (amountmoney -= number).ToString();
       
        but2exp.text = "-  " + (amountbut2exp += number).ToString();

        amounttotexp += number;
        totexp.text = amounttotexp.ToString();
    }
}
