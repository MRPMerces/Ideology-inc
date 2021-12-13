using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tabclick : MonoBehaviour
{
    TabManager tabManager;
    [SerializeField]
    GameObject content;
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        GameObject foreldre = transform.parent.gameObject;//use this and the following lines to retrieve the state of the relevant animation set
        GameObject besteforeldre = foreldre.transform.parent.gameObject;
        animator = besteforeldre.GetComponent<Animator>();
        animator.GetBool("State");
        animator.SetBool("State", false); //sets the tabs to closed, since it looks better to start with your menues closed
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickOnTab()
    { 
        if (FindObjectOfType<TabManager>().CurrentTab == content || animator.GetBool("State") == false)
        {   //swithces the bool which will cause the opening or closing animation to play.
            animator.SetBool("State", !(animator.GetBool("State")));
            Debug.Log("BOOOOOOOOOOOOOOOOOP " + content.name + " " + FindObjectOfType<TabManager>().CurrentTab.name);
        }
        
        Debug.Log("Clicked on " + gameObject.name);
        GameObject foreldre = transform.parent.gameObject;
        GameObject besteforeldre = foreldre.transform.parent.gameObject;
        besteforeldre.GetComponent<TabManager>().SetActiveTab(content); 
    }
}
