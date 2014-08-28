using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor {

	private SerializedObject m_Object;

	private SerializedProperty m_Name;
	private SerializedProperty m_Description;
	private SerializedProperty m_Weight;
	private SerializedProperty m_Icon;
	private SerializedProperty m_EquippedPrefab;

	private SerializedProperty m_Stackable;
	private SerializedProperty m_StackAmount;
	private SerializedProperty m_StackMax;
	private SerializedProperty m_Cooldown;
	private SerializedProperty m_Automatic;
	private SerializedProperty m_AutomaticAlt;

	
	private SerializedProperty m_CuresBleeding;
	private SerializedProperty m_CuresPoison;
	private SerializedProperty m_RestoresHunger;
	private SerializedProperty m_RestoresStamina; //Future Field
	private SerializedProperty m_RestoresHealth; //Future Field
	private SerializedProperty m_IsRanged;
	private SerializedProperty m_IsMelee;

	private SerializedProperty m_HungerToRestore;

	private SerializedProperty m_Damage;
	private SerializedProperty m_Charges;
	private SerializedProperty m_ChargeMax;
	private SerializedProperty m_ChargeRegen;
	private SerializedProperty m_AmmoName;
	private SerializedProperty m_ReloadTime;

	public void OnEnable()
	{
		m_Object = new SerializedObject (target);
		m_Name = m_Object.FindProperty ("Name");
		m_Description = m_Object.FindProperty ("Description");
		m_Weight = m_Object.FindProperty ("Weight");
		m_Icon = m_Object.FindProperty ("Icon");
		m_EquippedPrefab = m_Object.FindProperty ("BController.EquippedPrefab");

		m_Stackable = m_Object.FindProperty ("Stackable");
		m_StackAmount = m_Object.FindProperty ("StackAmount");
		m_StackMax = m_Object.FindProperty ("StackMax");
		m_Cooldown = m_Object.FindProperty ("ItemCoolDown");
		m_Automatic = m_Object.FindProperty ("Automatic");
		m_AutomaticAlt = m_Object.FindProperty ("AutomaticAlt");

		m_CuresBleeding = m_Object.FindProperty ("BController.CuresBleeding");
		m_CuresPoison = m_Object.FindProperty ("BController.CuresPoison");
		m_RestoresHunger = m_Object.FindProperty ("BController.RestoresHunger");
		m_IsRanged = m_Object.FindProperty ("BController.IsRangedWeapon");
		m_IsMelee = m_Object.FindProperty ("BController.IsMeleeWeapon");

		m_HungerToRestore = m_Object.FindProperty ("BController.HungerToRestore");

		m_Damage = m_Object.FindProperty ("BController.Damage");
		m_Charges = m_Object.FindProperty ("BController.Charges");
		m_ChargeMax = m_Object.FindProperty ("BController.ChargesMax");
		m_ChargeRegen = m_Object.FindProperty ("BController.ChargeOverTime");
		m_AmmoName = m_Object.FindProperty ("BController.AmmoName");
		m_ReloadTime = m_Object.FindProperty ("BController.ReloadTime");

	}

	public override void OnInspectorGUI ()
	{
		m_Object.Update ();

		bool oldEnabled = GUI.enabled;

		EditorGUILayout.PropertyField (m_Name);
		//[MultilineAttribute]
		EditorGUILayout.PropertyField (m_Description);
		EditorGUILayout.PropertyField (m_Weight);
		EditorGUILayout.PropertyField (m_Icon);
		EditorGUILayout.PropertyField (m_EquippedPrefab);

		EditorGUILayout.PropertyField (m_Cooldown);
		EditorGUILayout.PropertyField (m_Automatic);
		EditorGUILayout.PropertyField (m_AutomaticAlt);

		EditorGUILayout.PropertyField (m_Stackable, new GUIContent("Stack Options"));

		EditorGUILayout.BeginHorizontal ();
		{


			GUI.enabled = m_Stackable.boolValue && oldEnabled;

			EditorGUILayout.PropertyField (m_StackMax, new GUIContent("Maximum"), GUILayout.Width(Screen.width/2));
			EditorGUILayout.PropertyField (m_StackAmount, GUIContent.none, GUILayout.Width(Screen.width/2));

			GUI.enabled = oldEnabled;
		}
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.Separator ();

		EditorGUILayout.PropertyField(m_RestoresHunger, new GUIContent("Restores Hunger"));
		{

			GUI.enabled = m_RestoresHunger.boolValue && oldEnabled;

			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField (m_HungerToRestore, new GUIContent("Hunger To Restore"));
			EditorGUI.indentLevel--;

			GUI.enabled = oldEnabled;
		}

		EditorGUILayout.Separator ();

		EditorGUILayout.PropertyField (m_CuresBleeding, new GUIContent("Cures Bleeding"));
		EditorGUILayout.PropertyField (m_CuresPoison, new GUIContent("Cures Poison"));

		EditorGUILayout.Separator ();

		EditorGUILayout.PropertyField (m_IsMelee, new GUIContent ("Is Used Melee"));
		{
		}

		EditorGUILayout.PropertyField (m_IsRanged, new GUIContent ("Is Used Ranged"));
		{
		}

		EditorGUILayout.Separator ();

		GUI.enabled = (m_IsMelee.boolValue || m_IsRanged.boolValue) && oldEnabled;
		EditorGUILayout.LabelField ("Weapon Attributes");

		EditorGUI.indentLevel++;
		{

			EditorGUILayout.PropertyField(m_Damage, new GUIContent("Damage Modifier"), true);

			EditorGUILayout.PropertyField(m_Charges, new GUIContent("Initial Charges"));
			EditorGUILayout.PropertyField(m_ChargeMax, new GUIContent("Max Charges"));
			EditorGUILayout.PropertyField(m_ChargeRegen, new GUIContent("Charge Over Time"));
			EditorGUILayout.PropertyField(m_AmmoName, new GUIContent("Name Of Ammo Type"));
			EditorGUILayout.PropertyField(m_ReloadTime, new GUIContent("Time To Reload"));

		}
		EditorGUI.indentLevel--;
		GUI.enabled = oldEnabled;

		m_Object.ApplyModifiedProperties ();

	}
}
