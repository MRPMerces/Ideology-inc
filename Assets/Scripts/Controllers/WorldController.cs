using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldController : MonoBehaviour {

    public static WorldController worldController { get; protected set; }

    // The world and tile data
    World world;

    static bool loadWorld = false;

    // Use this for initialization
    void OnEnable() {

        worldController = this;

        if (loadWorld) {
            loadWorld = false;
            CreateWorldFromSaveFile();
        }

        else {
            CreateEmptyWorld();
        }
    }

    void Update() {
        // TODO: Add pause/unpause, speed controls, etc...
        world.Update(Time.deltaTime);

    }

    public void NewWorld() {
        Debug.Log("NewWorld button was clicked.");

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveWorld() {
        Debug.Log("SaveWorld button was clicked.");

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextWriter writer = new StringWriter();
        serializer.Serialize(writer, world);
        writer.Close();

        Debug.Log(writer.ToString());

        PlayerPrefs.SetString("SaveGame00", writer.ToString());

    }

    public void LoadWorld() {
        Debug.Log("LoadWorld button was clicked.");

        // Reload the scene to reset all data (and purge old references)
        loadWorld = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    void CreateEmptyWorld() {
        // Create a world with Empty tiles
        world = new World(100, 100);

        // Center the Camera
        Camera.main.transform.position = new Vector3(world.width / 2, world.height / 2, Camera.main.transform.position.z);

    }

    void CreateWorldFromSaveFile() {
        Debug.Log("CreateWorldFromSaveFile");
        // Create a world from our save file data.

        XmlSerializer serializer = new XmlSerializer(typeof(World));
        TextReader reader = new StringReader(PlayerPrefs.GetString("SaveGame00"));
        Debug.Log(reader.ToString());
        world = (World)serializer.Deserialize(reader);
        reader.Close();



        // Center the Camera
        Camera.main.transform.position = new Vector3(world.width / 2, world.height / 2, Camera.main.transform.position.z);

    }

}
