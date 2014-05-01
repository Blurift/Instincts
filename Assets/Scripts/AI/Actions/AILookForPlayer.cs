using UnityEngine;
using System.Collections;

public class AILookForPlayer : AIAction {

	private float lastCheck = 0;
	private float checkFrequency = 1f;
	private float maxDistanceToCheck = 15;

	public override void Initialize (AI parent)
	{
		base.Initialize(parent);
		transparent = true;
		ActionName = "AILookForPlayer";
	}
	
	public override void Update ()
	{
		if(Time.time - lastCheck > checkFrequency)
		{
			GameObject playerTarget = null;
			float playerDistance = maxDistanceToCheck;

			Vector3 pos = ParentAIGameObject.transform.position;

			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

			for(int i = 0; i < players.Length; i++)
			{
				float currentPDistance = Helper.DistanceFloatFromTarget(players[i].transform.position, pos);
				if(currentPDistance < playerDistance)
				{
					RaycastHit2D hit = Physics2D.Raycast(ParentAI.transform.position, players[i].transform.position, currentPDistance, ParentAI.Avoid);

					Debug.Log("Trying to find player");
					if(hit.collider == null)
					{
							Debug.Log("Player found");
							playerDistance = currentPDistance;
							playerTarget = players[i];
					}
				}
			}

			if(playerTarget != null)
			{
				ParentAI.ClearActions(new AIChaseTarget());
				ParentAI.Target = playerTarget;
			}

			lastCheck = Time.time;
		}

		base.Update ();
	}
}
