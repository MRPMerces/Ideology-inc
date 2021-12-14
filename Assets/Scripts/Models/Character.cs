using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using UnityEngine;

public class Character : IXmlSerializable {

    public Character(Tile tile) {
        currentTile = destinationTile = nextTile = tile;
        happiness = 50;
        tiredness = 5;
    }

    public float happiness { get; protected set; }

    public float tiredness { get; protected set; }

    public float x {
        get {
            if (nextTile == null)
                return currentTile.x;

            return Mathf.Lerp(currentTile.x, nextTile.x, movementPercentage);
        }
    }

    public float y {
        get {
            if (nextTile == null)
                return currentTile.y;

            return Mathf.Lerp(currentTile.y, nextTile.y, movementPercentage);
        }
    }

    public Tile currentTile { get; protected set; }

    // If we aren't moving, then destTile = currTile
    Tile _destinationTile;

    Tile destinationTile {
        get { return _destinationTile; }
        set {
            if (_destinationTile != value) {
                _destinationTile = value;
                pathAStar = null;   // If this is a new destination, then we need to invalidate pathfinding.
            }
        }
    }

    Tile nextTile;  // The next tile in the pathfinding sequence
    Path_AStar pathAStar;
    float movementPercentage; // Goes from 0 to 1 as we move from currTile to destTile

    float speed = 5f;   // Tiles per second

    Action<Character> cbCharacterChanged;

    Job myJob;

    // The item we are carrying (not gear/equipment)
    public Inventory inventory;

    private Character() {
        // Use only for serialization
    }

    void GetNewJob() {
        myJob = World.world.jobQueue.Dequeue();

        if (myJob == null) {
            return;
        }

        destinationTile = myJob.tile;
        myJob.RegisterJobStoppedCallback(OnJobStopped);

        // Immediately check to see if the job tile is reachable.
        // NOTE: We might not be pathing to it right away (due to 
        // requiring materials), but we still need to verify that the
        // final location can be reached.

        pathAStar = new Path_AStar(currentTile, destinationTile);    // This will calculate a path from curr to dest.
        if (pathAStar.Length() == 0) {
            Debug.LogError("Path_AStar returned no path to target job tile!");
            AbandonJob();
            destinationTile = currentTile;
        }
    }

    void Update_DoJob(float deltaTime) {
        // Do I have a job?
        if (myJob == null) {
            GetNewJob();

            if (myJob == null) {
                // There was no job on the queue for us, so just return.
                destinationTile = currentTile;
                return;
            }
        }

        // We have a job! (And the job tile is reachable)

        // STEP 1: Does the job have all the materials it needs?
        if (!myJob.HasAllMaterial()) {
            // No, we are missing something!

            // STEP 2: Are we CARRYING anything that the job location wants?
            if (inventory != null) {
                if (myJob.DesiresInventoryType(inventory) > 0) {
                    // If so, deliver the goods.
                    //  Walk to the job tile, then drop off the stack into the job.
                    if (currentTile == myJob.tile) {
                        // We are at the job's site, so drop the inventory
                        World.world.inventoryManager.PlaceInventory(myJob, inventory);
                        myJob.DoWork(0); // This will call all cbJobWorked callbacks, because even though
                                         // we aren't progressing, it might want to do something with the fact
                                         // that the requirements are being met.

                        // Are we still carrying things?
                        if (inventory.stackSize == 0) {
                            inventory = null;
                        }

                        else {
                            Debug.LogError("Character is still carrying inventory, which shouldn't be. Just setting to NULL for now, but this means we are LEAKING inventory.");
                            inventory = null;
                        }
                    }

                    else {
                        // We still need to walk to the job site.
                        destinationTile = myJob.tile;
                        return;
                    }
                }

                else {
                    // We are carrying something, but the job doesn't want it!
                    // Dump the inventory at our feet
                    // TODO: Actually, walk to the nearest empty tile and dump it there.
                    if (!World.world.inventoryManager.PlaceInventory(currentTile, inventory)) {
                        Debug.LogError("Character tried to dump inventory into an invalid tile (maybe there's already something here.");
                        // FIXME: For the sake of continuing on, we are still going to dump any
                        // reference to the current inventory, but this means we are "leaking"
                        // inventory.  This is permanently lost now.
                        inventory = null;
                    }
                }
            }

            else {
                // At this point, the job still requires inventory, but we aren't carrying it!

                // Are we standing on a tile with goods that are desired by the job?
                if (currentTile.inventory != null &&
                    (myJob.canTakeFromStockpile || currentTile.furniture == null || !currentTile.furniture.IsStockpile()) &&
                    myJob.DesiresInventoryType(currentTile.inventory) > 0) {
                    // Pick up the stuff!

                    World.world.inventoryManager.PlaceInventory(this, currentTile.inventory, myJob.DesiresInventoryType(currentTile.inventory));
                }

                else {
                    // Walk towards a tile containing the required goods.


                    // Find the first thing in the Job that isn't satisfied.
                    Inventory desired = myJob.GetFirstDesiredInventory();

                    Inventory supplier = World.world.inventoryManager.GetClosestInventoryOfType(desired.objectType, currentTile, desired.itemsToMaxStackSize(), myJob.canTakeFromStockpile);

                    if (supplier == null) {
                        Debug.Log("No tile contains objects of type '" + desired.objectType + "' to satisfy job requirements.");
                        AbandonJob();
                        return;
                    }

                    destinationTile = supplier.tile;
                    return;
                }

            }

            return; // We can't continue until all materials are satisfied.
        }

        // If we get here, then the job has all the material that it needs.
        // Lets make sure that our destination tile is the job site tile.
        destinationTile = myJob.tile;

        // Are we there yet?
        if (currentTile == myJob.tile) {
            // We are at the correct tile for our job, so 
            // execute the job's "DoWork", which is mostly
            // going to countdown jobTime and potentially
            // call its "Job Complete" callback.
            myJob.DoWork(deltaTime);
            tiredness += deltaTime / 5;
        }

        // Nothing left for us to do here, we mostly just need Update_DoMovement to
        // get us where we want to go.
    }

