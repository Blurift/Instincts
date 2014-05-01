using UnityEngine;
using System.Collections;

public class AIAttack : AIAction {

	public override void Initialize (AI parent)
	{
		base.Initialize(parent);
		transparent = false;
		ActionName = "AIAttack";
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
		else
		{
			ParentAI.Stop (ParentAI.Attack.Attack(ParentAI.Target));

			End ();
		}
	}
}
