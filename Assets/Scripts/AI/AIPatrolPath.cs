using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIPatrolPath : MonoBehaviour {

	[SerializeField]
	public List<Vector2> Points;

	[SerializeField]
	public float PointRadius;

	void OnDrawGizmos()
	{

		for (int i = 0; i < Points.Count; i++)
		{
			Gizmos.DrawCube(new Vector3(Points[i].x,Points[i].y,0), new Vector3(0.5f,0.5f,0.5f));
		}
	}
}