    public void AbandonJob() {
        nextTile = destinationTile = currentTile;
        World.world.jobQueue.Enqueue(myJob);
        myJob = null;
    }

    void Update_DoMovement(float deltaTime) {
        if (currentTile == destinationTile) {
            pathAStar = null;
            return; // We're already were we want to be.
        }

        // currTile = The tile I am currently in (and may be in the process of leaving)
        // nextTile = The tile I am currently entering
        // destTile = Our final destination -- we never walk here directly, but instead use it for the pathfinding

        if (nextTile == null || nextTile == currentTile) {
            // Get the next tile from the pathfinder.
            if (pathAStar == null || pathAStar.Length() == 0) {
                // Generate a path to our destination
                pathAStar = new Path_AStar(currentTile, destinationTile);    // This will calculate a path from curr to dest.
                if (pathAStar.Length() == 0) {
                    Debug.LogError("Path_AStar returned no path to destination!");
                    AbandonJob();
                    return;
                }

                // Let's ignore the first tile, because that's the tile we're currently in.
                nextTile = pathAStar.Dequeue();

            }

            // Grab the next waypoint from the pathing system!
            nextTile = pathAStar.Dequeue();

            if (nextTile == currentTile) {
                Debug.LogError("Update_DoMovement - nextTile is currTile?");
            }
        }

        /*		if(pathAStar.Length() == 1) {
                    return;
                }
        */
        // At this point we should have a valid nextTile to move to.

        // What's the total distance from point A to point B?
        // We are going to use Euclidean distance FOR NOW...
        // But when we do the pathfinding system, we'll likely
        // switch to something like Manhattan or Chebyshev distance
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currentTile.x - nextTile.x, 2) + Mathf.Pow(currentTile.y - nextTile.y, 2));

        if (nextTile.IsEnterable() == ENTERABILITY.Never) {
            // Most likely a wall got built, so we just need to reset our pathfinding information.
            // FIXME: Ideally, when a wall gets spawned, we should invalidate our path immediately,
            //		  so that we don't waste a bunch of time walking towards a dead end.
            //		  To save CPU, maybe we can only check every so often?
            //		  Or maybe we should register a callback to the OnTileChanged event?
            Debug.LogError("FIXME: A character was trying to enter an unwalkable tile.");
            nextTile = null;    // our next tile is a no-go
            pathAStar = null;   // clearly our pathfinding info is out of date.
            return;
        }

        else if (nextTile.IsEnterable() == ENTERABILITY.Soon) {
            // We can't enter the NOW, but we should be able to in the
            // future. This is likely a DOOR.
            // So we DON'T bail on our movement/path, but we do return
            // now and don't actually process the movement.
            return;
        }

        // How much distance can be travel this Update?
        float distThisFrame = speed / nextTile.movementCost * deltaTime;

        // How much is that in terms of percentage to our destination?
        float percThisFrame = distThisFrame / distToTravel;

        // Add that to overall percentage travelled.
        movementPercentage += percThisFrame;

        tiredness += deltaTime / 10;

        if (movementPercentage >= 1) {
            // We have reached our destination

            // TODO: Get the next tile from the pathfinding system.
            //       If there are no more tiles, then we have TRULY
            //       reached our destination.

            currentTile = nextTile;
            movementPercentage = 0;
            // FIXME?  Do we actually want to retain any overshot movement?
        }
    }

    public void Update(float deltaTime) {
        //Debug.Log("Character Update");

        Update_DoJob(deltaTime);

        Update_DoMovement(deltaTime);

        cbCharacterChanged?.Invoke(this);
    }

    public void SetDestination(Tile tile) {
        if (currentTile.IsNeighbour(tile, true) == false) {
            Debug.Log("Character::SetDestination -- Our destination tile isn't actually our neighbour.");
        }

        destinationTile = tile;
    }

    public void RegisterOnChangedCallback(Action<Character> cb) {
        cbCharacterChanged += cb;
    }

    public void UnregisterOnChangedCallback(Action<Character> cb) {
        cbCharacterChanged -= cb;
    }

    void OnJobStopped(Job job) {
        // Job completed (if non-repeating) or was cancelled.

        job.UnregisterJobStoppedCallback(OnJobStopped);

        if (job != myJob) {
            Debug.LogError("Character being told about job that isn't his. You forgot to unregister something.");
            return;
        }

        myJob = null;
    }

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", currentTile.x.ToString());
        writer.WriteAttributeString("Y", currentTile.y.ToString());
    }

    public void ReadXml(XmlReader reader) {
    }
}
