using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Inventory : MonoBehaviour {
	
	public Item[] Equipment = new Item[6];
	public Item[] Items = new Item[20];
	public List<Item> ItemsToEquip = new List<Item> ();

	public int SelectedIndex = -1;

	public Vector2 ScreenPosition;

	public PlayerController Player;

	void Awake()
	{
		Equipment = new Item[6];
        Items = new Item[20];
	}

	// Use this for initialization
	void Start () {

		Player = GetComponent<PlayerController> ();

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
		for(int i = 0; i < Items.Length; i++)
		{
			if(item == Items[i])
			{
                if (Network.isServer)
                    DropItemRPC(i, "I");
                else
                    networkView.RPC("DropItemRPC", RPCMode.Server, i, "I");
				Items[i] = null;
				Destroy(item);
                
			}
		}

		for(int i = 0; i < Equipment.Length; i++)
		{
			if(item == Equipment[i])
			{
                if (Network.isServer)
                    DropItemRPC(i, "E");
                else
                    networkView.RPC("DropItemRPC", RPCMode.Server, i, "E");
				Equipment[i] = null;
				Destroy(item);
			}
		}
	}

	[RPC]
	public void DropItemRPC(int i, string type)
	{
		if(type == "I")
		{
            if (Items[i] != null)
            {
                //ItemManager.SpawnItem(Items[i].name, transform.position);
                ItemManager.SpawnItemFromItem(Items[i], transform.position);
                Destroy(Items[i]);
                Items[i] = null;
            }
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

		//If we are not the server tell the server to drop all the items.
		if(!Network.isServer)
		{
            for (int i = Items.Length - 1; i > -1; i--)
			{
				Destroy (Items[i]);
                Items[i] = null;
			}
			
			for(int i = 0; i < Equipment.Length; i++)
			{
				Destroy(Equipment[i]);
				Equipment[i] = null;
			}
			networkView.RPC("DropAll", RPCMode.Server);
			return;
		}

        for (int i = Items.Length - 1; i > -1; i--)
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

        for (int i = 0; i < Items.Length; i++)
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
        for (int i = 0; i < Items.Length; i++)
		{
            if (Items[i] != null && Items[i].name == name)
			{
				return Items[i];
			}
		}
		return null;
	}

	public bool IsStackAvailable(string name, int need)
	{
		int amount = 0;

        for (int i = 0; i < Items.Length; i++)
		{
            
			if(Items[i] != null && Items[i].name == name)
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
        for (int i = 0; i < Items.Length; i++)
		{
            if (Items[i] != null && Items[i].name == name && amount < need)
			{
				amount += Items[i].TakeFromStack(need - amount);
				if(Items[i].StackAmount > 0)
					ChangeStack(i, Items[i].StackAmount);
			}
		}

        for (int i = Items.Length - 1; i >= 0; i--)
		{
            if (Items[i] != null && Items[i].StackAmount <= 0)
			{
				RemoveFromInventory(i, "I");
			}
		}

		return amount;
	}

    /// <summary>
    /// Move item from one spot to another in the inventory.
    /// </summary>
    /// <param name="t"></param>
    /// <param name="target"></param>
    /// <param name="s"></param>
    /// <param name="source"></param>
    public void MoveItem(int t, string target, int s, string source)
    {
        if (Network.isServer && !networkView.isMine)
            networkView.RPC("MoveItemRPC", networkView.owner, t, target, s,  source);
        else if (!Network.isServer && networkView.isMine)
            networkView.RPC("MoveItemRPC", RPCMode.Server, t, target, s, source);
        MoveItemRPC(t, target, s, source);
    }

    [RPC]
    public void MoveItemRPC(int t, string target, int s, string source)
    {
        //Get the item out of the old spot.
        Item m = null;

        if(source == "I")
        {
            m = Items[s];
            Items[s] = null;
        }
        else if(source == "E")
        {
            m = Equipment[s];
            Equipment[s] = null;
        }

        //Return is no item found.
        if (m == null)
            return;

        //Check for item in target spot
        Item mTarget = null;

        if(target == "I")
        {
            mTarget = Items[t];
            Items[t] = m;
        }
        else if(target == "E")
        {
            mTarget = Equipment[t];
            Equipment[t] = m;
        }

        //Move Item back if there is one to move back.
        if (mTarget == null)
            return;

        if(source == "I")
        {
            Items[s] = mTarget;
        }
        else if(source == "E")
        {
            Equipment[s] = mTarget;
        }

    }

    /// <summary>
    /// Moves an item in the equipment bar to the inventory
    /// </summary>
    /// <param name="source">The index of the equipment bar to move item from.</param>
    public void MoveToInventory(int source)
    {

        if (Network.isServer && !networkView.isMine)
            networkView.RPC("MoveToInventoryRPC", networkView.owner, source);
        else if(!Network.isServer && networkView.isMine)
            networkView.RPC("MoveToInventoryRPC", RPCMode.Server, source);
        MoveToInventoryRPC(source);
    }

    [RPC]
    public void MoveToInventoryRPC(int source)
    {
        if (source < 0 || source >= Equipment.Length)
            return;

        if(Equipment[source] != null)
        {
            //Items.Add(Equipment[source]);
            Equipment[source] = null;
        }
    }

    /// <summary>
    /// Moves an item in the inventory to the equipment bar.
    /// </summary>
    /// <param name="source">The index of the inventory to move item from.</param>
    /// <param name="destination">The index of the equipment bar to move item to.</param>
    public void MoveToEquipment(int source, int destination)
    {
        if (Network.isServer)
            networkView.RPC("MoveToEquipmentRPC", networkView.owner, source, destination);
        else
            networkView.RPC("MoveToEquipmentRPC", RPCMode.Server, source, destination);
        MoveToEquipmentRPC(source, destination);
    }

    [RPC]
    public void MoveToEquipmentRPC(int source, int destination)
    {
        if (source < 0 || source >= Items.Length || destination < 0 || destination >= Equipment.Length)
            return;

        //If item already equipped in this slot move it.
        if (Equipment[destination] != null)
        {
            if(SelectedIndex == destination)
            {
                Unequip(destination);
            }
            //Items.Add(Equipment[destination]);
            Equipment[destination] = null;
        }

        Equipment[destination] = Items[source];
        //Items.RemoveAt(source);
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
			Items[i] = null;
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

    #region Add to Inventory

    /// <summary>
    /// Adds an item to the inventory.
    /// </summary>
    /// <param name="name">Name of the item to look up.</param>
    /// <param name="stack">How large the stack of the item is, -1 to use default.</param>
    /// <param name="charges">How many charges the current item has. -1 to use default.</param>
    /// <param name="type">The type of slot to add the item to, "I" to use default.</param>
    /// <param name="index">The index of where to add the item too, -1 to fine available.</param>
    /// <returns></returns>
	public bool AddToInventory(string name, int stack, int charges, string type, int index)
	{
		if(Network.isServer && !networkView.isMine)
			networkView.RPC("AddToInventoryRPC", networkView.owner, name,stack,charges, type, index);
		else if(Network.isClient)
			networkView.RPC("AddToInventoryRPC", RPCMode.Server, name,stack,charges, type, index);

        return AddToInventoryRPC(name, stack, charges, type, index);
	}

	public bool AddToInventory(string name, int stack, int charges)
	{
		return AddToInventory (name, stack, charges, "I", -1);
	}

	[RPC]
	bool AddToInventoryRPC(string name, int stack, int charges, string type, int index)
	{
		ScriptableObject s = ItemManager.CreateItem (name);
		Item item = (Item)s;
        if (stack != -1) item.StackAmount = stack;
		if (charges != -1)		item.BController.Charges = charges;

        //If item to add to inventory is Equipment
		if(type == "E")
		{
            if (Equipment[index] == null)
            {
                Equipment[index] = item;
                return true;
            }
		}

        //It Item to add to inventory is for Inventory slots
        if (type == "I")
        {
            //If item has a set index
            if(index != -1)
            {
                Items[index] = item;
                return true;
            }

            //Search for index,
            index = FreeIndex();


            //If item is not stackable
            if (!item.Stackable && index > -1)
            {
                Items[index] = item;
                return true;
            }

            //If item is stackable go through the inventory to see if their is any stacks to add too.
            if (item.Stackable)
            {

                int stackLeft = stack;

                for (int i = 0; i < Items.Length; i++)
                {
                    Item itemC = Items[i];
                    if (itemC != null && itemC.Name == item.Name)
                    {
                        int stackSpace = itemC.StackMax - itemC.StackAmount;

                        if (stackLeft <= stackSpace)
                        {
                            ChangeStack(i, itemC.StackAmount + stackLeft);

                            stackLeft = 0;
                            Destroy(item);
                            return true;
                        }
                        else
                        {
                            ChangeStack(i, itemC.StackMax);
                            stackLeft -= stackSpace;
                        }
                    }
                }

                if(index > -1)
                {
                    item.StackAmount = stackLeft;
                    Items[FreeIndex()] = item;
                    return true;
                }
            }
        }

        //Check if this is the controller of the inventoryy and warn them their inventory is full.
        if (networkView.isMine)
            HUDN.HelperText("Your inventory is full, you can't pick up this item.");

        return false;
	}

	[RPC]
	public bool AddToInventory(string name, int amount)
	{
		return AddToInventory (name, amount, -1);
	}

    /// <summary>
    /// Find a a place in the inventory that is not holding an item. 
    /// </summary>
    /// <returns>The index of a free slot, -1 if no available slot.</returns>
    public int FreeIndex()
    {
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] == null)
                return i;
        }
        return -1;
    }

    #endregion

    /// <summary>
    /// Change the amount in a stack of an inventory item
    /// </summary>
    /// <param name="i">Index</param>
    /// <param name="a">Amount to change to</param>
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
            for (int i = 0; i < Items.Length; i++)
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

        if (type == "I" && Items.Length > i)
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
			AddToInventory(item.Name,item.Stack,item.Charges, "I", item.Index);
		}

		foreach(Item.ItemState item in state.Equips)
		{
			AddToInventory(item.Name,item.Stack,item.Charges, "E", item.Index);
		}
	}

    /// <summary>
    /// Get the state of the inventory it currently is.
    /// </summary>
    /// <returns></returns>
	public InventoryState GetInvState()
	{
		InventoryState state = new InventoryState ();
        for (int i = 0; i < Items.Length; i++)
		{
            if (Items[i] != null)
            {
                Item.ItemState iState = Items[i].GetItemState();
                iState.Index = i;
                state.Items.Add(iState);
            }
		}

		for(int i = 0; i < Equipment.Length; i++)
		{
            if (Equipment[i] != null)
            {
                Item.ItemState iState = Equipment[i].GetItemState();
                iState.Index = i;
                state.Equips.Add(iState);
            }
		}
		return state;
	}

	public void VerifyItems()
	{
        for (int i = Items.Length - 1; i > -1; i--)
		{
			if(Items[i] != null && Items[i].Stackable && Items[i].StackAmount <= 0)
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
            for (int i = Items.Length - 1; i > -1; i--)
			{
				if(Items[i] == item)
				{
					Items[i] = null;
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
