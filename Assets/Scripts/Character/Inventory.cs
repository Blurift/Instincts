using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Inventory : MonoBehaviour {
	
	public Item[] Equipment = new Item[6];
	public List<Item> Items = new List<Item>();
	public List<Item> ItemsToEquip = new List<Item> ();

	public int SelectedIndex = -1;

	public Vector2 ScreenPosition;

	public PlayerController Player;

	// Use this for initialization
	void Start () {

		Player = GetComponent<PlayerController> ();
		Equipment = new Item[6];

		for (int i = 0; i < ItemsToEquip.Count; i++)
		{
			if(i < Equipment.Length)
				Equipment[i] = ItemsToEquip[i];
		}

		ItemsToEquip = null;

		GameManager.WriteMessage ("Inventory Init: " + Equipment.Length);

		if(networkView.isMine)
			GameManager.ControllingInventory = this;
	}

	// Update is called once per frame
	void Update () {
		if(networkView.isMine)
		{
			int itemInt = -1;
			if (Input.GetKeyDown (KeyCode.Alpha1))
				itemInt = 0;
			if (Input.GetKeyDown (KeyCode.Alpha2))
				itemInt = 1;
			if (Input.GetKeyDown (KeyCode.Alpha3))
				itemInt = 2;
			if (Input.GetKeyDown (KeyCode.Alpha4))
				itemInt = 3;
			if (Input.GetKeyDown (KeyCode.Alpha5))
				itemInt = 4;
			if (Input.GetKeyDown (KeyCode.Alpha6))
				itemInt = 5;

			if(itemInt > -1)
				SelectItem (itemInt);


		}
	}
	
	[RPC]
	void CraftItem(string itemName, NetworkPlayer player)
	{
		if(Network.isServer)
		{

			GameObject itemGO = ItemManager.CraftItem(itemName, player);

			Item item = (Item)itemGO.GetComponent(typeof(Item));

			item.ChangeOwnerRPC(player);

			networkView.RPC("CraftItemAttach", player, itemGO.networkView.viewID);
		}
	}

	[RPC]
	void CraftItemAttach(NetworkViewID viewId)
	{
		GameObject[] items = GameObject.FindGameObjectsWithTag ("Item");

		for(int i = items.Length-1 ; i > -1; i--)
		{
			if(items[i].networkView.viewID == viewId)
			{
				Item item  = (Item)items[i].GetComponent(typeof(Item));
				AddToInventory(item);
				break;
			}
		}
	}

	[RPC]
	void ItemAttach(NetworkViewID viewId)
	{
		GameObject[] items = GameObject.FindGameObjectsWithTag ("Item");

		Item item = null;

		for(int i = items.Length-1 ; i > -1; i--)
		{
			if(items[i].networkView.viewID == viewId)
			{
				item = (Item)items[i].GetComponent(typeof(Item));
				break;
			}
		}



		if(item != null)
		{
			//GameManager.WriteMessage("Equipping Item: " + Equipment.Length);
			if(Equipment.Length > 0)
			{
				for (int i = 0; i < Equipment.Length; i++)
				{
					if(Equipment[i] == null)
					{

						item.networkView.RPC("ChangeOwnerRPC", RPCMode.Server, Network.player);
						item.invOwner = this;
						Equipment[i] = item;
						break;
					}
				}
			}
			else
			{
				item.networkView.RPC("ChangeOwnerRPC", RPCMode.Server, Network.player);
				item.invOwner = this;
				ItemsToEquip.Add(item);
			}
		}
	}



	public void PickUpItem(Item item)
	{
	}

	public void DropItem(Item item)
	{
		for(int i = 0; i < Items.Count; i++)
		{
			if(item == Items[i])
			{
				Items.RemoveAt(i);
				item.networkView.RPC("DropItemInWorldRPC", RPCMode.Server, transform.position);
			}
		}

		for(int i = 0; i < Equipment.Length; i++)
		{if(item == Equipment[i])
			{
				Equipment[i] = null;
				item.networkView.RPC("DropItemInWorldRPC", RPCMode.Server, transform.position);
			}
		}
	}

	public void DropAll()
	{
		if(SelectedIndex > -1)
		{
			if(Equipment[SelectedIndex] != null)
			{
				Equipment[SelectedIndex].Unequip(Player);
			}
		}


		//Drop all Inventory Items
		for(int i = Items.Count-1; i > -1; i--)
		{
			Items[i].networkView.RPC("DropItemInWorldRPC", RPCMode.Server, transform.position);
			Items.RemoveAt(i);
		}

		for(int i = 0; i < Equipment.Length; i++)
		{
			if(Equipment[i] != null)
			{
				Equipment[i].networkView.RPC("DropItemInWorldRPC", RPCMode.Server, transform.position);
				Equipment[i] = null;

			}
		}
	}

	public float TotalWeight()
	{
		float weight = 0;

		for (int i = 0; i < Items.Count; i++)
		{
			weight += Items[i].Weight;
		}

		for (int i = 0; i < Equipment.Length; i++)
		{
			if(Equipment[i] != null)
				weight += Equipment[i].Weight;
		}

		return weight;
	}

	public void SelectItem(int index)
	{
		//Unequipe previous Item
		if(SelectedIndex > -1 && SelectedIndex < Equipment.Length)
		{
			Unequip(SelectedIndex);
		}

		//Check if the new index is a valid slot
		if(index >= Equipment.Length || SelectedIndex == index)
		{
			SelectedIndex = -1;
		}
		else if(Equipment[index] != null)
		{
			SelectedIndex = index;

			Item equipItem = Equipment[SelectedIndex];
			equipItem.Equip(Player);
		}
		else
		{
			SelectedIndex = -1;
		}


		Player.SetAnimEquipped (SelectedIndex != -1);
	}

	public void Unequip(int index)
	{
		if(index > -1 && index < Equipment.Length)
		{
			if(Equipment[index] != null)
			{
				Item unItem = Equipment[index];
				unItem.Unequip(Player);
			}
		}
	}

	public Item SearchForItem(string name)
	{
		for(int i = 0; i < Items.Count; i++)
		{
			if(Items[i].name == name)
			{
				return Items[i];
			}
		}
		return null;
	}

	public void AddToInventory(Item item)
	{
		if(!item.Stackable)
		{
			item.networkView.RPC("ChangeOwnerRPC", RPCMode.Server, Network.player);
			Items.Add(item);
			return;
		}

		int stackLeft = item.StackAmount;

		for(int i = 0; i < Items.Count; i++)
		{
			Item itemC = Items[i];
			if(itemC.Name == item.Name)
			{
				int stackSpace = itemC.StackMax - itemC.StackAmount;

				if(stackLeft <= stackSpace)
				{
					itemC.StackAmount += stackLeft;

					stackLeft = 0;
					item.networkView.RPC("DestroyItem", RPCMode.Server);
					break;
				}
				else
				{
					itemC.StackAmount = itemC.StackMax;
					stackLeft -= stackSpace;
				}
			}
		}

		if(stackLeft > 0)
		{
			item.StackAmount = stackLeft;
			item.networkView.RPC("ChangeOwnerRPC", RPCMode.Server, Network.player);
			item.invOwner = this;
			Items.Add(item);

		}
	}

	public bool IsStackAvailable(string name, int need)
	{
		int amount = 0;

		for (int i = 0; i < Items.Count; i++)
		{
			if(Items[i].name == name)
			{
				amount += Items[i].StackAmount;

				if(amount >= need)
					return true;
			}
		}

		return false;
	}

	public int RetrieveFromStack(string name, int need)
	{

		int amount = 0;
		for (int i = 0; i < Items.Count; i++)
		{
			if(Items[i].name == name && amount < need)
			{
				amount += Items[i].TakeFromStack(need - amount);

			}
		}

		for (int i = Items.Count-1; i >= 0; i--)
		{
			if(Items[i].StackAmount <= 0)
			{
				Items[i].DestroyItem();
				Items.RemoveAt(i);
			}
		}

		return amount;
	}

	public void VerifyItem(Item item)
	{
		if(item.StackAmount == 0)
		{
			for(int i = Items.Count-1; i > -1; i--)
			{
				if(Items[i] == item)
				{
					Items.RemoveAt(i);
					break;
				}
			}

			for(int i = 0; i < Equipment.Length; i++)
			{
				if(Equipment[i] == item)
				{
					if(SelectedIndex == i)
					{
						Equipment[i].Unequip(Player);
						SelectedIndex = -1;
					}

					Equipment[i] = null;

				}
			}
		}
	}
}
