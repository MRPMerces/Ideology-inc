using UnityEngine;
using UnityEngine.UI;

public class FurnitureBuildMenu : MonoBehaviour {

    public GameObject buildFurnitureButtonPrefab;

    // Use this for initialization
    void Start() {
        // For each furniture prototype in our world, create one instance
        // of the button to be clicked!

        foreach (string s in World.world.furniturePrototypes.Keys) {
            GameObject gameObject = Instantiate(buildFurnitureButtonPrefab);
            gameObject.transform.SetParent(transform);

            gameObject.name = "Button - Build " + s;

            gameObject.transform.GetComponentInChildren<Text>().text = "Build " + World.world.furniturePrototypes[s].Name;

            gameObject.GetComponent<Button>().onClick.AddListener(delegate { FindObjectOfType<BuildModeController>().SetMode_BuildFurniture(s); });
        }
    }
}
