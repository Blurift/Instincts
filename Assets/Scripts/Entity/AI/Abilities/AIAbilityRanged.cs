using UnityEngine;
using System.Collections;

[AddComponentMenu("AI/Abilities/Ranged")]
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

		Vector2 direction = (target - new Vector2(SourceAbility.position.x,SourceAbility.position.y)).normalized;

		if(Raycast)
		{
			RaycastHit2D hit = Physics2D.Raycast(base.SourceAbility.position,direction,MaximumRange,Layers);
			if(hit.collider != null)
			{
				HealthSystem h = hit.collider.gameObject.GetComponent<HealthSystem>();
				
				if(h != null && hit.collider.gameObject != SourceAI)
				{
					h.TakeDamage(Damage,SourceAI);
				}
			}
			if (ProjectileEffect != null)
				SourceAI.GetComponent<AI> ().FireProjectile (SourceAbility.position, ProjectileEffect.name, null);
		}
		else
		{
			if (ProjectileEffect != null)
				SourceAI.GetComponent<AI> ().FireProjectile (SourceAbility.position, ProjectileEffect.name, Damage);
		}

		return true;
	}
}
