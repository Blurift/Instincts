/* Name: Test
 * Desc: Used to test other game objects without needing to go through the whole game
 * Author: Keirron Stach
 * Version: 0.6
 * Created: 1/04/2014
 * Edited: 20/04/2014
 */ 

using UnityEngine;
using System.Collections;

[AddComponentMenu("EffectsSystem/BehaviourAll")]
public class EffectBehaviour : MonoBehaviour {

	public float FadeTime;
	public float LightPosition = 1;

	private float startTime;
	private float startIntensity;

	public string SortLayer = "";

    public bool StopEmmitingOnNoParent = false;

    //private Light light;
    //private ParticleSystem particleSystem;

	// Use this for initialization                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
	void Start () {
		startTime = Time.time;

		if(light != null)
		{
			startIntensity = light.intensity;
			transform.position = new Vector3(transform.position.x, transform.position.y, LightPosition*-1);
		}

		if(renderer != null)
			renderer.sortingLayerName = SortLayer;
	}
	
	// Update is called once per frame
	void Update () {
		bool stop = true;

		if(particleSystem != null)
		{
            if(StopEmmitingOnNoParent)
            {
                if(transform.parent == null)
                {
                    //particleSystem.enableEmission = false;
                    particleSystem.Stop();
                }
            }
			if(particleSystem.isPlaying)
				stop = false;
		}

        if(particleEmitter != null)
        {
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
