using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour {
    public static MouseController mouseController { get; protected set; }

    private BuildModeController buildModeController {
        get { return BuildModeController.buildModeController; }
    }

    public GameObject circleCursorPrefab;

    // The world-position of the mouse last frame.
    Vector3 lastFramePosition;
    Vector3 currFramePosition;

    // The world-position start of our left-mouse drag operation
    Vector3 dragStartPosition;

    List<GameObject> dragPreviewGameObjects;
    List<GameObject> hoverPreviewGameObjects;

    List<Tile> dragTiles;

    bool canceled = false;

    // The currently hovered tile
    // NOTE migth be null!
    public Tile currentHoveredTile {
        get {
            int x = Mathf.FloorToInt(currFramePosition.x + 0.5f);
            int y = Mathf.FloorToInt(currFramePosition.y + 0.5f);

            return World.world.getTileAt(x, y);
        }
    }

    // Use this for initialization
    void Start() {
        dragPreviewGameObjects = new List<GameObject>();

        mouseController = this;
        hoverPreviewGameObjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update() {
        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        UpdateMouseHover();
        UpdateCameraMovement();

        // Save the mouse position from this frame
        // We don't use currFramePosition because we may have moved the camera.
        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;
    }

    void UpdateMouseHover() {

        //Clean up old tile previews
        while (hoverPreviewGameObjects.Count > 0) {
            SimplePool.Despawn(hoverPreviewGameObjects[0]);
            hoverPreviewGameObjects.RemoveAt(0);
        }
    }

    void UpdateCameraMovement() {
        // Handle screen panning
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {   // Right or Middle Mouse Button

            Vector3 difference = lastFramePosition - currFramePosition;
            Camera.main.transform.Translate(difference);
        }

        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f);
    }

    public void dragInfrastructure() {
        // If we're over a UI element, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject()) {
            return;
        }

        // Clean up old drag previews
        while (dragPreviewGameObjects.Count > 0) {
            SimplePool.Despawn(dragPreviewGameObjects[0]);
            dragPreviewGameObjects.RemoveAt(0);
        }

        // If the player presses escape. Quit the drag function.
        if (Input.GetKeyDown(KeyCode.Escape)) {
            buildModeController.doBuild(null);
        }

        // quit Drag
        if (Input.GetMouseButton(1)) {
            dragStartPosition = currFramePosition;
            canceled = true;
            return;
        }

        // Start Drag
        if (Input.GetMouseButtonDown(0)) {
            dragStartPosition = currFramePosition;
            canceled = false;
        }

        int start_x = Mathf.FloorToInt(dragStartPosition.x + 0.5f);
        int start_y = Mathf.FloorToInt(dragStartPosition.y + 0.5f);
        int end_x = Mathf.FloorToInt(currFramePosition.x + 0.5f);
        int end_y = Mathf.FloorToInt(currFramePosition.y + 0.5f);

        // We may be dragging in the "wrong" direction, so flip things if needed.
        if (end_x < start_x) {
            int temp = end_x;
            end_x = start_x;
            start_x = temp;
        }
        if (end_y < start_y) {
            int temp = end_y;
            end_y = start_y;
            start_y = temp;
        }

        // end_x is always greater than start_x same with y

        if (!canceled && Input.GetMouseButton(0)) {

            dragTiles = new List<Tile>();

            createPreview(World.world.getTileAt(start_x, start_y));

            // Display a preview of the drag area
            if (start_x != end_x) {
                for (int x = start_x; x <= end_x; x++) {
                    createPreview(World.world.getTileAt(x, start_y));
                }
            }

            else if (start_y != end_y) {
                for (int y = start_y; y <= end_y; y++) {
                    createPreview(World.world.getTileAt(start_x, y));
                }
            }
        }

        // End Drag
        if (!canceled && Input.GetMouseButtonUp(0) && dragTiles.Count > 0) {
            buildModeController.doBuild(dragTiles.ToArray());
        }
    }

    void createPreview(Tile tile) {
        if (tile != null) {
            // Display the building hint on top of this tile position
            GameObject gameObject = SimplePool.Spawn(circleCursorPrefab, new Vector3(tile.X, tile.Y, 0), Quaternion.identity);
            gameObject.transform.SetParent(this.transform, true);
            gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "UI";
            dragPreviewGameObjects.Add(gameObject);
            dragTiles.Add(tile);
        }
    }
}

/// update the color of the cursorcircle based on can we build?