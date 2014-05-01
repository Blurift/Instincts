using UnityEngine;
using System.Collections;

public class SoundEffect : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if(Network.isServer)
		{
			NetworkManager.RemoveNetworkBuffer(networkView.viewID);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(!audio.isPlaying)
			Destroy(gameObject);
	}
}
