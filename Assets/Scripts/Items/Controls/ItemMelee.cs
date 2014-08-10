using UnityEngine;
using System.Collections;

public class ItemMelee : ItemBehaviour {

	public DamageType Damage;

	public override void Equip (PlayerController controller)
	{
		base.Equip (controller);

		if(currentEquip != null)
		{
			ItemMeleeEquipped e = (ItemMeleeEquipped)currentEquip.GetComponent(typeof(ItemMeleeEquipped));
			e.SetDamage(Damage.Effect.ToString(), Damage.Damage, Damage.AltDamage);
		}
	}

	public override void UseItem (PlayerController controller, Item item)
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

	public override void UseItemAlt (PlayerController controller)
	{

	}

	public override string DescValue ()
	{
		return "Damage: " + Damage.Damage + "\nType: " + Damage.Effect.ToString ();
	}
}
