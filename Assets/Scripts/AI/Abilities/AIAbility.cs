using UnityEngine;
using System.Collections;


public abstract class AIAbility : MonoBehaviour {

	public GameObject SourceAI;
	public Transform SourceAbility;

	public float Cooldown = 3;
	public float GlobalCooldown = 0.6f;
	private float lastUsed = 0;
	public float MinimumRange = 1;
	public float MaximumRange = 3;

	public DamageType Damage;
	public int Weight = 1;

	public LayerMask Layers;

	#region Methods

	public virtual bool Use(Vector2 target)
	{
		//Check to see if the ability is usable before activiting
		if (!Usable (target))
			return false;

		lastUsed = Time.time;
		return true;
	}

	public bool Usable(Vector2 target)
	{
		if(SourceAI == null || SourceAbility == null)
			return false;

		if (Time.time - lastUsed < Cooldown)
						return false;

		float distance = Vector2.Distance (SourceAI.transform.position, target);

		if (distance > MaximumRange || distance < MinimumRange)
			return false;

		return true;
	}

	public bool Usable(Vector3 target)
	{
		return Usable (new Vector2 (target.x, target.y));
	}

	#endregion
}
