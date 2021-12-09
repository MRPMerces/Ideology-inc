using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tabclick : MonoBehaviour
{
    [SerializeField]
    GameObject content;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickOnTab()
    {
        Debug.Log("Clicked on " + gameObject.name);
        GameObject foreldre = transform.parent.gameObject;
        GameObject besteforeldre = foreldre.transform.parent.gameObject;
        besteforeldre.GetComponent<TabManager>().SetActiveTab(content);
    }
}
