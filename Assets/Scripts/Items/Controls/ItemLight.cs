using UnityEngine;
using System.Collections;

public class ItemLight : ItemBehaviour {

	public override void Equip (PlayerController controller)
	{
		Vector3 pos = controller.EquipPosition.position;
		pos.z = -0.5f;
		currentEquip = (GameObject)Network.Instantiate (EquippedPrefab, pos, Quaternion.identity, 0);


		ItemEquipManager equipManager = (ItemEquipManager)currentEquip.GetComponent(typeof(ItemEquipManager));

		equipManager.EquipPosition = controller.EquipPosition;

		//currentEquip.transform.parent = controller.transform;
	}
}
