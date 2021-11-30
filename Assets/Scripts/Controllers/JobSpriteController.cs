using UnityEngine;
using System.Collections.Generic;

public class JobSpriteController : MonoBehaviour {

    // This bare-bones controller is mostly just going to piggyback
    // on FurnitureSpriteController because we don't yet fully know
    // what our job system is going to look like in the end.

    FurnitureSpriteController furnitureSpriteController;
    Dictionary<Job, GameObject> jobGameObjectMap;

    // Use this for initialization
    void Start() {
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        furnitureSpriteController = FindObjectOfType<FurnitureSpriteController>();

        // FIXME: No such thing as a job queue yet!
        World.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);

    }

    void OnJobCreated(Job job) {

        if (job.jobObjectType == null) {
            // This job doesn't really have an associated sprite with it, so no need to render.
            return;
        }

        // FIXME: We can only do furniture-building jobs.
        // TODO: Sprite

        if (jobGameObjectMap.ContainsKey(job)) {
            Debug.LogError("OnJobCreated for a jobGO that already exists -- most likely a job being RE-QUEUED, as opposed to created.");
            return;
        }

        GameObject gameObject = new GameObject();

        // Add our tile/GO pair to the dictionary.
        jobGameObjectMap.Add(job, gameObject);

        gameObject.name = "JOB_" + job.jobObjectType + "_" + job.tile.x + "_" + job.tile.y;
        gameObject.transform.position = new Vector3(job.tile.x + ((job.furniturePrototype.Width - 1) / 2f), job.tile.y + ((job.furniturePrototype.Height - 1) / 2f), 0);
        gameObject.transform.SetParent(this.transform, true);

        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = furnitureSpriteController.GetSpriteForFurniture(job.jobObjectType);
        spriteRenderer.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        spriteRenderer.sortingLayerName = "Jobs";

        if (job.jobObjectType == "Door") {
            // By default, the door graphic is meant for walls to the east & west
            // Check to see if we actually have a wall north/south, and if so
            // then rotate this GO by 90 degrees
            if (job.tile.North() != null && job.tile.South() != null && job.tile.North().hasFurnitureOfType("furn_SteelWall") && job.tile.South().hasFurnitureOfType("furn_SteelWall")) {
                gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }

        job.RegisterJobCompletedCallback(OnJobEnded);
        job.RegisterJobStoppedCallback(OnJobEnded);
    }

    void OnJobEnded(Job job) {
        // This executes whether a job was COMPLETED or CANCELLED

        // FIXME: We can only do furniture-building jobs.

        GameObject gameObject = jobGameObjectMap[job];

        job.UnregisterJobCompletedCallback(OnJobEnded);
        job.UnregisterJobStoppedCallback(OnJobEnded);

        Destroy(gameObject);
    }
}
