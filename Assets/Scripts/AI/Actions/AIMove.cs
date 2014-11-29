using UnityEngine;
using System.Collections;

public class AIMove : AIAction {

	public Vector2 Destination;
	public float SpeedModifier = 1;

	public float started;

	// Use this for initialization
	public override void Initialize (AI parent)
	{
		base.Initialize(parent);
		transparent = false;
		ActionName = "AIMove";
		started = Time.time;
	}
	
	public override void Update ()
	{
        Debug.Log("Needs fixing");
        /*
		Vector3 dest = new Vector3(Destination.x,Destination.y,0);
		Vector2 move = Helper.DirectionVector (dest, ParentAIGameObject.transform.position);

		move.Normalize ();

		float angle = Mathf.Atan2 (-move.x,move.y) * Mathf.Rad2Deg;
		ParentAI.transform.rotation = Quaternion.AngleAxis (angle, Vector3.forward);

		move *= ParentAI.Speed;

		if(Helper.DistanceFloatFromDirection(move) < Helper.DistanceFloatFromTarget(dest,ParentAIGameObject.transform.position))
		{
			float moveX = ParentAIGameObject.transform.position.x + move.x * Time.deltaTime * SpeedModifier;
			float moveY = ParentAIGameObject.transform.position.y + move.y * Time.deltaTime * SpeedModifier;

			ParentAIGameObject.rigidbody2D.transform.position = new Vector3 (moveX, moveY,0);
		}
		else
			End();
        */
	}
}
