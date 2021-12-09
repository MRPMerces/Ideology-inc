using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField]
    GameObject newgame, setting, menu;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NewGame(){
        Debug.Log("NewGame");
        menu.SetActive(false);
        newgame.SetActive(true);
    }

    public void LoadGame(){
        Debug.Log("LoadGame");
    }

    public void SaveGame(){
        Debug.Log("SaveGame");
    }

    public void Settings(){
        Debug.Log("Settings");
        menu.SetActive(false);
        setting.SetActive(true);
    }

    public void Quit(){
        Debug.Log("Quit");
        Application.Quit();
    }
}
