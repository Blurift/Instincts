using UnityEngine;
using System.Collections;

public class AIChaseTarget : AIAction {

	private float lastSync = 0;

	public override void Initialize (AI parent)
	{
		base.Initialize(parent);
		transparent = true;
		ActionName = "AIChaseTarget";
	}

	public override void Update ()
	{
		if(ParentAI.Target == null)
		{
			ParentAI.ClearActions (new AILookForPlayer());
			ParentAI.AddAction (new AIWander());
			End ();
			return;
		}

		if(Time.time - lastSync > 2)
		{
			float distance = Helper.DistanceFloatFromTarget (ParentAI.Target.transform.position, ParentAI.transform.position);
			if(distance < ParentAI.Attack.Range)
			{
				ParentAI.AddAction(new AIAttack());
			}

			else if(distance < ParentAI.VisionRange)
			{
				ParentAI.Speed = ParentAI.BaseSpeed * 3.3f;
				ParentAI.Move(ParentAI.Target.transform.position);
			}
			else
			{
				ParentAI.AddAction(new AILookForPlayer());
				ParentAI.AddAction(new AIWander());
				ParentAI.Target = null;
				End ();
			}

			lastSync = Time.time;
		}



	}

	private Vector2 ConvertDirection(Vector2 o, Vector2 d)
	{
		float dRadians = Mathf.Atan2 (d.y, d.x);
		float oRadians = Mathf.Atan2 (o.y, o.x);

		Vector2 displacement = new Vector2 ((float)Mathf.Sin (dRadians + oRadians), (float)Mathf.Sin (dRadians + oRadians));

		return displacement;
	}
}
