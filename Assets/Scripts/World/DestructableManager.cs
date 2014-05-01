/* Name: Destructable Scenery Manager
 * Desc: Stores and manipulates vitals for a game object
 * Author: Keirron Stach
 * Version: 1.0
 * Created: 22/04/2014
 * Edited: 23/04/2014
 */ 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[RequireComponent(typeof(NetworkView))]
public class DestructableManager : MonoBehaviour {

	private static DestructableManager instance;

	public List<DestructableSpawner> spawners;
	private List<DestructableProp> temps;
	private Dictionary<int, DestructableProp> props;

	public GameObject[] PropPrefabs;

	private int tempId = 0;


	void Awake ()
	{
		Debug.Log ("Manager assigned");
		instance = this;
		spawners = new List<DestructableSpawner> ();
		props = new Dictionary<int, DestructableProp> ();
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if(!Network.isServer)
			return;
		for (int i = 0; i < spawners.Count; i++)
		{
			spawners[i].DUpdate();
		}
	}

	public static void AddSpawner(DestructableSpawner spawner)
	{
		//TO DO
		//Check for doubles
		instance.spawners.Add (spawner);
	}

	public static void DestroyProp(DestructableProp prop)
	{
		int id = prop.Id;

		instance.DestroyPropRPC (id);
		instance.networkView.RPC ("DestroyPropRPC", RPCMode.Others, id);

	}

	[RPC]
	void DestroyPropRPC(int id)
	{
		DestructableProp prop = props [id];

		prop.Death ();

		props.Remove (id);
	}

	public static GameObject CreateProp(GameObject prop, Vector3 pos, Quaternion rot)
	{
		int prefabId = instance.GetPropPrefId (prop);

		if(prefabId == -1)
			return null;

		instance.CreatePropRPC (instance.tempId, prefabId, pos, rot);
		instance.networkView.RPC ("CreatePropRPC", RPCMode.Others, instance.tempId, prefabId, pos, rot);

		instance.tempId++;

		return instance.props [instance.tempId - 1].gameObject;
	}

	[RPC]
	void CreatePropRPC(int id, int prefabId, Vector3 pos, Quaternion rot)
	{
		GameObject prefab = PropPrefabs [prefabId];
		GameObject prop = (GameObject)Instantiate (prefab, pos, rot);
		DestructableProp des = prop.GetComponent<DestructableProp> ();
		des.Id = id;
		props.Add (id, des);
	}

	private int GetPropPrefId(GameObject prop)
	{
		for (int i = 0; i < PropPrefabs.Length; i++)
		{
			if(prop.name.Equals(PropPrefabs[i].name))
				return i;
		}
		return -1;
	}
}
