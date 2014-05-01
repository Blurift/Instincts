using UnityEngine;
using System.Collections;

public class SpawnerAI : MonoBehaviour {

	public float SpawnFrequency = 10;
	public float SpawnRadius = 200;
	public AIPatrolPath PatrolPath;

	public string[] Enemies;

	private float lastSpawn = 0;
	private AIManager aiManagerScript;



	// Use this for initialization
	void Start () {
		GameObject aiManager = GameObject.FindWithTag("AIManager");
		aiManagerScript = aiManager.GetComponent(typeof(AIManager)) as AIManager;
	}
	
	// Update is called once per frame
	void Update () {
		if(Network.isServer)
		{
			if(Time.time - lastSpawn > SpawnFrequency)
			{
				SpawnEnemy();
				lastSpawn = Time.time;
			}
		}
	}

	private void SpawnEnemy()
	{
		if (AIManager.MaxEnemies > GameObject.FindGameObjectsWithTag("AI").Length)
		{
			if(Enemies.Length>0 && AIManager.MaxEnemies > AIManager.Enemies)
			{
				int index = Random.Range(0,Enemies.Length-1);
				GameObject ai = AIManager.Instance.SpawnEnemy(Enemies[index], transform.position);

				AI aiScript = (AI)ai.GetComponent(typeof(AI));

				aiScript.PatrolPath = PatrolPath;
			}
		}
	}
}
