using UnityEngine;
using System.Collections;

[AddComponentMenu("AI/Abilities/Melee")]
public class AIAbilityMelee : AIAbility {

	public GameObject HitEffect;

	public override bool Use (Vector2 target)
	{
		if (!base.Use (target))
			return false;

		Vector2 direction = (target - new Vector2(SourceAbility.position.x,SourceAbility.position.y)).normalized;

		/*
		 * Check to see if the the ability hits anythign when triggered
		 */
		RaycastHit2D hit = Physics2D.Raycast(base.SourceAbility.position,direction,MaximumRange,Layers);
		if(hit.collider != null )
		{
			HealthSystem h = hit.collider.gameObject.GetComponent<HealthSystem>();

			if(h != null && hit.collider.gameObject != SourceAI)
			{
				h.TakeDamage(Damage,SourceAI);
				if(HitEffect != null)
					SourceAI.GetComponent<AI>().FireProjectile(target, HitEffect.name, null);
			}
		}

		return true;
	}


}
