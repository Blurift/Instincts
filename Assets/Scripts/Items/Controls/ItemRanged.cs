﻿using UnityEngine;
using System.Collections;

public class ItemRanged : ItemBehaviour {
	
	public string Projectile;
	public string FireLightEffect;
	public string FireAnimation;

	public DamageType Damage;

	public int AmmoMax;
	public int Ammo;

	public string AmmoName;

	public float ReloadTime;

	public override void Equip (PlayerController controller)
	{
		base.Equip (controller);
		
		if(currentEquip != null)
		{
			ItemRangedEquip e = (ItemRangedEquip)currentEquip.GetComponent(typeof(ItemRangedEquip));
			e.SetDamage(Damage.Effect.ToString(), Damage.Damage, Damage.AltDamage);
		}
	}

	public override void UseItem (PlayerController controller, Item item)
	{
		if(Ammo > 0)
		{
			controller.networkView.RPC("UseEquipped", RPCMode.Server);
			//Get the vector position the gun is aiming at.
			Vector3 aim3 = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			Vector2 aim = new Vector2 (aim3.x, aim3.y);

			if(currentEquip != null)
			{
				if(Projectile != null && Projectile != "")
				{
					//EffectManager.CreateProjectile(controller.EquipPosition.position, Projectile, controller.transform.rotation, Vector3.Distance(aim3,transform.position));
				}

				ItemRangedEquip ranged = (ItemRangedEquip)currentEquip.GetComponent(typeof(ItemRangedEquip));
				ranged.Fire(aim.x,aim.y);
				Ammo--;
				//networkView.RPC("ItemRangedSetRPC", RPCMode.Server, Ammo);
			}
		}
	}

	public override float Reload(PlayerController controller)
	{
		int reloadAmmo = controller.Inventory.RetrieveFromStack (AmmoName, AmmoMax - Ammo);

		if(reloadAmmo > 0)
			Ammo += reloadAmmo;

		return 1f;
	}

	public override string HUDDisplay ()
	{
		return AmmoName + ": " + Ammo;
	}
}
