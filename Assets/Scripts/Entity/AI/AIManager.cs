using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Blurift;

public class AIManager : MonoBehaviour, GameEventListener {

	public static AIManager Instance;


	public GameObject[] EnemyPre;
	private Dictionary<string, GameObject> EnemyPrefabs = new Dictionary<string, GameObject>();

    //TODO need to add something to spawn different enemies more often.

    public List<AI> Enemies = new List<AI>();

    void Awake()
    {
        Instance = this;
    }

	// Use this for initialization
	void Start () {

        GameEventManager.AddListener("SoundAlert", this);
        GameEventManager.AddListener("SpawnEnemies", this);

		foreach(GameObject go in EnemyPre)
		{
			if(go != null)
			{
				EnemyPrefabs.Add(go.name, go);
			}
		}
	}

    #region GameEventListener

    public void PushEvent(object sender, string type, GameEvent e)
    {
        switch (type)
        {
            case "SoundAlert":
                Vector2 loc = e.Location;
                SoundAlert.SoundAlertEvent sa = (SoundAlert.SoundAlertEvent)e;
                AlertEnemies(sa.SoundDistance, sa.Location);
                break;
            case "SpawnEnemies":
                World.SpawnEnemiesEvent s = (World.SpawnEnemiesEvent)e;
                Debug.Log("AIManager: Spawn Enemy event receieved.");
                SpawnEnemies(s.EnemiesToSpawn, s.Location);

                break;
            default:
                break;
        }
    }

    #endregion

    #region Spawn Enemies
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

    #endregion

    #region Spawn Enemies Event

    private void SpawnEnemies(int n, Vector2 loc)
    {
        for (int i = 0; i < n; i++)
        {

            string name = EnemyPre[Random.Range(0, EnemyPre.Length)].name;
            SpawnEnemy(name, loc);
        }
    }

    private void AlertEnemies(float distance, Vector2 loc)
    {
        foreach (AI enemy in Enemies)
        {
            if(Vector2.Distance(loc, enemy.transform.position) < distance)
            {
                //TODO
            }
        }
    }

    #endregion

    public void RemoveEnemy(AI ai)
    {
        if (Enemies.Contains(ai))
            Enemies.Remove(ai);
    }


}
