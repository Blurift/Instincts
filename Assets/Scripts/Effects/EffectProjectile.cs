using UnityEngine;
using System.Collections;

[AddComponentMenu("EffectsSystem/ProjectileInstant")]
public class EffectProjectile : MonoBehaviour {

	private float timeCreated;

	public float AliveTime;

	//Distancecheck
	private float DistanceTravelled = 0;
	public float DistanceTarget = 0;

	//Properties
	public float Speed = 50;
	public DamageType Damage;

	//
	public GameObject Source;

	// Use this for initialization
	void Start () {
		timeCreated = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - timeCreated > AliveTime)
		{
			Destroy (gameObject);
		}

		if (DistanceTravelled > DistanceTarget)
			Destroy (gameObject);

		Vector3 move = transform.up * Speed * Time.deltaTime;

		DistanceTravelled += Vector3.Distance (Vector3.zero, move);

		transform.position = transform.position + move;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
        if (collision.gameObject == Source)
            return;
        
        Destroy (gameObject);

		HealthSystem h = collision.gameObject.GetComponent<HealthSystem> ();

		if(h != null)
		{
			h.TakeDamage(Damage, Source);
            Debug.Log("Projecitle hit for " + Damage.Damage);
		}
	}
}
