using UnityEngine;
using System.Collections;

public class AIAbilityRanged : AIAbility {

	public GameObject ProjectileEffect;
	public GameObject FireEffect;

	public bool Raycast = true;
	public float Range = 15;


	public override bool Use (Vector2 target)
	{
		if (!base.Use (target))
			return false;

		if(FireEffect != null)
			SourceAI.GetComponent<AI>().SyncEffect(target, FireEffect.name);

		Vector2 direction = (new Vector2(SourceAbility.position.x,SourceAbility.position.y) + target).normalized;

		if(Raycast)
		{
			RaycastHit2D hit = Physics2D.Raycast(base.SourceAbility.position,direction,MaximumRange,Layers);
			if(hit.collider != null)
			{
				HealthSystem h = hit.collider.gameObject.GetComponent<HealthSystem>();
				
				if(h != null)
				{
					h.TakeDamage(Damage,SourceAI);
				}
			}
			if (ProjectileEffect != null)
				SourceAI.GetComponent<AI> ().FireProjectile (target, ProjectileEffect.name, null);
		}
		else
		{
			if (ProjectileEffect != null)
				SourceAI.GetComponent<AI> ().FireProjectile (target, ProjectileEffect.name, Damage);
		}

		return true;
	}
}
