using UnityEngine;
using System.Collections;

public class ItemBehaviour : MonoBehaviour {

	public GameObject EquippedPrefab;

	protected GameObject currentEquip;

	public virtual void Equip(PlayerController controller)
	{
		if (EquippedPrefab == null)
			return;

		currentEquip = (GameObject)Network.Instantiate (EquippedPrefab, controller.EquipPosition.position, Quaternion.identity, 0);
		ItemEquipManager equipManager = currentEquip.GetComponent<ItemEquipManager> ();
		equipManager.EquipPosition = controller.EquipPosition;
	}

	public virtual void Unequip(PlayerController controller)
	{
		if(currentEquip != null)
			((ItemEquipManager)currentEquip.GetComponent (typeof(ItemEquipManager))).Destroy ();
		currentEquip = null;
	}

	public virtual void UseItem(PlayerController controller, Item item) {}

	public virtual void UseItemAlt(PlayerController controller) {}

	public virtual float Reload(PlayerController controller) { return 0; }

	public virtual void VerifyControlWithServer(NetworkPlayer player) {}

	public virtual void VerifyControlWithServer() {}
	
	public virtual void CopyFromControl (ItemBehaviour control) {}

	public virtual string HUDDisplay()
	{
		return "";
	}

	public virtual string DescValue()
	{
		return "";
	}
}
