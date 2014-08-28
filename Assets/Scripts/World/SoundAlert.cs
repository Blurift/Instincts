using UnityEngine;
using System.Collections;
using Blurift;

public class SoundAlert : MonoBehaviour {

    public float SoundDistance = 5;

	// Use this for initialization
	void Start () {

        if(Network.isServer)
            GameEventManager.PushEvent(gameObject, "SoundAlert", new SoundAlertEvent(transform.position, SoundDistance));

        Destroy(this);
	}

    public class SoundAlertEvent : GameEvent
    {
        public float SoundDistance = 5;

        public SoundAlertEvent(Vector3 location, float soundDistance) : base(location)
        {
            this.SoundDistance = soundDistance;
        }
    }
}
