using UnityEngine;

[ExecuteInEditMode]
public class AutomaticVerticalSize : MonoBehaviour {

	public float childHeight = 35f;

	// Use this for initialization
	void Start () {
		AdjustSize();
	}

	void Update() {
		AdjustSize();
	}
	
	public void AdjustSize() {
		Vector2 size = GetComponent<RectTransform>().sizeDelta;
		size.y = transform.childCount * childHeight;
		GetComponent<RectTransform>().sizeDelta = size;
	}
}
