using UnityEngine;
using UnityEngine.UI;

public enum BuildMode { None };
public class BuildModeController : MonoBehaviour {
    public static BuildModeController buildModeController { get; protected set; }

    public Button roadButton;

    private BuildMode buildMode = BuildMode.None;

    private Tile currentTile {
        get { return MouseController.mouseController.currentHoveredTile; }
    }

    void Start() {
        buildModeController = this;
    }

    // Update is called once per frame
    void Update() {

        ColorBlock colorBlock = new ColorBlock {
            normalColor = Color.white,
            pressedColor = Color.green,
            highlightedColor = Color.gray,
            disabledColor = Color.red,
            colorMultiplier = 1f,
            fadeDuration = 1f
        };

        roadButton.colors = colorBlock;

        roadButton.enabled = true;

        if (buildMode != BuildMode.None){
            MouseController.mouseController.dragInfrastructure();
        }
    }

    /// <summary>
    /// Function to set the buildMode from a int.
    /// Can be used by buttons with the mode parameter set in th inspector.
    /// </summary>
    /// <param name="mode">int parameter to be casted to a BuildMode and applied to buildMode</param>
    public void setBuildMode(int mode) {
        if (buildMode == BuildMode.None) {
            buildMode = (BuildMode) mode;
        }
    }

    public void doBuild(Tile[] tiles) {

        // tiles will be null if the user cancels the build, before actually building something.
        if (tiles != null) {
            
        }
        buildMode = BuildMode.None;
    }
}
