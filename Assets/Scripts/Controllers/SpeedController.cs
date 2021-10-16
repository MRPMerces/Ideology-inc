using System;
using UnityEngine;

public class SpeedController : MonoBehaviour {

    public static SpeedController speed { get; protected set; }

    DateTime date;

    public float worldTick { get; protected set; }

    public int currentDay { get; protected set; }

    float time = 0.0f;

    void OnEnable() {
        speed = this;
        currentDay = 0;
    }

    // Start is called before the first frame update
    void Start() {
        set_interval();

        date = new DateTime(1850, 01, 01);
    }

    void Update() {
        time += Time.deltaTime;

        if (time >= worldTick) {
            time -= worldTick;
            date = date.AddDays(1);
            currentDay++;
        }
    }

    /// Callback
    void set_interval() {
        worldTick = 1f;
    }

    public string currentDate() {
        return date.ToString("yyyy, MMM, dd");
    }
}
