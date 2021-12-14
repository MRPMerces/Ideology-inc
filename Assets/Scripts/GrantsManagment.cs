using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;

public class GrantsManagment : MonoBehaviour
{
    private Grants grants;
    [SerializeField]
    private GameObject grantTemplate;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("GrantsManager start");
        WriteTest();
        ReadTest();
        Read();
        OnEnable();
    }

    void OnEnable()
    {
        for (int i = 0; i < grants.grants.Count; i++)
        {
            GameObject grant = Instantiate(grantTemplate) as GameObject;
            grant.SetActive(true);

            grant.GetComponent<GrantObject>().SetContent(grants.grants[i]);

        }
    }

    void WriteTest()
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

        Grants grants = new Grants();
        grants.grants = new List<Grant> { grant };
        string json = JsonUtility.ToJson(grants);
        File.WriteAllText(Application.dataPath + "/grants/testgrants.json", json);
    }

    void ReadTest()
    {
        string json = File.ReadAllText(Application.dataPath + "/grants/testgrants.json");
        Grants grants = JsonUtility.FromJson<Grants>(json);
        Grant grant = grants.grants[0];
        Assert.AreEqual("grow", grant.name);
        Requirement req = grant.requirements[0];
        Assert.AreEqual("Get 10 followers", req.displaytext);
    }

    void Read()
    {
        string json = File.ReadAllText(Application.dataPath + "/grants/grants.json");
        grants = JsonUtility.FromJson<Grants>(json);
    }

    [Serializable]
    public class Requirement
    {
        public string displaytext;
        public int value;
        public string property;
        public string modifyer;
    }

    [Serializable]
    public class Grant
    {
        public int id;
        public string name;
        public int reward;
        public List<int> dependency;
        public List<Requirement> requirements;
    }

    [Serializable]
    public class Grants
    {
        public List<Grant> grants;
    }

    public enum Status
    {
        Unlocked,
        Accepted,
        Locked,
        Completed

    }
}
