﻿using UnityEngine;

public class KeyInputController : MonoBehaviour {

    public GameObject mainMenu;
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (mainMenu.activeSelf) {
                mainMenu.SetActive(false);
            }
            else {
                mainMenu.SetActive(true);
            }
        }
    }
}
