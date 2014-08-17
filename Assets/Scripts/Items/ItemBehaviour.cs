﻿using UnityEngine;
using System.Collections;
using System;

//public class ItemBehaviour : MonoBehaviour {
[Serializable]
public class ItemBehaviour {

	[SerializeField]
	public GameObject EquippedPrefab;

	public bool RestoresHunger = false;
	public bool CuresBleeding = false;
	public bool CuresPoison = false;
	public bool IsMeleeWeapon = false;
	public bool IsRangedWeapon = false;
	
	public DamageType Damage;

	public int Charges = 1;
	public int ChargesMax = 1;
	public bool ChargeOverTime = false;

	public string AmmoName;
	
	public int Durability = 100;
	public int DurabilityMax = 100;

	public int HungerToRestore = 0;

	public float ReloadTime = 1;

	[NonSerialized]
	protected GameObject currentEquip;

	public virtual void Equip(PlayerController controller)
	{
		if (EquippedPrefab == null)
			return;

		NetworkViewID viewID = Network.AllocateViewID ();

		controller.SetEquippedNet (EquippedPrefab.name, viewID);
		controller.networkView.RPC ("SetEquippedNet", RPCMode.Server, EquippedPrefab.name, viewID);
		currentEquip = controller.GetEquipped ();

		if(currentEquip != null)
		{
			//Set the damage of the equipped prefabs
			if(IsRangedWeapon)
			{
				ItemRangedEquip e = (ItemRangedEquip)currentEquip.GetComponent(typeof(ItemRangedEquip));
				e.SetDamage(Damage.Effect.ToString(), Damage.Damage, Damage.AltDamage);
			}

			if(IsMeleeWeapon)
			{
				ItemMeleeEquipped e = (ItemMeleeEquipped)currentEquip.GetComponent(typeof(ItemMeleeEquipped));
				e.SetDamage(Damage.Effect.ToString(), Damage.Damage, Damage.AltDamage);
			}
		}

		/* OLD CODE
		currentEquip = (GameObject)Network.Instantiate (EquippedPrefab, controller.EquipPosition.position, Quaternion.identity, 0);
		ItemEquipManager equipManager = currentEquip.GetComponent<ItemEquipManager> ();
		equipManager.EquipPosition = controller.EquipPosition;
		*/
	}

	public virtual void Unequip(PlayerController controller)
	{
		if(currentEquip != null)
		{
			controller.UnsetEquipped();
			controller.networkView.RPC ("UnsetEquipped", RPCMode.Others);

			//((ItemEquipManager)currentEquip.GetComponent (typeof(ItemEquipManager))).Destroy ();
		}
		currentEquip = null;
	}

	public virtual void UseItem(PlayerController controller, Item item)
	{
		if(IsRangedWeapon)
		{

			controller.Inventory.ChangeAtts(item);
		}

		if(IsMeleeWeapon)
		{
			Vector3 aim3 = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			Vector3 aim = new Vector3 (aim3.x, aim3.y,0);
			
			if(currentEquip != null)
			{
				controller.networkView.RPC("UseEquipped", RPCMode.Server);
				ItemMeleeEquipped ranged = (ItemMeleeEquipped)currentEquip.GetComponent(typeof(ItemMeleeEquipped));
				ranged.Use(controller.transform.position, aim);
				controller.UseEquipped();
			}
		}

		if(CuresBleeding)
		{
			HealthSystem health = (HealthSystem)controller.GetComponent (typeof(HealthSystem));
			if (health.ContainsHealthEffect(typeof(HealthBleeding)))
			{
				health.networkView.RPC("BleedingRPC", RPCMode.Server, false);
				item.TakeFromStack(1);
			}
		}

		if(CuresPoison)
		{
		}

		if(RestoresHunger)
		{
			HealthSystem health = (HealthSystem)controller.GetComponent (typeof(HealthSystem));
			health.Hunger = health.Hunger + HungerToRestore;
			//health.networkView.RPC ("ChangeHunger", RPCMode.Server, health.Hunger + HungerToRestore);
			item.TakeFromStack(1);
		}
	}

	public virtual void UseItemAlt(PlayerController controller)
	{
		if(IsRangedWeapon)
		{
		}
		
		if(IsMeleeWeapon)
		{
		}
	}

	public virtual float Reload(PlayerController controller)
	{
		if(IsRangedWeapon)
		{
		}
		return 0;
	}

	public virtual void VerifyControlWithServer(NetworkPlayer player) {}

	public virtual void VerifyControlWithServer() {}
	
	public virtual void CopyFromControl (ItemBehaviour control) {}

	public virtual string HUDDisplay()
	{
		string val = "";
		return "";
	}

	public virtual string DescValue()
	{
		string val = "";
		if(RestoresHunger)
			val += "Hunger: " + HungerToRestore.ToString() + "\n";
		return "";
	}
}
