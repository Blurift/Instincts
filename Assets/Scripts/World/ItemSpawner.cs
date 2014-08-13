using UnityEngine;
using System.Collections;

public class ItemSpawner : MonoBehaviour {

	public string[] Items;
	public float SpawnRate;



	public float lastSpawn;
	private GameObject currentSpawn;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Network.isServer)
		{
			if(currentSpawn == null && Time.time - lastSpawn > SpawnRate)
			{

				int itemToSpawn = Random.Range(0,Items.Length);

				currentSpawn = ItemManager.SpawnItem(Items[itemToSpawn], transform.position);

				//string itemLog = "Item " + Items[itemToSpawn] + " was spawned at " + transform.position.ToString();
				//Debug.Log(itemLog);
				//GameManager.WriteMessage(itemLog);

				lastSpawn = Time.time;
			}

			if(currentSpawn != null)
			{
				ItemDrop itemDrop = (ItemDrop)currentSpawn.GetComponent(typeof(ItemDrop));

				lastSpawn = Time.time;
				itemDrop.DespawnTime = Time.time + 120;
			}
		}
		else if(Network.isClient)
		{
			//Destroy (gameObject);
		}
	}
}
