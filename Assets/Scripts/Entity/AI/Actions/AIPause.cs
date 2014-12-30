using UnityEngine;
using System.Collections;

public class AIPause : AIAction {

	public float TimeToPause = 1;
	private float TimeStart = -1;

	public AIPause(float timeToPause) { TimeToPause = timeToPause; }

	public override void Initialize (AI parent)
	{
		base.Initialize(parent);
		transparent = false;
		ActionName = "AIPause";
	}
	
	public override void Update ()
	{
		if(TimeStart == -1)
			TimeStart = Time.time;

		if(Time.time - TimeStart > TimeToPause)
		{
			End ();
		}

	}
}
