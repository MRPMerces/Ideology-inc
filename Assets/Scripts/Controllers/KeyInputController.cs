using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        if (Input.GetKeyDown(KeyCode.F1)) {
            TileSpriteController.tileSpriteController.enableBorder(false);
        }

        if (Input.GetKeyDown(KeyCode.F7)) {
            TileSpriteController.tileSpriteController.enableBorder(true);
        }
    }
}
