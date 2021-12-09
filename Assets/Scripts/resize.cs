using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class resize : MonoBehaviour
{

    public Image image;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseEnter()
    {
        image.rectTransform.sizeDelta = new Vector2(100, image.rectTransform.sizeDelta.y);
    }
    public void OnMouseExit()
    {
        image.rectTransform.sizeDelta = new Vector2(50, image.rectTransform.sizeDelta.y);
    }
}
