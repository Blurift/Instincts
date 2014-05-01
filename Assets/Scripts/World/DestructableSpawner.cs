/* Name: Destructable Spawner
 * Desc: Stores and manipulates vitals for a game object
 * Author: Keirron Stach
 * Version: 0.1
 * Created: 22/04/2014
 * Edited: 22/04/2014
 */ 

using UnityEngine;
using System.Collections;

public class DestructableSpawner : MonoBehaviour {

	#region Fields

	public int RespawnTime;
	private float respawnFromSync;

	public GameObject DProp;
	private GameObject dInstance;

	#endregion

	void Start()
	{
		DestructableManager.AddSpawner (this);
	}

	public void DUpdate()
	{
		if(dInstance != null)
		{
			respawnFromSync = Time.time;
			return;
		}

		if(Time.time - respawnFromSync > RespawnTime)
		{
			dInstance = DestructableManager.CreateProp(DProp, transform.position,transform.rotation);
		}
	}
}
