using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class Explosion : MonoBehaviour
{
	public LayerMask Layers;
	public DamageType Damage;
	public float Radius = 3;
	public float Delay = 0;
	private float timeStart = 0;

	void Start()
	{
		if (Delay == 0)
			End ();
		timeStart = Time.time;

	}

	void Update()
	{
		if(Time.time - timeStart > Delay)
			End ();
	}

	private void End()
	{
		if(Network.isServer)
		{
			Collider2D[] col = Physics2D.OverlapCircleAll (transform.position, Radius);
			
			for (int i = 0; i < col.Length; i++)
			{
				HealthSystem health = col[i].GetComponent<HealthSystem>();
				
				if(health != null)
					health.TakeDamage(Damage, gameObject);
			}
		}
		Destroy (this);
	}
}
