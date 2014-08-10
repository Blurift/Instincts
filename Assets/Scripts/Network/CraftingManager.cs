using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingManager : MonoBehaviour {

	public static CraftingManager Instance;

	public CraftableItem[] CraftableItems;

	// Use this for initialization
	void Start () {
		Instance = this;
	}


	/// <summary>
	/// Finds a list of what is available to craft for the player 
	/// </summary>
	/// <returns>The crafts.</returns>
	/// <param name="inv">Inv.</param>
	public static CraftableItem[] AvailableCrafts(Inventory inv)
	{
		List<CraftableItem> crafts = new List<CraftableItem> ();

		CraftableItem[] CraftableItems = Instance.CraftableItems;

		for(int i = 0; i < Instance.CraftableItems.Length; i++)
		{
			bool canCraft = true;
			CraftableItem item = CraftableItems[i];

			for(int j = 0; j < item.ItemNeeded.Length; j++)
			{
				if(!inv.IsStackAvailable(item.ItemNeeded[j].name,item.ItemQNeeded[j]))
				{
					canCraft = false;
				}
			}

			if(canCraft)
				crafts.Add(item);
		}

		return crafts.ToArray();
	}
}

[System.Serializable]
public class CraftableItemV
{
	public string ItemName;
	public string[] ItemNeeded;
	public int[] ItemQNeeded;
	[System.NonSerialized]
	public bool Foldout = false;
}

[System.Serializable]
public class CraftableItem
{
	public Item Item;
	public Item[] ItemNeeded;
	public int[] ItemQNeeded;
	[System.NonSerialized]
	public bool Foldout = false;
}
