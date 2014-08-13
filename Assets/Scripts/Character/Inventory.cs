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
		{if(item == Equipment[i])
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
			ItemManager.SpawnItem(Items[i].name, transform.position);
			Destroy (Items[i]);
			Items.RemoveAt(i);

		}
		if(type == "E")
		{
			ItemManager.SpawnItem(Items[i].name, transform.position);
			Destroy(Equipment[i]);
			Equipment[i] = null;
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


		//Drop all Inventory Items
		for(int i = Items.Count-1; i > -1; i--)
		{
			DropItem (Items[i]);
		}

		for(int i = 0; i < Equipment.Length; i++)
		{
			if(Equipment[i] != null)
			{
				DropItem (Equipment[i]);
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
				RemoveFromInventory(i);
			}
		}

		return amount;
	}

	public void RemoveFromInventory(int i)
	{
		RemoveFromInventoryRPC (i);
		if(Network.isServer)
			networkView.RPC ("RemoveFromInventoryRPC", networkView.owner, i);
		else
			networkView.RPC ("RemoveFromInventoryRPC", RPCMode.Server, i);
	}

	[RPC]
	public void RemoveFromInventoryRPC(int i)
	{
		Destroy (Items [i]);
		Items.RemoveAt (i);

	}

	public void AddToInventory(string name, int stack, int custom)
	{
		AddToInventoryRPC (name, stack, custom);

		if(Network.isServer)
			networkView.RPC("AddToInventoryRPC", networkView.owner, name,stack,custom);
		else
			networkView.RPC("AddToInventoryRPC", RPCMode.Server, name,stack,custom);
	}

	[RPC]
	void AddToInventoryRPC(string name, int stack, int custom)
	{
		ScriptableObject s = ItemManager.CreateItem (name);
		Item item = (Item)s;

		if(!item.Stackable)
		{
			Items.Add ((Item)s);
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
		ScriptableObject s = ItemManager.CreateItem (name);
		Item item = (Item)s;
		if(item.Stackable)
			item.StackAmount = Mathf.Clamp (amount, 0, item.StackMax);

		Items.Add (item);
	}

	public void ChangeStack(int i, int a)
	{
		ChangeStackRPC (i, a);
		if(Network.isServer)
			networkView.RPC ("ChangeStackRPC", networkView.owner, i, a);
		else
			networkView.RPC ("ChangeStackRPC", RPCMode.Server, i, a);
	}

	[RPC]
	public void ChangeStackRPC(int i, int a)
	{
		if (Items [i].Stackable)
			Items [i].StackAmount = Mathf.Clamp (a, 0, Items [i].StackMax);

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
