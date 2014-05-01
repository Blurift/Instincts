using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(CraftingManager))]
public class CraftingManagerEditor : Editor {

	public override void OnInspectorGUI ()
	{
		//DrawDefaultInspector ();

		CraftingManager script = (CraftingManager)target;

		List<CraftableItem> Items = new List<CraftableItem> ();
		Items.AddRange (script.CraftableItems);

		int deleteItem = -1;
		bool dirty = false;

		for(int i = 0; i < Items.Count; i++)
		{
			EditorGUILayout.BeginHorizontal();
			Items[i].Foldout = EditorGUILayout.Foldout(Items[i].Foldout, Items[i].ItemName);
			if(GUILayout.Button("Delete", GUILayout.Width(Screen.width*0.2f), GUILayout.MinWidth(70)))
			{
				deleteItem = i;
			}
			EditorGUILayout.EndHorizontal();

			if(Items[i].Foldout)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(Screen.width*0.1f);
				string originalName = Items[i].ItemName;
				Items[i].ItemName = EditorGUILayout.TextField(Items[i].ItemName);
				if(originalName != Items[i].ItemName) dirty = true;
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(Screen.width*0.2f);
				EditorGUILayout.LabelField("Ingrediants");
				EditorGUILayout.EndHorizontal();

				List<string> Needs = new List<string>();
				List<int> NeedsQ = new List<int>();

				int deleteIngrediant = -1;

				if(Items[i].ItemNeeded != null)
				{

					Needs.AddRange(Items[i].ItemNeeded);
					NeedsQ.AddRange(Items[i].ItemQNeeded);

					for(int j = 0; j < Needs.Count; j++)
					{
						EditorGUILayout.BeginHorizontal();
						GUILayout.Space(Screen.width*0.2f);
						string oIng = Needs[j];
						int oW = NeedsQ[j];
						Needs[j] = EditorGUILayout.TextField(Needs[j]);
						NeedsQ[j] = EditorGUILayout.IntField(NeedsQ[j]);
						if(oIng != Needs[j] || oW != NeedsQ[j]) dirty = true;
						if(GUILayout.Button("Delete"))
						{
							deleteIngrediant = j;
						}
						EditorGUILayout.EndHorizontal();
					}
				}

				if(deleteIngrediant > -1)
				{
					Needs.RemoveAt(deleteIngrediant);
					NeedsQ.RemoveAt(deleteIngrediant);
					dirty = true;
				}

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(Screen.width*0.2f);
				if(GUILayout.Button("Add"))
				{
					Needs.Add("");
					NeedsQ.Add(1);
					dirty = true;
				}
				EditorGUILayout.EndHorizontal();

				Items[i].ItemNeeded = Needs.ToArray();
				Items[i].ItemQNeeded = NeedsQ.ToArray();
				
			}
		}
		if (deleteItem > -1)
						Items.RemoveAt (deleteItem);

		if(GUILayout.Button("Add"))
		{
			Items.Add(new CraftableItem());
			dirty = true;
		}

		script.CraftableItems = Items.ToArray ();

		if(dirty)
			EditorUtility.SetDirty (target);
	}
}