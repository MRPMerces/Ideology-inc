using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyInputController : MonoBehaviour {
    public static KeyInputController keyInputController;
    public GameObject mainMenu;

    // Start is called before the first frame update
    void Start() {
        keyInputController = this;
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
            disableOverlays();
        }

        if (Input.GetKeyDown(KeyCode.F7)) {
            disableOverlays();
            TileSpriteController.tileSpriteController.enableBorder(true);
        }

        if (Input.GetKeyDown(KeyCode.F6)) {
            disableOverlays();
            TileSpriteController.tileSpriteController.enableBorder(true);
            RoomSpriteController.roomSpriteController.enableRoomOverlay(true);
        }
    }

    public void disableOverlays() {
        TileSpriteController.tileSpriteController.enableBorder(false);
        RoomSpriteController.roomSpriteController.enableRoomOverlay(false);
    }
}
