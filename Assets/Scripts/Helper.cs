using UnityEngine;
using System.Collections;

public class Helper {

	public static Vector2 DirectionVector(Vector3 target, Vector3 source)
	{
		float x = target.x - source.x;
		float y = target.y - source.y;
		return new Vector2 (x, y);
	}

	public static float DistanceFloatFromDirection(Vector2 direction)
	{
		return Mathf.Sqrt ((direction.x * direction.x) + (direction.y * direction.y));
	}

	public static float DistanceFloatFromTarget(Vector3 target, Vector3 source)
	{
		return DistanceFloatFromDirection (DirectionVector (target, source));
	}


}
