using UnityEngine;
using System.Collections;

public class WorldMeteorSpawn : MonoBehaviour {

	void Start () {

        World.Instance.SpawnPoints.Add(transform.position);
        Destroy(gameObject);
	}
}
