using UnityEngine;
using UnityEngine.UI;

public class MouseOverTileTypeText : MonoBehaviour {

    // Every frame, this script checks to see which tile
    // is under the mouse and then updates the GetComponent<Text>.text
    // parameter of the object it is attached to.

    Text myText;
    MouseController mouseController;

    // Use this for initialization
    void Start() {
        myText = GetComponent<Text>();

        if (myText == null) {
            Debug.LogError("MouseOverTileTypeText: No 'Text' UI component on this object.");
            enabled = false;
            return;
        }

        mouseController = GameObject.FindObjectOfType<MouseController>();
        if (mouseController == null) {
            Debug.LogError("How do we not have an instance of mouse controller?");
            return;
        }
    }

    // Update is called once per frame
    void Update() {
        Tile tile = mouseController.GetMouseOverTile();
        if (tile != null) {
            myText.text = "Tile Type: " + tile.Type.ToString();
        }
    }
}
