using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemInUseManager : MonoBehaviour {

	private static ItemInUseManager Instance;

	public GameObject[] Items;
	private Dictionary<string,GameObject> ItemsLookUp = new Dictionary<string, GameObject>();

	void Awake()
	{
		Instance = this;
	}

	// Use this for initialization
	void Start () {
		for(int i = 0; i < Items.Length; i++)
		{
			ItemsLookUp.Add(Items[i].name, Items[i]);
		}
	}

	public static GameObject Create(string name)
	{
		if(Instance.ItemsLookUp.ContainsKey(name))
		{
			return (GameObject)Instantiate(Instance.ItemsLookUp[name]);
		}

		return null;
	}

	public static GameObject Create(GameObject obj)
	{
		if (obj == null)
						return null;

		return Create (obj.name);
	}
}
