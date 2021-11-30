using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class MouseController : MonoBehaviour {

    public GameObject dragObject;

    public static MouseController mouseController;

    BuildModeController buildModeController {
        get { return BuildModeController.buildModeController; }
    }

    // The world-position of the mouse last frame.
    Vector3 lastFramePosition;
    Vector3 currFramePosition;

    // The world-position start of our left-mouse drag operation
    Vector3 dragStartPosition;

    List<GameObject> dragPreviewGameObjects;


    bool isDragging = false;

    enum MouseMode { NONE, SELECT, BUILD }

    MouseMode currentMode = MouseMode.NONE;

    // Use this for initialization
    void Start() {
        mouseController = this;
        dragPreviewGameObjects = new List<GameObject>();
    }

    public Tile GetMouseOverTile() {
        return World.world.GetTileAt(currFramePosition);
    }

    // Update is called once per frame
    void Update() {
        currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currFramePosition.z = 0;

        switch (currentMode) {
            case MouseMode.NONE:
                break;

            case MouseMode.SELECT:
                // Clean up old drag previews
                cleanUpPreviews();

                if (Input.GetMouseButtonDown(0)) {
                    buildModeController.DoBuild(GetMouseOverTile());
                    currentMode = MouseMode.NONE;
                    break;
                }

                // show the generic dragging visuals
                addPreview(GetMouseOverTile());

                break;

            case MouseMode.BUILD:
                UpdateDragging();
                break;

            default:
                Debug.LogError("Unrecognized currentMode");
                break;
        }

        if (Input.GetKeyUp(KeyCode.Escape)) {
            if (currentMode != MouseMode.NONE) {
                currentMode = MouseMode.NONE;
                buildModeController.setBuildMode("NONE");

                // Clean up old drag previews
                cleanUpPreviews();
            }

            /// What to do?
            else if (currentMode == MouseMode.NONE) {
                Debug.Log("Show game menu?");
            }
        }

        UpdateCameraMovement();

        // Save the mouse position from this frame
        // We don't use currFramePosition because we may have moved the camera.
        lastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        lastFramePosition.z = 0;
    }

    void UpdateDragging() {
        // If we're over a UI element, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject()) { // || (currFramePosition == lastFramePosition && isDragging)
            return;
        }

        // Clean up old drag previews
        cleanUpPreviews();

        // Start Drag
        if (Input.GetMouseButtonDown(0)) {
            dragStartPosition = currFramePosition;
            isDragging = true;
        }

        else if (!isDragging) {
            dragStartPosition = currFramePosition;
        }

        if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape)) {
            // The RIGHT mouse button was released, so we
            // are cancelling any dragging/build mode.
            isDragging = false;
        }

        if (!buildModeController.IsObjectDraggable()) {
            dragStartPosition = currFramePosition;
        }

        int start_x = Mathf.FloorToInt(dragStartPosition.x + 0.5f);
        int start_y = Mathf.FloorToInt(dragStartPosition.y + 0.5f);
        int end_x = Mathf.FloorToInt(currFramePosition.x + 0.5f);
        int end_y = Mathf.FloorToInt(currFramePosition.y + 0.5f);

        // We may be dragging in the "wrong" direction, so flip things if needed.
        if (end_x < start_x) {
            int tmp = end_x;
            end_x = start_x;
            start_x = tmp;
        }

        if (end_y < start_y) {
            int tmp = end_y;
            end_y = start_y;
            start_y = tmp;
        }

        // Display a preview of the drag area
        for (int x = start_x; x <= end_x; x++) {
            for (int y = start_y; y <= end_y; y++) {
                Tile tile = World.world.GetTileAt(x, y);
                if (tile != null) {
                    // Display the building hint on top of this tile position

                    if (buildModeController.buildMode == BuildMode.FURNITURE) {
                        ShowFurnitureSpriteAtTile(buildModeController.buildModeObjectType, tile);
                    }

                    else {
                        // show the generic dragging visuals
                        addPreview(tile);
                    }
                }
            }
        }

        // End Drag
        if (isDragging && Input.GetMouseButtonUp(0)) {
            isDragging = false;

            List<Tile> tiles = new List<Tile>();

            // Loop through all the tiles
            for (int x = start_x; x <= end_x; x++) {
                for (int y = start_y; y <= end_y; y++) {
                    Tile tile = World.world.GetTileAt(x, y);

                    if (tile != null) {
                        tiles.Add(tile);
                    }
                }
            }
            buildModeController.DoBuild(tiles);
        }
    }

    void cleanUpPreviews() {
        while (dragPreviewGameObjects.Count > 0) {
            SimplePool.Despawn(dragPreviewGameObjects[0]);
            dragPreviewGameObjects.RemoveAt(0);
        }
    }

    void addPreview(Tile tile) {
        GameObject gameObject = SimplePool.Spawn(dragObject, new Vector3(tile.x, tile.y, 0), Quaternion.identity);
        gameObject.transform.SetParent(transform, true);
        dragPreviewGameObjects.Add(gameObject);
    }

    void UpdateCameraMovement() {
        // Handle screen panning

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
            // Right or Middle Mouse Button was pressed
            Camera.main.transform.Translate(lastFramePosition - currFramePosition);
        }

        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f);
    }

    void ShowFurnitureSpriteAtTile(string furnitureType, Tile tile) {

        GameObject gameobject = new GameObject();
        gameobject.transform.SetParent(transform, true);

        SpriteRenderer spriteRenderer = gameobject.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = "Jobs";
        spriteRenderer.sprite = FurnitureSpriteController.furnitureSpriteController.GetSpriteForFurniture(furnitureType);

        if (World.world.IsFurniturePlacementValid(furnitureType, tile)) {
            spriteRenderer.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        }

        else {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.25f);
        }

        Furniture proto = World.world.furniturePrototypes[furnitureType];

        gameobject.transform.position = new Vector3(tile.x + ((proto.Width - 1) / 2f), tile.y + ((proto.Height - 1) / 2f), 0);

        dragPreviewGameObjects.Add(gameobject);
    }

    public void StartBuildMode() {
        currentMode = MouseMode.BUILD;
    }

    public void StartSelectMode() {
        currentMode = MouseMode.SELECT;
    }
}


/// mode Select is meant for selecting Ui as apposed to selecting a tile to doBuild. This is just a naming error, but should be fixed. Ritght now the "currentMode" doesn't impact UI.
/// 