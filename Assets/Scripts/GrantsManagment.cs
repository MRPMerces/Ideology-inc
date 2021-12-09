using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class GrantsManagment : MonoBehaviour
{
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GrantsManager start");
        test();
    }

    void test()
    {
        Grant grant = new Grant();
        grant.id = 1;
        grant.name = "grow";
        grant.reward = 100;
        grant.dependency = new List<int>();
        grant.dependency.Add(0);

        Requirement req = new Requirement();
        req.displaytext = "Get 10 followers";
        req.value = 10;
        req.property = "followers";
        req.modifyer = "<";
        grant.requirements = new List<Requirement>();
        grant.requirements.Add(req);

        Grant[] grants = new Grant[] { grant };
        string json = JsonUtility.ToJson(grants);
        File.WriteAllText(Application.dataPath + "/textfiles/grants/testgrants2.json", json);
    }

    public class Requirement
    {
        public string displaytext;
        public int value;
        public string property;
        public string modifyer;
    }

    public class Grant
    {
        public int id;
        public string name;
        public int reward;
        public List<int> dependency;
        public List<Requirement> requirements;
    }

}
