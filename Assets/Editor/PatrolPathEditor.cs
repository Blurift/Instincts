using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(AIPatrolPath))]
public class PatrolPathEditor : Editor {

	private Vector2 pointScrollPos = Vector2.zero;
	//private bool pointsOn = false;

	private List<Vector2> points;

	public override void OnInspectorGUI ()
	{
		AIPatrolPath script = (AIPatrolPath)target;

		bool dirty = false;

		//Point list
		{
			int deletePoint = -1;

			EditorGUILayout.LabelField("Points");
			pointScrollPos = EditorGUILayout.BeginScrollView(pointScrollPos, GUILayout.MaxHeight(Screen.height/4));
			for (int i = 0; i < script.Points.Count; i++)
			{
				EditorGUILayout.LabelField(script.Points[i].ToString(), GUILayout.Width(Screen.width-80));
			 	EditorGUILayout.BeginHorizontal(GUILayout.Width(Screen.width-80));
				Vector2 temp = script.Points[i];
				script.Points[i] = new Vector2(EditorGUILayout.FloatField(script.Points[i].x),EditorGUILayout.FloatField(script.Points[i].y));
				if(temp != script.Points[i])
					dirty = true;
				if(GUILayout.Button("Delete Point"))
				{
					dirty = true;
					deletePoint = i;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
			//EditorGUILayout.EndToggleGroup();

			if(deletePoint >-1)
			{
				script.Points.RemoveAt(deletePoint);
			}

			if(GUILayout.Button("Add Point"))
			{
				script.Points.Add(SceneView.lastActiveSceneView.camera.transform.position);
				dirty = true;
			}
		}

		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField("Point Radius");
		float pointRadius = script.PointRadius;
		script.PointRadius = EditorGUILayout.FloatField (script.PointRadius);
		if (pointRadius != script.PointRadius)
			dirty = true;
		EditorGUILayout.EndHorizontal ();

		if(dirty)
			EditorUtility.SetDirty(script);

		SceneView.lastActiveSceneView.Repaint ();

	}

	public void OnSceneGUI()
	{
		//AIPatrolPath script = (AIPatrolPath)target;
	}
}
