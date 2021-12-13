using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuEnable : MonoBehaviour
{
    // Start is called before the first frame update
    
        public void buttonClicked()
        {
            this.gameObject.SetActive(!this.gameObject.activeSelf);
        }
}
