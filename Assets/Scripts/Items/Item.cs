using UnityEngine;
using System.Collections;
using System;

public class Item : MonoBehaviour {

	public string Name;
	public string Description;
	public float Weight;
	public Texture2D Icon;

	public ItemType IType = ItemType.Material;

	public bool Stackable = false;
	public int StackAmount = 1;
	public int StackMax = 1;

	public float ItemCoolDown;
	private float canUseTime = 0;

	public ItemBehaviour[] ControlList;

	public bool IsOwned = false;
	public NetworkPlayer owner;
	public Inventory invOwner;

	void Start()
	{
		name = name.Replace ("(Clone)", "");
	}

	void Update()
	{
		if(owner != null && IsOwned)
		{
			if(!Network.isServer && owner != Network.player)
			{
				Destroy(gameObject);
			}
		}
	}

	[RPC]
	public void ChangeOwnerRPC(NetworkPlayer owner)
	{
		if(Network.isServer)
		{
			networkView.RPC("ChangeOwnerRPC", RPCMode.OthersBuffered, owner);
		}

		this.owner = owner;
		IsOwned = true;
		if (light != null)
			light.enabled = false;

		SpriteRenderer renderer = (SpriteRenderer)GetComponent (typeof(SpriteRenderer));
		if(owner == null)
			renderer.enabled = true;
		else
			renderer.enabled = false;
	}

	public void UseItem(PlayerController character)
	{
		if(Time.time > canUseTime)
		{
			canUseTime = Time.time + ItemCoolDown;
			for (int i = 0; i < ControlList.Length; i++)
			{
				ControlList[i].UseItem(character, this);
			}
		}
	}
	
	public void UseItemAlt(PlayerController character)
	{
		if(Time.time > canUseTime)
		{
			canUseTime = Time.time + ItemCoolDown;
			for (int i = 0; i < ControlList.Length; i++)
			{
				ControlList[i].UseItemAlt(character);
			}
		}
	}
	
	public void Equip(PlayerController character)
	{
		for (int i = 0; i < ControlList.Length; i++)
		{
			ControlList[i].Equip(character);
		}
	}
	
	public void Unequip(PlayerController character)
	{
		for (int i = 0; i < ControlList.Length; i++)
		{
			ControlList[i].Unequip(character);
		}
	}

	public void Reload(PlayerController character)
	{
		if(Time.time > canUseTime)
		{
			canUseTime = Time.time;
			for (int i = 0; i < ControlList.Length; i++)
			{
				canUseTime += ControlList[i].Reload(character);
			}
		}
	}

	public int TakeFromStack(int need)
	{
		int ret = StackAmount;

		if(StackAmount >= need)
		{
			StackAmount -= need;
			ret = need;
		}
		else
		{
			StackAmount = 0;
		}

		if(StackAmount == 0)
		{
			if(invOwner != null)
			{
				invOwner.VerifyItem(this);
			}

			networkView.RPC("DestroyItem", RPCMode.Server);
		}

		if(!Network.isServer)
			networkView.RPC("ChangeStackRPC", RPCMode.Server, StackAmount);
		ChangeStackRPC (StackAmount);
		return ret;
	}

	public string HUDDisplay()
	{
		string HUD = Name;

		for(int i = 0; i < ControlList.Length; i++)
		{
			HUD += "\n" + ControlList[i].HUDDisplay();
		}

		return HUD;
	}

	public string ItemDesc()
	{
		string desc = "<b>" + Name + "</b>";

		for(int i = 0; i < ControlList.Length; i++)
		{
			if(ControlList[i].DescValue() != "") desc += "\n" + ControlList[i].DescValue();
		}

		desc += "\n" + Description;

		return desc;
	}

	[RPC]
	public void ChangeStackRPC(int amount)
	{
		if(Network.isServer)
		{
			if(!IsOwned)
			{
				networkView.RPC("ChangeStackRPC", RPCMode.Others, amount);
			}
			else
				networkView.RPC("ChangeStackRPC", owner, amount);
		}

		StackAmount = amount;
	}

	[RPC]
	public void ChangeStack(int amount, NetworkPlayer player)
	{
		networkView.RPC("ChangeStackRPC", player, amount);

		ChangeStackRPC (amount);
	}

	[RPC]
	public void DropItemInWorldRPC(Vector3 pos)
	{
		if(Network.isServer)
		{
			if(owner != null)
			{
				GameObject newDrop = ItemManager.SpawnItem(Name, pos);

				Item item = (Item)newDrop.GetComponent(typeof(Item));

				item.CopyFromItem(this);

				DestroyItem();
			}
		}
	}

	[RPC]
	public void DestroyItem()
	{
		if(Network.isServer)
		{
			Network.RemoveRPCs(networkView.viewID);
			if(IsOwned)
			{
				networkView.RPC("DestroyItem", owner);
			}
			else
			{
				networkView.RPC("DestroyItem", RPCMode.Others);
			}
		}
		Destroy (gameObject);
	}

	public void CopyFromItem(Item item)
	{
		this.StackAmount = item.StackAmount;

		for (int i = 0; i < ControlList.Length; i++)
		{
			ControlList[i].CopyFromControl(item.ControlList[i]);
		}

		VerifyItemWithServer ();
	}

	public void VerifyItemWithServer()
	{
		ChangeStackRPC(StackAmount);
		
		for (int i = 0; i < ControlList.Length; i++)
		{
			ControlList[i].VerifyControlWithServer();
		}
	}

	public void VerifyItemWithServer(NetworkPlayer player)
	{
		ChangeStack(StackAmount,player);

		for (int i = 0; i < ControlList.Length; i++)
		{
			ControlList[i].VerifyControlWithServer(player);
		}
	}
}

public enum ItemType
{
	Melee,
	Projectile,
	Usable,
	Outfit,
	Material
}