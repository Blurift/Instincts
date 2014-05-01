using UnityEngine;
using System.Collections;

public class RendererSort : MonoBehaviour {

	public string SortLayer;

	// Use this for initialization
	void Start () {
		if (renderer != null)
						renderer.sortingLayerName = SortLayer;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
