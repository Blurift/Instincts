/* Name: Test
 * Desc: Used to test other game objects without needing to go through the whole game
 * Author: Keirron Stach
 * Version: 0.5
 * Created: 1/04/2014
 * Edited: 20/04/2014
 */ 

using UnityEngine;
using System.Collections;

[AddComponentMenu("Effects/Behaviour")]
public class EffectBehaviour : MonoBehaviour {

	public float FadeTime;
	
	private float startTime;
	private float startIntensity;

	public string SortLayer = "";

	// Use this for initialization                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
	void Start () {
		startTime = Time.time;

		if(light != null)
			startIntensity = light.intensity;

		if(renderer != null)
			renderer.sortingLayerName = SortLayer;
	}
	
	// Update is called once per frame
	void Update () {
		bool stop = true;

		if(particleSystem != null)
		{
			if(particleSystem.isPlaying)
				stop = false;
		}

		if(audio != null)
		{
			if(audio.isPlaying)
				stop = false;
		}

		if(light != null)
		{
			light.intensity = startIntensity * ((FadeTime - (Time.time - startTime)) / FadeTime);
			if(light.intensity < 0)
				light.intensity = 0;
			
			if(Time.time - startTime <= FadeTime)
			{
				stop = false;
			}
		}

		if(animation != null)
		{
			if(animation.isPlaying)
				stop = false;
		}

		if(stop)
		{
			Destroy(gameObject);
		}
	}
}
