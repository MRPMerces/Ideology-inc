using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Book : MonoBehaviour
{
    [SerializeField]
    GameObject closebook, openbook, newgame, setting;

    // Start is called before the first frame update
    void Start()
    {
        openbook.SetActive(false);

        closebook.SetActive(true);

        newgame.SetActive(false);

        setting.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenBook(){
        Debug.Log(openbook.name + " " + closebook.name);
        openbook.SetActive(true);
        closebook.SetActive(false);
    }
}
