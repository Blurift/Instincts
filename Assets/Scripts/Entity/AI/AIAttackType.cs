using UnityEngine;
using System.Collections;

public class AIAttackType : MonoBehaviour {

	//public AI AI;
	public Transform Source;

	//Attack Details
	public DamageType Damage;
	public float Range = 1;
	public float UseTime = 1;
	public AttackType Type = AttackType.Melee;
	public bool useRaycast;

	//Attack Effects
	public GameObject ProjectilePrefab;
	public string AttackEffect;

	public bool CanAttack(GameObject target)
	{
		if(Vector3.Distance(transform.position, target.transform.position) < Range)
			return true;
		return false;
	}

	public float Attack(GameObject target)
	{
		if(Network.isServer)
		{
			if(CanAttack(target))
			{
				if(AttackEffect != null && AttackEffect != "")
					EffectManager.CreateNetworkEffect(Source.position, AttackEffect, transform.rotation);

				HealthSystem health = target.GetComponent<HealthSystem>();

				switch(Type)
				{
				case AttackType.Melee:
					if(health != null)
					{
						health.TakeDamage(Damage, gameObject);
					}
					break;
				case AttackType.Ranged:
					if(useRaycast)
					{
						if(health != null)
						{
							health.TakeDamage(Damage, gameObject);
						}
					}
					if(ProjectilePrefab != null)
					{
						CreateProjectile();
						networkView.RPC("CreateProjectile", RPCMode.Others);
					}
					break;
				}
			}
		}

		return UseTime;
	}

	[RPC]
	void CreateProjectile()
	{
		GameObject proj = (GameObject)Instantiate(ProjectilePrefab, Source.position, transform.rotation);
		Projectile p = proj.GetComponent<Projectile>();
		
		p.Source = this.gameObject;
		p.Damage = Damage;
		p.Speed = 2;
	}
}

public enum AttackType
{
	Ranged,
	Melee,
}