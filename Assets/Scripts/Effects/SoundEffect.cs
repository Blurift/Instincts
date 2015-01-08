﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("EffectsSystem/Sound Effect")]
public class SoundEffect : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if(Network.isServer)
		{
			NetworkManager.RemoveNetworkBuffer(networkView.viewID);
		}
        audio.volume *= GameManager.SoundLevel;
	}
	
	// Update is called once per frame
	void Update () {
		if(!audio.isPlaying)
			Destroy(gameObject);
	}
}
