using UnityEngine;
using System.Collections;

[AddComponentMenu("EffectsSystem/LockParticles")]
[RequireComponent(typeof(ParticleSystem))]
public class LockParticles : MonoBehaviour {

    public float Z = 0;
    private ParticleSystem ps;

	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
        ParticleSystem.Particle[] par = new ParticleSystem.Particle[ps.particleCount];
        ps.GetParticles(par);
        for (int i = 0; i < par.Length; i++ )
        {
            par[i].position = new Vector3(par[i].position.x, par[i].position.y, Z);
        }
        ps.SetParticles(par, ps.particleCount);
	}
}
