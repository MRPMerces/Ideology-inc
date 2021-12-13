using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttonclick1 : MonoBehaviour
{
    public Image World;
    public GameObject worldbutton;
    public GameObject NoteBook;
    [SerializeField]
    
    
    Animator animatorBook;
    Animator animatorMap;
    // Start is called before the first frame update
    void Start()
    {
        World.enabled = true;
        NoteBook.SetActive(true);
        animatorBook = NoteBook.GetComponent<Animator>();//defines an animator variable which i will later use to 
        animatorMap = worldbutton.GetComponent<Animator>(); //retrieve The current state of the animation, so that i can change it
    }

    // Update is called once per frame
    public void Update()
    {
       
    }

    public void openWorld(){
        //World.enabled = !World.enabled;
        bool Open = !animatorMap.GetBool("IsOpen"); //defines a bool based on the current state of the animation. 
        animatorMap.SetBool("IsOpen", Open); //uses the previously defined bool to change the state of the animation.
    }

    public void OpenCloseNotebook()
    {
        //NoteBook.SetActive(!NoteBook.activeSelf);
        bool Active = !animatorBook.GetBool("State");//defines a bool based on the current state of the animation.
        animatorBook.SetBool("State", Active);      //uses the previously defined bool to change the state of the animation

    }

    
}
