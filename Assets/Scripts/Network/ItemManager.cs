using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemManager : MonoBehaviour {

	public static ItemManager Instance;

	public Dictionary<string, Item> ItemPrefabs;
	
	public Item[] ItemObjects;

	public GameObject ItemDropPrefab;
	private List<GameObject> currentItemDrops = new List<GameObject> ();
	private int DropCounter = 0;

	// Use this for initialization
	void Start () {
		ItemPrefabs = new Dictionary<string, Item> ();

		for (int i = 0; i < ItemObjects.Length; i++) {
			if(ItemObjects[i] != null)
				ItemPrefabs.Add(ItemObjects[i].name,ItemObjects[i]);
		}

		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static ScriptableObject CreateItem(string name)
	{
		if(Instance.ItemPrefabs.ContainsKey(name))
		{
			return (ScriptableObject)Instantiate(Instance.ItemPrefabs[name]);
		}

		Debug.LogError ("ItemManager: Create item name not reconised - (" + name + ")");
		return null;
	}

	public static ScriptableObject CreateItem(Item item)
	{
		return CreateItem (item.name);
	}

	[RPC]
	GameObject SpawnItemRPC(string itemName, Vector3 position, int id, int stack, int charges)
	{
		if(!ItemPrefabs.ContainsKey(itemName))
		{
			Debug.LogError("ItemManager: Could not spawn item (" + itemName +")");
			return null;
		}
		GameObject o = (GameObject)Instantiate (ItemDropPrefab, position, Quaternion.identity);
		ItemDrop d = o.GetComponent<ItemDrop>();
		
		d.DropID = id;
		d.item = ItemPrefabs[itemName];
		d.ItemStack = stack;
		d.ItemCharges = charges;
		d.DespawnTime = Time.time + 120;
		if (stack == -1)
			d.ItemStack = d.item.StackAmount;
		if (charges == -1)
			d.ItemCharges = d.item.BController.Charges;

		currentItemDrops.Add (o);
		return o;
	}

	[RPC]
	GameObject SpawnItemWorld (string name, Vector3 worldPosition, int stack, int charges)
	{
		if (Network.isServer)
		{
			networkView.RPC("SpawnItemRPC", RPCMode.Others, name, worldPosition, DropCounter, stack, charges);
			GameObject o = SpawnItemRPC(name,worldPosition,DropCounter, stack,charges);

			if(o!=null)
				DropCounter++;

			return o;
		}
		return null;
	}

	public static GameObject SpawnItem(string itemName, Vector3 worldPosition, int stack, int charges)
	{
		if(Network.isServer)
		{
			return Instance.SpawnItemWorld(itemName, worldPosition, stack,charges);
		}
		else
			Instance.networkView.RPC("SpawnItemWorld", RPCMode.Server, itemName, worldPosition, stack, charges);

		return null;
	}

	public static GameObject SpawnItem(string itemName, Vector3 worldPosition)
	{
		return SpawnItem (itemName, worldPosition, -1, -1);
	}

	public static GameObject SpawnItemFromItem(Item item, Vector3 position)
	{
		return SpawnItem (item.name, position, item.StackAmount, item.BController.Charges);
	}

	[RPC]
	void RemoveDropRPC(int id)
	{
		for(int i = 0; i < currentItemDrops.Count; i++)
		{
			GameObject o = currentItemDrops[i];
			if(o.GetComponent<ItemDrop>().DropID == id)
			{
				Destroy (currentItemDrops[i]);
				currentItemDrops.RemoveAt(i);
			}
		}
	}

	[RPC]
	void RemoveDrop(int id)
	{
		if(Network.isServer)
		{
			RemoveDropRPC(id);
			networkView.RPC("RemoveDropRPC", RPCMode.Others, id);
		}
		else
			networkView.RPC("RemoveDrop", RPCMode.Server, id);
	}

	public static void RemoveDropFromWorld(int id)
	{
		Instance.RemoveDrop (id);
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

	public static void InitializeDropsForPlayer(NetworkPlayer player)
	{
		foreach(GameObject g in Instance.currentItemDrops)
		{
			ItemDrop d = g.GetComponent<ItemDrop>();
			Instance.networkView.RPC("SpawnItemRPC", player, d.item.name, d.transform.position, d.DropID, d.ItemStack,d.ItemCharges);
		}
	}
}
