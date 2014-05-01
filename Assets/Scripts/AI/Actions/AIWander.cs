using UnityEngine;
using System.Collections;

public class AIWander : AIAction {


	public override void Initialize (AI parent)
	{
		base.Initialize (parent);
		transparent = false;
		ActionName = "AIWander";
	}

	public override void Update ()
	{
		if(ParentAI.PatrolPath != null && ParentAI.CurrentPath == null)
		{
			ParentAI.Speed = ParentAI.BaseSpeed;
			Vector3 target = new Vector3(ParentAI.PatrolPath.Points[ParentAI.PatrolPathIndex].x,ParentAI.PatrolPath.Points[ParentAI.PatrolPathIndex].y,0);
			if(Vector3.Distance(ParentAI.transform.position, target) < ParentAI.PatrolPath.PointRadius)
				ParentAI.PatrolPathIndex++;
			if(ParentAI.PatrolPathIndex >= ParentAI.PatrolPath.Points.Count)
				ParentAI.PatrolPathIndex = 0;
			ParentAI.Move(ParentAI.PatrolPath.Points[ParentAI.PatrolPathIndex]);

		}
		else if(ParentAI.CurrentPath == null)
		{
			ParentAI.Speed = ParentAI.BaseSpeed;
			ParentAI.Move(ParentAI.transform.position + GetRandomDirection());
		}
	}

	private Vector3 GetRandomDirection()
	{
		Vector3 r = new Vector3 (Random.Range (0f, 40f) - 20f, Random.Range (0f, 40f) - 20f,0);
		return r;
	}
}
