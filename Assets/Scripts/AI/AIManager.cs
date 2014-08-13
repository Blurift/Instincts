using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

	public static AIManager Instance;

	public int MaxEnemyMultiplier = 6;
	public static int MaxEnemies = 18;
	public static int Enemies = 0;

	public GameObject[] EnemyPre;
	private Dictionary<string, GameObject> EnemyPrefabs = new Dictionary<string, GameObject>();

	// Use this for initialization
	void Start () {

		foreach(GameObject go in EnemyPre)
		{
			if(go != null)
			{
				//Logger.Write("Enemy " + go.name + " loaded");
				EnemyPrefabs.Add(go.name, go);
			}
		}

		Instance = this;
	}

	[RPC]
	void SpawnEnemyRPC(string name, Vector3 position, NetworkViewID id)
	{
		GameObject e = (GameObject)Instantiate (EnemyPrefabs [name], position, Quaternion.identity);
		e.networkView.viewID = id;
	}

	GameObject SpawnEnemy(string name, Vector3 position, NetworkViewID id)
	{
		GameObject e = (GameObject)Instantiate (EnemyPrefabs [name], position, Quaternion.identity);
		e.networkView.viewID = id;
		return e;
	}

	public GameObject SpawnEnemy(string name, Vector3 position)
	{
		//GameManager.WriteMessage ("Spawning " + name + " at " + position.ToString ());
		NetworkViewID id = Network.AllocateViewID ();
		if (Network.isServer)
			networkView.RPC ("SpawnEnemyRPC", RPCMode.Others, name, position, id);
		return SpawnEnemy (name, position, id);
	}

	public void SpawnEnemy(GameObject o, NetworkPlayer player)
	{
		if (Network.isServer)
			networkView.RPC ("SpawnEnemyRPC", player, o.name, o.transform.position, o.networkView.viewID);
	}

	private int PlayersN = 0;

	// Update is called once per frame
	void Update () {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");

		if(PlayersN != players.Length)
		{
			if(players.Length > 0)
			{
				MaxEnemies = MaxEnemyMultiplier * players.Length;
			}

			else
				MaxEnemies = MaxEnemyMultiplier*4;

			if(MaxEnemies < MaxEnemyMultiplier*4)
				MaxEnemies = MaxEnemyMultiplier*4;

			if(MaxEnemies > 30)
				MaxEnemies = 30;

			PlayersN = players.Length;
		}
	}
}
