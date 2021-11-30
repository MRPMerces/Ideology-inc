using UnityEngine;
using UnityEngine.UI;

public class MouseOverRoomDetails : MonoBehaviour {

    // Every frame, this script checks to see which tile
    // is under the mouse and then updates the GetComponent<Text>.text
    // parameter of the object it is attached to.

    Text myText;
    MouseController mouseController {
        get {
            return MouseController.mouseController;
        }
    }

    // Use this for initialization
    void Start() {
        myText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update() {
        myText.text = "Roomtype: ";
        Tile tile = mouseController.GetMouseOverTile();

        if (tile == null || tile.room == null) {
            return;
        }

        myText.text = "Roomtype: " + tile.room.roomType.ToString();
    }
}
