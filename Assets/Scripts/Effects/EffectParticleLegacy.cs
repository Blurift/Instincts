using UnityEngine;
using System.Collections;

[AddComponentMenu("EffectsSystem/ParticleLegacy")]
public class EffectParticleLegacy : MonoBehaviour {

    private float startTime = 0;
	public float StopEmiting = 0;

    public string RenderLayer;

    void Start()
    {
        startTime = Time.time;
        if (renderer)
            renderer.sortingLayerName = RenderLayer;
    }

    void Update()
    {
        if(startTime + StopEmiting < Time.time)
        {
            particleEmitter.emit = false;
            if (particleEmitter.particleCount == 0)
                Destroy(gameObject);
        }
        
    }
}
