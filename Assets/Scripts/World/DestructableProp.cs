﻿/* Name: Destructable Scenery
 * Desc: Stores and manipulates vitals for a game object
 * Author: Keirron Stach
 * Version: 1.1
 * Created: 22/04/2014
 * Edited: 25/04/2014
 */ 

using UnityEngine;
using System.Collections;

public class DestructableProp : MonoBehaviour {

	public int Id = -1;

	private HealthSystem health;

	public GameObject[] DeathEffect;
	public GameObject Drop;

	// Use this for initialization
	void Start () {
		health = GetComponent<HealthSystem> ();
		health.Death += OnDeath;
	}

	public void Death()
	{
		Debug.Log ("Prop dying");
		if(DeathEffect.Length > 0)
		{
			int random = Random.Range(0, DeathEffect.Length);
			Instantiate (DeathEffect[random], transform.position, transform.rotation);
		}

		if (Network.isServer && Drop != null)
		{
			ItemManager.SpawnItem(Drop.name, transform.position);
		}

		Destroy (gameObject);
	}

	private void OnDeath()
	{
		if(Network.isServer)
			DestructableManager.DestroyProp (this);
	}
}
