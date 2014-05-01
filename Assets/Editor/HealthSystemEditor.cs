/* Name: Health System Editor
 * Desc: Stores and manipulates vitals for a game object
 * Author: Keirron Stach
 * Version: 1.1
 * Created: 22/04/2014
 * Edited: 26/04/2014
 */ 

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(HealthSystem))]
public class HealthSystemEditor : Editor {

	private bool hitEffectsToggle = false;
	private bool hitDropToggle = false;

	public override void OnInspectorGUI ()
	{
		HealthSystem script = (HealthSystem)target;

		//bool dirty = false;



		//Health Values
		EditorGUILayout.LabelField ("Health");
		EditorGUILayout.BeginHorizontal ();
		{
			script.Health = EditorGUILayout.IntField(script.Health);
			script.HealthMax = EditorGUILayout.IntField(script.HealthMax);
		}	
		EditorGUILayout.EndHorizontal ();
		script.HealthRegenerates = EditorGUILayout.Toggle ("Health Regens? ",script.HealthRegenerates);

		//Hunger
		script.HungerEnabled = EditorGUILayout.Toggle ("Hunger", script.HungerEnabled);
		if(script.HungerEnabled)
		{
			EditorGUILayout.BeginHorizontal ();
			{
				script.Hunger = EditorGUILayout.IntField(script.Hunger);
				script.HungerMax = EditorGUILayout.IntField(script.HungerMax);
			}	
			EditorGUILayout.EndHorizontal ();
		}

		//Stamina
		script.StaminaEnabled = EditorGUILayout.Toggle ("Stamina Enabled", script.StaminaEnabled);
		if(script.StaminaEnabled)
		{
			EditorGUILayout.BeginHorizontal ();
			{
				script.Stamina = EditorGUILayout.IntField(script.Stamina);
				script.StaminaMax = EditorGUILayout.IntField(script.StaminaMax);
			}	
			EditorGUILayout.EndHorizontal ();
		}

		EditorGUILayout.Separator ();
		hitEffectsToggle = EditorGUILayout.Foldout (hitEffectsToggle, "Hit Effects");
		if(hitEffectsToggle)
		{
			List<GameObject> hitEffects = new List<GameObject>();
			hitEffects.AddRange(script.HitEffects);
			int delete = -1;

			for(int i = 0; i < hitEffects.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				hitEffects[i] = (GameObject)EditorGUILayout.ObjectField(hitEffects[i],typeof(GameObject), false);
				if(GUILayout.Button("Remove"))
					delete = i;


				EditorGUILayout.EndHorizontal();
			}
			if(delete > -1)
				hitEffects.RemoveAt(delete);


			if(GUILayout.Button("Add"))
			{
				hitEffects.Add(null);
			}

			script.HitEffects = hitEffects.ToArray();
		}

		EditorGUILayout.Separator ();
		hitDropToggle = EditorGUILayout.Foldout (hitDropToggle, "Hit Drop Effects");
		if(hitDropToggle)
		{
			List<GameObject> hitDrops= new List<GameObject>(script.HitDropEffects);
			int delete = -1;

			for(int i = 0; i < hitDrops.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				hitDrops[i] = (GameObject)EditorGUILayout.ObjectField(hitDrops[i],typeof(GameObject), false);
				if(GUILayout.Button("Remove"))
					delete = i;
				
				
				EditorGUILayout.EndHorizontal();
			}
			if(delete > -1)
				hitDrops.RemoveAt(delete);
			
			
			if(GUILayout.Button("Add"))
			{
				hitDrops.Add(null);
			}
			
			script.HitDropEffects = hitDrops.ToArray();
		}


		EditorGUILayout.Separator ();

		if(GUILayout.Button("Save"))
		{
			EditorUtility.SetDirty(script);
		}
	}
}
