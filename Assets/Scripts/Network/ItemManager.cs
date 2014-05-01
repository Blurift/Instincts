using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour {

	public static ItemManager Instance;

	public Dictionary<string, GameObject> ItemPrefabs;

	public string[] ItemNames;
	public GameObject[] ItemObjects;

	// Use this for initialization
	void Start () {
		ItemPrefabs = new Dictionary<string, GameObject> ();
		if (ItemNames.Length == ItemObjects.Length && ItemNames.Length > 0)
		{
			for (int i = 0; i < ItemNames.Length; i++) {
				if(ItemNames[i] != null && ItemObjects[i] != null)
					ItemPrefabs.Add(ItemNames[i],ItemObjects[i]);
			}
		}

		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	[RPC]
	void SpawnItemRPC(string itemName, Vector3 position, NetworkViewID id)
	{
		GameObject o = (GameObject)Instantiate (Instance.ItemPrefabs [itemName], position, Quaternion.identity);
		o.networkView.viewID = id;
	}

	public static GameObject SpawnItem(string itemName, Vector3 worldPosition)
	{
		if(Network.isServer)
		{
			NetworkViewID id = Network.AllocateViewID();
			GameObject o = (GameObject)Instantiate (Instance.ItemPrefabs [itemName], worldPosition, Quaternion.identity);

			o.networkView.viewID = id;
			Instance.networkView.RPC("SpawnItemRPC", RPCMode.Others, itemName, worldPosition, id);
			return o;
		}

		return null;
	}

	public static GameObject CraftItem(string itemName, NetworkPlayer player)
	{
		if(Network.isServer)
		{
			NetworkViewID id = Network.AllocateViewID();
			GameObject o = (GameObject)Instantiate (Instance.ItemPrefabs [itemName], Vector3.zero, Quaternion.identity);
			
			o.networkView.viewID = id;
			Instance.networkView.RPC("SpawnItemRPC", player, itemName, Vector3.zero, id);
			return o;
		}
		
		return null;
	}
}
