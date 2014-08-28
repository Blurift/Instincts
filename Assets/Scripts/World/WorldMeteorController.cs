using UnityEngine;
using System.Collections;

public class WorldMeteorController : MonoBehaviour {

    public float Timeout = 1;
    public bool DetachChildren = true;

    private float startTime = 0;

	// Use this for initialization
	void Start () {
        startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
	    if(Time.time > startTime + Timeout)
        {
            if (DetachChildren)
                transform.DetachChildren();

            if(Network.isServer)
                World.Instance.SpawnMeteoriteSendEvent(transform.position);

            Destroy(this);
        }
	}
}
