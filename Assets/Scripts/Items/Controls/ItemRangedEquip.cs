using UnityEngine;
using System;
using System.Collections;

public class ItemRangedEquip : MonoBehaviour {

	public LayerMask Layers;
	public DamageType Damage;

	public GameObject Owner;
	public GameObject EffectAnimation;
	public GameObject EffectSound;
	public GameObject EffectProjectile;

	public Transform ProjectileSource;

	public bool RaycastProjectile = false;

	public void Fire(float x, float y)
	{
		if(Network.isServer)
			FireRPC(x,y);
	}

	public void FireRPC(float x, float y)
	{
		if(Network.isServer)
		{
            Debug.Log("Ranged: Shooting");
			Vector2 origin = new Vector2(ProjectileSource.position.x,ProjectileSource.position.y);
			
			Vector3 point = new Vector2(x,y) - origin;
			
			Vector2 direction = new Vector2 (point.x, point.y);
			direction.Normalize ();

			if(RaycastProjectile)
			{
				RaycastHit2D[] hit = Physics2D.RaycastAll (origin, direction, 100, Layers);
				if(hit.Length > 0)
				{
					int i = 0;

					while(i < hit.Length)
					{
						if(hit[i].collider != Owner && hit[i].collider != gameObject)
						{
							HealthSystem health = hit[i].collider.GetComponent<HealthSystem>();

							if(health != null)
								health.TakeDamage(Damage, Owner);

							i = hit.Length;
						}
						i++;
					}

				}
			}
			else
			{
				GameObject p = (GameObject)Instantiate(EffectProjectile, ProjectileSource.position, transform.rotation);

				EffectProjectile proj = p.GetComponent<EffectProjectile>();
                proj.Source = Owner;
                proj.Damage = this.Damage;
                Debug.Log("Ranged Damage = " + proj.Damage.Damage);
			}
		}
	}

	[RPC]
	public void RangedFireEffects()
	{
		//if (Network.isServer) return;

		Vector3 p = new Vector3(ProjectileSource.position.x,ProjectileSource.position.y,-3);

		//Will be used to acurately control effects over the network
		if(EffectAnimation != null)
		{
			Instantiate(EffectAnimation, p, transform.rotation);
		}

		if(EffectSound != null)
		{
			Instantiate(EffectSound, p, transform.rotation);
		}

		if(EffectProjectile != null && !(Network.isServer && !RaycastProjectile))
		{
			Instantiate(EffectProjectile, p, transform.rotation);
		}
	}

	public void SetDamage(string t, int d, int ad)
	{
		DamageEffect effect = DamageEffect.None;
		try
		{
			effect = (DamageEffect)Enum.Parse(typeof(DamageEffect), t);
		} catch (Exception ex) {
			
		}
		Damage = new DamageType (effect, d, ad);
	}

}
