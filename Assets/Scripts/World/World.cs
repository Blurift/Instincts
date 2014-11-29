using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Blurift;

[RequireComponent(typeof(NetworkView))]
public class World : MonoBehaviour, GameEventListener
{

    #region Static

    public static World Instance;

    #endregion 

    #region Lists

    #endregion

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spawnLast = Time.time - 15;
    }

    #region GameEventListener

    public void PushEvent(object sender, string type, GameEvent e)
    {
        switch (type)
        {
            case "PlayerJoined":
                population++;
                AdjustEnemyThreshold();
                break;
            case "PlayerLeft":
                population--;
                AdjustEnemyThreshold();
                break;
            default:
                break;
        }
    }
 
    #endregion

    #region Update

    void Update()
    {
        if (!Network.isServer)
            return;
        SpawnMeteorite();
    }

    #endregion

    #region World Info
    private int population = 0; //Population of the server.
    public int EnemyMultiplier = 6;
    public int EnemyThreshold = 20;

    private void AdjustEnemyThreshold()
    {
        EnemyThreshold = EnemyMultiplier * population;
        if (EnemyThreshold < EnemyMultiplier * 4)
            EnemyThreshold = EnemyMultiplier * 4;

        if (EnemyThreshold > 48)
            EnemyThreshold = 48;
    }

    #endregion

    #region EnemySpawning

    private float spawnLast = -30;
    public GameObject MeteoriteLandingPrefab;
    public float SpawnMinWait = 45;
    public int SpawnLimit = 10;

    /// <summary>
    /// Manage when to send spawn notification and how many to spawn.
    /// </summary>
    private void SpawnMeteorite()
    {
        if(spawnLast + SpawnMinWait < Time.time)
        {
            if(AIManager.Instance.Enemies.Count < EnemyThreshold -5)
            {
                //Debug.Log("World: Spawn Points " + SpawnPoints.Count);
                Vector3 loc = Vector3.zero;
                if(SpawnPoints.Count > 0)
                    loc = SpawnPoints[Random.Range(0,SpawnPoints.Count)];

                if (loc != Vector3.zero)
                {
                    //Debug.Log("World: Spawn Enemies");
                    SpawnMeteoriteRPC(loc);
                    spawnLast = Time.time;
                    //int spawnAmount = Mathf.Min(SpawnLimit, EnemyThreshold - AIManager.Instance.Enemies.Count);
                    //GameEventManager.PushEvent(this, "SpawnEnemies", new SpawnEnemiesEvent(loc, spawnAmount));
                }
            }
        }
    }

    public void SpawnMeteoriteSendEvent(Vector2 location)
    {
        spawnLast = Time.time;
        int spawnAmount = Mathf.Min(SpawnLimit, EnemyThreshold - AIManager.Instance.Enemies.Count);
        GameEventManager.PushEvent(this, "SpawnEnemies", new SpawnEnemiesEvent(location, spawnAmount));
    }

    [RPC]
    void SpawnMeteoriteRPC(Vector3 location)
    {
        if (Network.isServer)
            networkView.RPC("SpawnMeteoriteRPC", RPCMode.Others, location);
        if(MeteoriteLandingPrefab)
        {
            Instantiate(MeteoriteLandingPrefab, location, Quaternion.identity);
        }
    }

    public List<Vector2> SpawnPoints = new List<Vector2>();    

    public class SpawnEnemiesEvent : GameEvent
    {
        private int enemiesToSpawn = 1;

        public SpawnEnemiesEvent(Vector3 location, int enemiesToSpawn) : base(location)
        {
            this.enemiesToSpawn = enemiesToSpawn;
        }

        public int EnemiesToSpawn
        {
            get { return enemiesToSpawn; }
        }
    }

    #endregion
}


