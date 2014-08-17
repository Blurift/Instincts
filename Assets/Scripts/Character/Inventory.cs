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

		//GameManager.WriteMessage ("Inventory Init: " + Equipment.Length);

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
				Destroy(item);
				networkView.RPC("DropItemRPC", RPCMode.Server, i, "I");
			}
		}

		for(int i = 0; i < Equipment.Length; i++)
		{
			if(item == Equipment[i])
			{

				Equipment[i] = null;
				Destroy(item);
				networkView.RPC("DropItemRPC", RPCMode.Server, i, "E");
			}
		}
	}

	[RPC]
	public void DropItemRPC(int i, string type)
	{
		if(type == "I")
		{
			//ItemManager.SpawnItem(Items[i].name, transform.position);
			ItemManager.SpawnItemFromItem(Items[i], transform.position);
			Destroy (Items[i]);
			Items.RemoveAt(i);

		}
		if(type == "E")
		{
			//ItemManager.SpawnItem(Equipment[i].name, transform.position);
			if(Equipment[i] != null)
			{
				ItemManager.SpawnItemFromItem(Equipment[i], transform.position);
				Destroy(Equipment[i]);
				Equipment[i] = null;
			}
		}
	}

	[RPC]
	public void DropAll()
	{
		if(SelectedIndex > -1)
		{
			if(Equipment[SelectedIndex] != null)
			{
				Equipment[SelectedIndex].Unequip(Player);
			}
		}

		//If we are not the server tell teh server to drop all the items.
		if(!Network.isServer)
		{
			for(int i = Items.Count-1; i > -1; i--)
			{
				Destroy (Items[i]);
				Items.RemoveAt(i);
			}
			
			for(int i = 0; i < Equipment.Length; i++)
			{
				Destroy(Equipment[i]);
				Equipment[i] = null;
			}
			networkView.RPC("DropAll", RPCMode.Server);
			return;
		}

		for(int i = Items.Count-1; i > -1; i--)
		{
			DropItemRPC(i, "I");
		}
		
		for(int i = 0; i < Equipment.Length; i++)
		{
			DropItemRPC (i,"E");
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
				if(Items[i].StackAmount > 0)
					ChangeStack(i, Items[i].StackAmount);
			}
		}

		for (int i = Items.Count-1; i >= 0; i--)
		{
			if(Items[i].StackAmount <= 0)
			{
				RemoveFromInventory(i, "I");
			}
		}

		return amount;
	}

	public void RemoveFromInventory(int i, string t)
	{
		RemoveFromInventoryRPC (i,t);
		if(Network.isServer && !networkView.isMine)
			networkView.RPC ("RemoveFromInventoryRPC", networkView.owner, i, t);
		else
			networkView.RPC ("RemoveFromInventoryRPC", RPCMode.Server, i, t);
	}

	[RPC]
	public void RemoveFromInventoryRPC(int i, string t)
	{
		if(t == "I")
		{
			Destroy (Items [i]);
			Items.RemoveAt (i);
		}
		if(t == "E")
		{
			if(Equipment[i] != null)
			{
				Destroy(Equipment[i]);
				Equipment[i] = null;
			}
		}
	}

	public void AddToInventory(string name, int stack, int charges, string type)
	{
		AddToInventoryRPC (name, stack, charges);
		
		if(Network.isServer && !networkView.isMine)
			networkView.RPC("AddToInventoryRPC", networkView.owner, name,stack,charges);
		else if(Network.isClient)
			networkView.RPC("AddToInventoryRPC", RPCMode.Server, name,stack,charges);
	}

	public void AddToInventory(string name, int stack, int charges)
	{
		AddToInventory (name, stack, charges, "I");
	}

	[RPC]
	void AddToInventoryRPC(string name, int stack, int charges)
	{
		ScriptableObject s = ItemManager.CreateItem (name);
		Item item = (Item)s;
		if(charges != -1)		item.BController.Charges = charges;

		if(!item.Stackable)
		{
			Items.Add (item);
			return;
		}

		int stackLeft = stack;

		for(int i = 0; i < Items.Count; i++)
		{
			Item itemC = Items[i];
			if(itemC.Name == item.Name)
			{
				int stackSpace = itemC.StackMax - itemC.StackAmount;
				
				if(stackLeft <= stackSpace)
				{
					ChangeStack(i, itemC.StackAmount + stackLeft);
					//itemC.StackAmount += stackLeft;
					
					stackLeft = 0;
					Destroy (item);
					break;
				}
				else
				{
					ChangeStack(i,itemC.StackMax);
					stackLeft -= stackSpace;
				}
			}
		}
		
		if(stackLeft > 0)
		{
			item.StackAmount = stackLeft;
			Items.Add(item);			
		}		
	}

	[RPC]
	public void AddToInventory(string name, int amount)
	{
		AddToInventory (name, amount, -1);
		return;
		ScriptableObject s = ItemManager.CreateItem (name);
		Item item = (Item)s;
		if(item.Stackable)
			item.StackAmount = Mathf.Clamp (amount, 0, item.StackMax);

		Items.Add (item);
	}

	public void ChangeStack(int i, int a)
	{
		ChangeStackRPC (i, a);
		if(Network.isServer && !networkView.isMine)
			networkView.RPC ("ChangeStackRPC", networkView.owner, i, a);
		else if (Network.isClient)
			networkView.RPC ("ChangeStackRPC", RPCMode.Server, i, a);
	}

	[RPC]
	public void ChangeStackRPC(int i, int a)
	{
		if (Items [i].Stackable)
			Items [i].StackAmount = Mathf.Clamp (a, 0, Items [i].StackMax);

	}

	/// <summary>
	/// Changes the extra attributes of an item such as the remaining ammo.
	/// </summary>
	/// <param name="item">The item that will be being changed to the server.</param>
	public void ChangeAtts(Item item)
	{
		//Only do this if we are not the server as the the hosts item doesnt need verifing.
		if(!Network.isServer)
		{
			int index = -1;
			string type = "I";

			//Look through each item in the inventory for a match
			for(int i = 0; i < Items.Count; i++)
			{
				if(Items[i] == item)
					index=i;
			}

			//If one is not found look at the equipment
			if(index == -1)
			{
				for(int i = 0; i < Equipment.Length; i++)
				{
					if(Equipment[i] == item)
					{
						index = i;
						type = "E";
					}
				}
			}

			//If on is found send through the request to the server;
			if(index != -1 )
			{
				if(Network.isServer)
					ChangeAttsRPC(index, type, item.BController.Charges);
				else
					networkView.RPC("ChangeAttsRPC", RPCMode.Server, index, type, item.BController.Charges);
			}
		}
	}

	[RPC]
	public void ChangeAttsRPC(int i,string type, int charges)
	{
		Item item = null;

		if(type == "I" && Items.Count > i)
		{
			item = Items[i];
		}
		else if(type == "E" && Equipment.Length > i)
		{
			item = Equipment[i];
		}

		if(item != null)
		{
			item.BController.Charges = charges;
		}
	}

	public void SetInvState(InventoryState state)
	{
		foreach(Item.ItemState item in state.Items)
		{
			AddToInventory(item.Name,item.Stack,item.Charges);
		}

		foreach(Item.ItemState item in state.Equips)
		{
			AddToInventory(item.Name,item.Stack,item.Charges);
		}
	}

	public InventoryState GetInvState()
	{
		InventoryState state = new InventoryState ();
		foreach (Item item in Items) 
		{
			state.Items.Add(item.GetItemState());
		}

		for(int i = 0; i < Equipment.Length; i++)
		{
			if(Equipment[i] != null)
				state.Equips.Add(Equipment[i].GetItemState());
		}
		return state;
	}

	public void VerifyItems()
	{
		for(int i = Items.Count-1; i > -1; i--)
		{
			if(Items[i].Stackable && Items[i].StackAmount <= 0)
			{
				this.RemoveFromInventory(i, "I");
			}
		}

		for(int i = 0; i < Equipment.Length; i++)
		{
			if(Equipment[i] != null)
			{
				if(Equipment[i].Stackable && Equipment[i].StackAmount <= 0)
				{
					if(SelectedIndex == i)
					{
						Unequip(i);
						SelectedIndex = -1;
					}
					this.RemoveFromInventory(i, "E");
				}
			}
		}
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

	[System.Serializable]
	public class InventoryState
	{
		public List<Item.ItemState> Items;
		public List<Item.ItemState> Equips;

		public InventoryState()
		{
			Items = new List<Item.ItemState>();
			Equips = new List<Item.ItemState>();
		}
	}
}
