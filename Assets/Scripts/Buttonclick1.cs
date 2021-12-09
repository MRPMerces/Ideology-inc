using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Buttonclick1 : MonoBehaviour
{
    public Image World;
    [SerializeField]
    GameObject NoteBook;
    // Start is called before the first frame update
    void Start()
    {
        World.enabled = false;
        NoteBook.SetActive(false);
    }

    // Update is called once per frame
    public void Update()
    {
       
    }

    public void openWorld(){
        World.enabled = !World.enabled;
    }

    public void OpenCloseNotebook()
    {
        NoteBook.SetActive(!NoteBook.activeSelf);
    }

    
}
