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
        if (ParentAI.CurrentPath == null)
        {
            ParentAI.Speed = ParentAI.BaseSpeed;
            ParentAI.Move(ParentAI.transform.position + GetRandomDirection());
            ParentAI.Stun(1);
        }
	}

	private Vector3 GetRandomDirection()
	{
		Vector3 r = new Vector3 (Random.Range (0f, 40f) - 20f, Random.Range (0f, 40f) - 20f,0);
		return r;
	}
}
