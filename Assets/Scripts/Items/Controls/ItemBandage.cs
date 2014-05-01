using UnityEngine;
using System.Collections;

public class ItemBandage : ItemBehaviour {

	public override void UseItem (PlayerController controller, Item item)
	{
		HealthSystem health = (HealthSystem)controller.GetComponent (typeof(HealthSystem));
		if (health.ContainsHealthEffect(typeof(HealthBleeding)))
		{
			health.networkView.RPC("BleedingRPC", RPCMode.Server, false);
			item.TakeFromStack(1);
		}
	}
}
