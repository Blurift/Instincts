using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour {

    //Depreciated
	public GameObject[] EffectsOriginal;
    public Dictionary<string, GameObject> Effects = new Dictionary<string, GameObject>();


    public EffectListing[] EffectListings;
    public Dictionary<string, EffectListing> EffectDictionary = new Dictionary<string, EffectListing>();

	public static EffectManager Instance;

    private List<Effect> staticEffects = new List<Effect>();
    private List<Effect> dynamicEffects = new List<Effect>();

    #region Inner Classes

    [System.Serializable]
    public class EffectListing
    {
        public EffectType Type = EffectType.Static;
        public GameObject Effect;
        public int Time = 300;
    }

    public enum EffectType
    {
        Static,
        Dynamic,
    }

    private class Effect
    {
        private SpriteRenderer s;
        private GameObject g;
        private float endTime = 0f;

        public Effect(GameObject g, float timer, Quaternion rotation)
        {
            endTime = Time.time + timer;
            this.g = g;
            s = g.GetComponent<SpriteRenderer>();

            g.transform.rotation = rotation;
        }

        public void Update(int i)
        {
            if(s != null)
                s.sortingOrder = i;
            if (Time.time > endTime)
                Destroy(g);
        }

        public bool Alive()
        {
            return g != null;
        }
    }

    #endregion

    // Use this for initialization
	void Start () {
		Instance = this;

		if (EffectsOriginal == null)
						return;
		for(int i = 0; i < EffectsOriginal.Length; i++)
		{
			if(EffectsOriginal[i] != null)
				Effects.Add(EffectsOriginal[i].name, EffectsOriginal[i]);
		}

        for(int i = 0 ; i < EffectListings.Length; i++)
        {
            if (EffectListings[i].Effect != null)
                EffectDictionary.Add(EffectListings[i].Effect.name, EffectListings[i]);
        }
	}

    void Update()
    {
        //Go through all static effects and update them
        for (int i = staticEffects.Count-1; i >= 0; i--)
        {
            staticEffects[i].Update(i);
            if (!staticEffects[i].Alive())
                staticEffects.RemoveAt(i);
        }
    }

	[RPC]
	void CreateEffectRPC(Vector3 pos, string effect, Quaternion rotation)
	{
        if (EffectDictionary.ContainsKey(effect))
		{
            GameObject g = (GameObject)Instantiate(EffectDictionary[effect].Effect, pos, rotation);

            if (EffectDictionary[effect].Type == EffectType.Static)
            {
                Effect e = new Effect(g, EffectDictionary[effect].Time, rotation);
                staticEffects.Add(e);
            }
		}
	}

	public static void CreateNetworkEffect(Vector3 pos, string effect, Quaternion rotation)
	{
		Instance.networkView.RPC ("CreateEffectRPC", RPCMode.All, pos, effect, rotation);
	}

	public static void CreateNetworkEffect(Vector3 pos, string effect)
	{
		CreateNetworkEffect (pos, effect, Quaternion.identity);
	}

	public static void CreateEffect(Vector3 pos, string effect, Quaternion rotation)
	{
		Instance.CreateEffectRPC (pos, effect, rotation);
	}

	public static void CreateEffect(Vector3 pos, string effect)
	{
		CreateEffect(pos, effect, Quaternion.Euler(new Vector3(0,0,Random.Range(0f, 360f))));
	}

	[RPC]
	void CreateProjectileRPC(Vector3 pos, string effect, Quaternion rotation, float distance)
	{
		if(Effects.ContainsKey(effect))
		{
			GameObject p = (GameObject)GameObject.Instantiate(EffectDictionary[effect].Effect, pos, rotation);
			p.GetComponent<EffectProjectile>().DistanceTarget = distance;
		}
	}

	public static void CreateProjectile(Vector3 pos, string effect, Quaternion rotation, float distance)
	{
		Instance.networkView.RPC ("CreateProjectileRPC", RPCMode.Others, pos, effect, rotation, distance);
		Instance.CreateProjectileRPC (pos, effect, rotation, distance);
	}

	private GameObject CreateLocalP(Vector3 pos, string effect, Quaternion rotation)
	{
        if (!EffectDictionary.ContainsKey(effect))
		{
			Debug.LogError("No effect with this name - (" +  effect + ")");
			return null;
		}
		return (GameObject)Instantiate (EffectDictionary[effect].Effect, pos, rotation);
	}

	public static GameObject CreateLocal(Vector3 pos, string effect, Quaternion rotation)
	{
		return Instance.CreateLocalP(pos,effect, rotation);
	}

	private int GetEffectIndex(GameObject prefab)
	{
		List<GameObject> effects = new List<GameObject> (EffectsOriginal);

		return effects.IndexOf (prefab);
	}
}
