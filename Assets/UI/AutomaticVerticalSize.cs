using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class AutomaticVerticalSize : MonoBehaviour
{

	public float childHeight = 10f;
	public float childWidth = 175f;

	// Use this for initialization
	void Start()
	{
		AdjustSize();
	}

	void Update()
	{
		AdjustSize();
	}

	public void AdjustSize()
	{
		Vector2 size = this.GetComponent<RectTransform>().sizeDelta;
		size.y = this.transform.childCount * childHeight;
		size.x = this.transform.childCount * childWidth;
		this.GetComponent<RectTransform>().sizeDelta = size;
	}
}