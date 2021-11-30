using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour {

    Dictionary<Character, GameObject> characterGameObjectMap;

    Dictionary<string, Sprite> characterSprites;

    // Use this for initialization
    void Start() {
        LoadSprites();

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        characterGameObjectMap = new Dictionary<Character, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        World.world.RegisterCharacterCreated(OnCharacterCreated);

        // Check for pre-existing characters, which won't do the callback.
        foreach (Character c in World.world.characters) {
            OnCharacterCreated(c);
        }


        //c.SetDestination( world.GetTileAt( world.Width/2 + 5, world.Height/2 ) );
    }

    void LoadSprites() {
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Characters/");

        //Debug.Log("LOADED RESOURCE:");
        foreach (Sprite sprite in sprites) {
            //Debug.Log(s);
            characterSprites[sprite.name] = sprite;
        }
    }

    public void OnCharacterCreated(Character character) {
        //		Debug.Log("OnCharacterCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject gameObject = new GameObject();

        // Add our tile/GO pair to the dictionary.
        characterGameObjectMap.Add(character, gameObject);

        gameObject.name = "Character";
        gameObject.transform.position = new Vector3(character.x, character.y, 0);
        gameObject.transform.SetParent(transform, true);

        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = characterSprites["Worker"];
        spriteRenderer.sortingLayerName = "Characters";

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        character.RegisterOnChangedCallback(OnCharacterChanged);

    }

    void OnCharacterChanged(Character c) {
        //Debug.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (characterGameObjectMap.ContainsKey(c) == false) {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
            return;
        }

        GameObject char_go = characterGameObjectMap[c];
        //Debug.Log(furn_go);
        //Debug.Log(furn_go.GetComponent<SpriteRenderer>());

        //char_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

        char_go.transform.position = new Vector3(c.x, c.y, 0);
    }



}
