using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectManager : MonoBehaviour {

	public GameObject[] EffectsOriginal;
	public Dictionary<string, GameObject> Effects = new Dictionary<string, GameObject> ();

	public static EffectManager Instance;

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
	}

	[RPC]
	void CreateEffectRPC(Vector3 pos, string effect, Quaternion rotation)
	{
		if(Effects.ContainsKey(effect))
		{
			Instantiate(Effects[effect], pos, rotation);
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
		CreateEffect(pos, effect, Quaternion.identity);
	}

	[RPC]
	void CreateProjectileRPC(Vector3 pos, string effect, Quaternion rotation, float distance)
	{
		if(Effects.ContainsKey(effect))
		{
			GameObject p = (GameObject)GameObject.Instantiate(Effects[effect], pos, rotation);
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
		if (!Effects.ContainsKey (effect))
		{
			Debug.LogError("No effect with this name - (" +  effect + ")");
			return null;
		}
		return (GameObject)Instantiate (Effects [effect], pos, rotation);
	}

	public static GameObject CreateLocal(Vector3 pos, string effect, Quaternion rotation)
	{
		return Instance.CreateLocalP(pos,effect, rotation);
	}

	private int GetEffectIndex(GameObject prefab)
	{
		List<GameObject> effects = new List<GameObject> (EffectsOriginal);

		return effects.IndexOf (prefab);

		//return -1;
	}
}
