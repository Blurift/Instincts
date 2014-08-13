using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Item : ScriptableObject {

	public string Name;
	public string Description;
	public float Weight;
	public Texture2D Icon;

	public ItemType IType = ItemType.Material;

	public bool Stackable = false;
	public int StackAmount = 1;
	public int StackMax = 1;

	public float ItemCoolDown = 1;
	private float canUseTime = 0;
	public bool RequiresPress = true;

	[SerializeField]
	//public ItemBehaviour[] ControlList;
	//public List<ItemBehaviour> ControlList;
	public ItemBehaviour BController = new ItemBehaviour ();

	public Inventory invOwner;

	void OnEnable()
	{
		name = name.Replace ("(Clone)", "");
		name = name.Replace ("(Item)", "");
	}



	public void UseItem(PlayerController character, bool pressed)
	{
		if(Time.time > canUseTime && !(RequiresPress && !pressed))
		{
			canUseTime = Time.time + ItemCoolDown;
			BController.UseItem(character, this);
		}
	}
	
	public void UseItemAlt(PlayerController character)
	{
		if(Time.time > canUseTime)
		{
			canUseTime = Time.time + ItemCoolDown;
			//BController.UseItemAlt(character, this);
		}
	}
	
	public void Equip(PlayerController character)
	{
		BController.Equip (character);
	}
	
	public void Unequip(PlayerController character)
	{
		BController.Unequip (character);
	}

	public void Reload(PlayerController character)
	{
		if(Time.time > canUseTime)
		{
			canUseTime = Time.time;
			BController.Reload(character);
		}
	}

	public int TakeFromStack(int need)
	{
		int r = 0;
		if(need  >= StackAmount)
		{
			r = StackAmount;
			StackAmount = 0;
			return r;
		}

		StackAmount -= need;
		return need;
	}

	public string HUDDisplay()
	{
		string HUD = Name;

		HUD += BController.HUDDisplay ();

		return HUD;
	}

	public string ItemDesc()
	{
		string desc = "<b>" + Name + "</b>";

		desc += BController.DescValue ();

		desc += "\n" + Description;

		return desc;
	}

	/// <summary>
	/// Gets the items attributes and returns it as a string;
	/// </summary>
	/// <returns>The item update.</returns>
	public string GetItemUpdate()
	{
		return "";
	}

	/// <summary>
	/// Updates the item with the given parameters
	/// </summary>
	/// <param name="update">Update.</param>
	public void UpdateItem(string update)
	{

	}

#if UNITY_EDITOR
	[MenuItem("Assets/Create/Item")]
	public static void CreateItem()
	{
		Item asset = ScriptableObject.CreateInstance<Item> ();

		AssetDatabase.CreateAsset (asset, "Assets/Prefabs/Items/NewItem.asset");
		AssetDatabase.SaveAssets ();

		EditorUtility.FocusProjectWindow ();

		Selection.activeObject = asset;
	}
#endif
}

public enum ItemType
{
	Melee,
	Projectile,
	Usable,
	Outfit,
	Material
}