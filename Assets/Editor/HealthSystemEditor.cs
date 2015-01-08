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

    private const string hostiles_data = "hostiles.Array.data[{0}]";
    private const string hitData = "HitEffects.Array.data[{0}]";
    private const string hitDropData = "HitDropEffects.Array.data[{0}]";

    private SerializedObject m_object;

    private SerializedProperty m_healthMax;
    private SerializedProperty m_healthRegen;

    private SerializedProperty m_staminaMax;
    private SerializedProperty m_staminaOn;

    private SerializedProperty m_hungerMax;
    private SerializedProperty m_hungerOn;
    
    private SerializedProperty m_category;

    private SerializedProperty m_hostiles_size;
    private SerializedProperty m_hitDropSize;
    private SerializedProperty m_hitSize;

    

    private SerializedProperty m_remains;
    private SerializedProperty m_death;



	private bool hitEffectToggle = false;
	private bool hitDropToggle = false;
    private bool factionToggle = false;

    public void OnEnable()
    {
        m_object = new SerializedObject(target);

        m_healthMax = m_object.FindProperty("healthMax");
        m_healthRegen = m_object.FindProperty("healthRegenerates");

        m_staminaMax = m_object.FindProperty("staminaMax");
        m_staminaOn = m_object.FindProperty("staminaEnabled");

        m_hungerMax = m_object.FindProperty("hungerMax");
        m_hungerOn = m_object.FindProperty("hungerEnabled");

        m_category = m_object.FindProperty("entityCategory");
        m_hostiles_size = m_object.FindProperty("hostiles.Array.size");
        m_hitSize = m_object.FindProperty("HitEffects.Array.size");
        m_hitDropSize = m_object.FindProperty("HitDropEffects.Array.size");

        m_remains = m_object.FindProperty("DeathRemainsPrefab");
        m_death = m_object.FindProperty("DeathEffectPrefab");
    }

	public override void OnInspectorGUI ()
    {
        m_object.Update();

        EditorGUILayout.PropertyField(m_healthMax, new GUIContent("Max"));
        EditorGUILayout.PropertyField(m_healthRegen);

        EditorGUILayout.PropertyField(m_hungerOn);
        if (m_hungerOn.boolValue)
        {
            EditorGUILayout.PropertyField(m_hungerMax);
        }

        EditorGUILayout.PropertyField(m_staminaOn);
        if (m_staminaOn.boolValue)
        {
            EditorGUILayout.PropertyField(m_staminaMax);
        }

        EditorGUILayout.Separator();
        factionToggle = EditorGUILayout.Foldout(factionToggle, new GUIContent("Faction Info"));
        if(factionToggle)
        {
            //Set Category and hostile info
            EditorGUILayout.PropertyField(m_category);
            EditorGUILayout.LabelField("Hostile Categories");

            int[] hostiles = GetHostiles();
            int remove = -1;
            for (int i = 0; i < hostiles.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                int result = EditorGUILayout.IntField(hostiles[i]);
                if(GUILayout.Button(new GUIContent("Remove")))
                {
                    remove = i;
                }
                EditorGUILayout.EndHorizontal();

                if (GUI.changed)
                    SetHostile(i, result);
            }
            if(remove > -1)
                RemoveHostile(remove, hostiles);
            if(GUILayout.Button(new GUIContent("Add Hostile")))
            {
                m_hostiles_size.intValue++;
                SetHostile(m_hostiles_size.intValue - 1, 0);
            }
        }

        EditorGUILayout.Separator();
        hitEffectToggle = EditorGUILayout.Foldout(hitEffectToggle, new GUIContent("Hit Effects"));
        if (hitEffectToggle)
        {
            GameObject[] effects = GetHitEffects();
            int remove = -1;
            for (int i = 0; i < effects.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GameObject result = (GameObject)EditorGUILayout.ObjectField(effects[i], typeof(GameObject));
                if (GUILayout.Button(new GUIContent("Remove")))
                {
                    remove = i;
                }
                EditorGUILayout.EndHorizontal();

                if (GUI.changed)
                    SetHitEffect(i, result);
            }
            if (remove > -1)
                RemoveHitEffect(remove, effects);
            if (GUILayout.Button(new GUIContent("Add Effect")))
            {
                m_hitSize.intValue++;
                SetHitEffect(m_hitSize.intValue - 1, null);
            }
        }

        EditorGUILayout.Separator();
        hitDropToggle = EditorGUILayout.Foldout(hitDropToggle, new GUIContent("Hit Drop Effects"));
        if (hitDropToggle)
        {
            GameObject[] effects = GetHitDropEffects();
            int remove = -1;
            for (int i = 0; i < effects.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GameObject result = (GameObject)EditorGUILayout.ObjectField(effects[i], typeof(GameObject));
                if (GUILayout.Button(new GUIContent("Remove")))
                {
                    remove = i;
                }
                EditorGUILayout.EndHorizontal();

                if (GUI.changed)
                    SetHitDropEffect(i, result);
            }
            if (remove > -1)
                RemoveHitDropEffect(remove, effects);
            if (GUILayout.Button(new GUIContent("Add Effect")))
            {
                m_hitDropSize.intValue++;
                SetHitDropEffect(m_hitSize.intValue - 1, null);
            }
        }

        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(m_remains);
        EditorGUILayout.PropertyField(m_death);


        m_object.ApplyModifiedProperties();

        return;
    }

    #region Hostile List Management

    private int[] GetHostiles()
    {
        int size = m_hostiles_size.intValue;
        int[] hostiles = new int[size];

        for (int i = 0; i < size; i++)
        {
            hostiles[i] = m_object.FindProperty(string.Format(hostiles_data, i)).intValue;
        }

        return hostiles;
    }

    private void SetHostile(int index, int value)
    {
        m_object.FindProperty(string.Format(hostiles_data, index)).intValue = value;
    }

    private void RemoveHostile(int index, int[] values)
    {
        for (int i = index; i < m_hostiles_size.intValue - 1; i++)
        {
            SetHostile(i, values[i + 1]);
        }
        m_hostiles_size.intValue--;
    }

    #endregion

    #region Hostile List Management

    private GameObject[] GetHitEffects()
    {
        int size = m_hitSize.intValue;
        GameObject[] effects = new GameObject[size];

        for (int i = 0; i < size; i++)
        {
            effects[i] = m_object.FindProperty(string.Format(hitData, i)).objectReferenceValue as GameObject;
        }

        return effects;
    }

    private void SetHitEffect(int index, GameObject value)
    {
        m_object.FindProperty(string.Format(hitData, index)).objectReferenceValue = value;
    }

    private void RemoveHitEffect(int index, GameObject[] values)
    {
        for (int i = index; i < m_hitSize.intValue - 1; i++)
        {
            SetHitEffect(i, values[i + 1]);
        }
        m_hitSize.intValue--;
    }

    private GameObject[] GetHitDropEffects()
    {
        int size = m_hitDropSize.intValue;
        GameObject[] effects = new GameObject[size];

        for (int i = 0; i < size; i++)
        {
            effects[i] = m_object.FindProperty(string.Format(hitDropData, i)).objectReferenceValue as GameObject;
        }

        return effects;
    }

    private void SetHitDropEffect(int index, GameObject value)
    {
        m_object.FindProperty(string.Format(hitDropData, index)).objectReferenceValue = value;
    }

    private void RemoveHitDropEffect(int index, GameObject[] values)
    {
        for (int i = index; i < m_hitDropSize.intValue - 1; i++)
        {
            SetHitDropEffect(i, values[i + 1]);
        }
        m_hitDropSize.intValue--;
    }

    #endregion
}
