using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour {

	public DamageType Damage;
	public float Speed = 1;
	public GameObject Source; 
	public string collisionEffect;
	public string ItemDrop = "";
	public int DropPercent = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//Move projectile untill something is hit or until player cannot see it
		rigidbody2D.transform.position += transform.up * Time.deltaTime * Speed;
	}

	void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject == Source)
			return;
		if(Network.isServer)
		{
			if(collision.gameObject.tag == "Player" || collision.gameObject.tag == "Enemy")
			{
				HealthSystem health = collision.gameObject.GetComponent<HealthSystem>();

				if(health != null)
				{
					health.TakeDamage(Damage, Source);
				}


			}
		}

		if(collisionEffect != null && collisionEffect != "")
		{
			EffectManager.CreateEffect(transform.position, collisionEffect);
		}

		if(ItemDrop != "" && Network.isServer)
		{
			if(Random.Range(1,100) > DropPercent)
			{
				ItemManager.SpawnItem(ItemDrop, gameObject.transform.position);
			}
		}

		Destroy(gameObject);
	}
}
