using UnityEngine;

public class SoundController : MonoBehaviour {
    float soundCooldown = 0;

    // Use this for initialization
    void Start() {
        World.world.RegisterFurnitureCreated(OnFurnitureCreated);
        World.world.RegisterTileChanged(OnTileChanged);
    }

    // Update is called once per frame
    void Update() {
        soundCooldown -= Time.deltaTime;
    }

    void OnTileChanged(Tile tile) {
        // FIXME

        if (soundCooldown > 0) {
            return;
        }

        AudioClip audioClip = Resources.Load<AudioClip>("Sounds/Floor_OnCreated");
        AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }

    public void OnFurnitureCreated(Furniture furniture) {
        // FIXME
        if (soundCooldown > 0) {
            return;
        }

        AudioClip audioClip = Resources.Load<AudioClip>("Sounds/" + furniture.objectType + "_OnCreated");

        if (audioClip == null) {
            // Since there's no specific sound for whatever Furniture this is, just
            // use a default sound -- i.e. the Wall_OnCreated sound.
            audioClip = Resources.Load<AudioClip>("Sounds/Wall_OnCreated");
        }

        AudioSource.PlayClipAtPoint(audioClip, Camera.main.transform.position);
        soundCooldown = 0.1f;
    }
}
