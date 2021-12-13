using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MenuTween : MonoBehaviour
{
    private void Start()
    {
        this.gameObject.SetActive(false);
    }
    public UnityEvent onCompleteCallBack;
    public void buttonClicked()
    {
        if (this.gameObject.activeSelf == false){
            OnEnable();
        }
        else
        {
            OnDisable();
        }
    }

    public void OnEnable()
    {
        this.gameObject.SetActive(true); //defines the object as being acitve so that it will be visible during the entire animation.
        transform.localScale = new Vector3(0, 0, 0); //this sets the starting size to 0, in all axis so that it is invisiable.
        LeanTween.scale(gameObject, new Vector3(1, 1, 1), 0.1f).setDelay(0.0f);//this scales up the element, giving the apperance of it bopping into view
        
    }
    public void OnComplete()
    {   // allows this function to be able to call other functions once done.
        if (onCompleteCallBack != null)
        {
            onCompleteCallBack.Invoke(); 
        }
    }

    // Update is called once per frame
    public void OnDisable() //is the antagonist animation to first one. it will shrink down the element and then disable it. 
    {
        LeanTween.scale(gameObject, new Vector3(0, 0, 0), 0.1f).setOnComplete(DestroyMeDaddy); 
    }
    void DestroyMeDaddy()
    {
        this.gameObject.SetActive(false); //disables the element in question. 
    }
}
