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
	
	// Update is called once per frame
	void Update () {
	
	}

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
				if(!inv.IsStackAvailable(item.ItemNeeded[j],item.ItemQNeeded[j]))
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
public class CraftableItem
{
	public string ItemName;
	public string[] ItemNeeded;
	public int[] ItemQNeeded;
	[System.NonSerialized]
	public bool Foldout = false;
}
