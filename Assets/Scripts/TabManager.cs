using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TabManager : MonoBehaviour
{
    [SerializeField]
    GameObject staff;
    [SerializeField]
    GameObject finance;
    [SerializeField]
    GameObject followers;
    [SerializeField]
    GameObject program;
    [SerializeField]
    GameObject needs;
    [SerializeField]
    GameObject grants;
    [SerializeField]
    GameObject upgrades;

    List<GameObject> tabs;
    public GameObject CurrentTab;
    // Start is called before the first frame update
    void Start()
    {
        tabs = new List<GameObject> {staff, finance, followers, program, needs, grants, upgrades};
        CurrentTab = tabs[0];
        SetActiveTab(staff);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActiveTab(GameObject currentTab){
        foreach (GameObject tab in tabs)
        {
            tab.SetActive(false); 
            //Debug.Log(tab.name + " " + tab.activeSelf);  
        }
        Debug.Log("current tab " + currentTab.name);
        CurrentTab = currentTab;
        currentTab.SetActive(true);
    }
}
