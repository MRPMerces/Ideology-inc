using System;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {
    Queue<Job> jobQueue;

    Action<Job> cbJobCreated;

    public JobQueue() {
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job job) {
        Debug.Log("Adding job to queue. Existing queue size: " + jobQueue.Count);
        if (job.jobTime < 0) {
            // Job has a negative job time, so it's not actually
            // supposed to be queued up.  Just insta-complete it.
            job.DoWork(0);
            return;
        }

        jobQueue.Enqueue(job);

        cbJobCreated?.Invoke(job);
    }

    public Job Dequeue() {
        if (jobQueue.Count == 0)
            return null;

        return jobQueue.Dequeue();
    }

    public void RegisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated += cb;
    }

    public void UnregisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated -= cb;
    }

    public void Remove(Job job) {
        // TODO: Check docs to see if there's a less memory/swappy solution
        List<Job> jobs = new List<Job>(jobQueue);

        if (jobs.Contains(job) == false) {
            //Debug.LogError("Trying to remove a job that doesn't exist on the queue.");
            // Most likely, this job wasn't on the queue because a character was working it!
            return;
        }

        jobs.Remove(job);
        jobQueue = new Queue<Job>(jobs);
    }

}
