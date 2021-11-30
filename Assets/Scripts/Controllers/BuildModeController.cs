using System;
using UnityEngine;
using System.Collections.Generic;

public enum BuildMode { NONE, TILE, FURNITURE, DECONSTRUCT, ROOM, CHARACTER }

public class BuildModeController : MonoBehaviour {
    public static BuildModeController buildModeController;

    MouseController mouseController {
        get {
            return MouseController.mouseController;
        }
    }

    public BuildMode buildMode { get; protected set; }

    TileType buildModeTile = TileType.Floor;

    public string buildModeObjectType;

    private void Start() {
        buildModeController = this;
        buildMode = BuildMode.NONE;
    }

    public bool IsObjectDraggable() {
        if (buildMode == BuildMode.TILE || buildMode == BuildMode.DECONSTRUCT || buildMode == BuildMode.ROOM) {
            // floors are draggable
            return true;
        }

        Furniture proto = World.world.furniturePrototypes[buildModeObjectType];

        return proto.Width == 1 && proto.Height == 1;
    }

    public void setBuildMode(string name) {
        buildMode = (BuildMode)Enum.Parse(typeof(BuildMode), name, true);

        if (buildMode == BuildMode.NONE) {
            return;
        }

        else if (buildMode == BuildMode.CHARACTER) {
            mouseController.StartSelectMode();
            return;
        }

        else if (buildMode == BuildMode.TILE) {
            Debug.LogWarning("buildMode set to TILE, but no tileType is given. Use BuildModeController::setBuildModeTile instead.");
            return;
        }

        else if (buildMode == BuildMode.FURNITURE) {
            Debug.LogWarning("buildMode set to FURNITURE, but no objecttype is given. Use BuildModeController::SetMode_BuildFurniture instead.");
            return;
        }

        else if (buildMode == BuildMode.DECONSTRUCT || buildMode == BuildMode.ROOM) { }

        else {
            Debug.LogError("Unrecognized BuildMode" + buildMode.ToString());
            return;
        }

        mouseController.StartBuildMode();
    }

    public void setBuildModeTile(string tileTypeMode) {
        buildModeTile = (TileType)Enum.Parse(typeof(TileType), tileTypeMode, true);
        buildMode = BuildMode.TILE;
        mouseController.StartBuildMode();
    }

    public void setBuildModeTile(TileType tileTypeMode) {
        buildModeTile = tileTypeMode;
        buildMode = BuildMode.TILE;
        mouseController.StartBuildMode();
    }

    public void SetMode_BuildFurniture(string objectType) {
        // Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
        buildMode = BuildMode.FURNITURE;
        buildModeObjectType = objectType;
        mouseController.StartBuildMode();
    }

    public void DoPathfindingTest() {
        World.world.SetupPathfindingExample();
    }

    public void DoBuild(Tile tile) {
        switch (buildMode) {
            case BuildMode.CHARACTER:
                World.world.CreateCharacter(tile);
                buildMode = BuildMode.NONE;
                return;

            default:
                return;
        }
    }

    public void DoBuild(List<Tile> tiles) {
        switch (buildMode) {
            case BuildMode.NONE:
                Debug.LogError("buildMode set to NONE");
                return;

            case BuildMode.TILE:
                // We are in tile-changing mode.
                foreach (Tile tile in tiles) {
                    tile.Type = buildModeTile;
                }

                return;

            case BuildMode.FURNITURE:
                // Create the Furniture and assign it to the tile
                foreach (Tile tile in tiles) {
                    if (World.world.IsFurniturePlacementValid(buildModeObjectType, tile) && tile.pendingFurnitureJob == null) {
                        // This tile position is valid for this furniture
                        // Create a job for it to be build

                        Job job;

                        if (World.world.furnitureJobPrototypes.ContainsKey(buildModeObjectType)) {
                            // Make a clone of the job prototype
                            job = World.world.furnitureJobPrototypes[buildModeObjectType].Clone();
                            // Assign the correct tile.
                            job.tile = tile;
                        }

                        else {
                            Debug.LogError("There is no furniture job prototype for '" + buildModeObjectType + "'");
                            job = new Job(tile, buildModeObjectType, FurnitureActions.JobComplete_FurnitureBuilding, 0.1f, null);
                        }

                        job.furniturePrototype = World.world.furniturePrototypes[buildModeObjectType];

                        tile.pendingFurnitureJob = job;
                        job.RegisterJobStoppedCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

                        // Add the job to the queue
                        World.world.jobQueue.Enqueue(job);
                    }
                }
                return;

            case BuildMode.DECONSTRUCT:
                foreach (Tile tile in tiles) {
                    if (tile.furniture != null) {
                        tile.furniture.Deconstruct();
                    }
                }

                return;

            case BuildMode.ROOM:
                RoomController.roomController.AddRoom(new Room(tiles, RoomType.Office));
                return;

            default:
                Debug.LogError("Unrecognized BuildMode" + buildMode.ToString());
                return;
        }
    }
}
