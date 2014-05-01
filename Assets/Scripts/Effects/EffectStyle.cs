using UnityEngine;
using System.Collections;

[AddComponentMenu ("EffectsSystem/Behaviour")]
public class EffectStyle : MonoBehaviour {

	public bool RandomAngle = false;

	// Use this for initialization
	void Start () {
		if(RandomAngle)
		{
			float randAngle = Random.Range(0f,360f);
			
			transform.rotation = Quaternion.AngleAxis(randAngle, Vector3.forward);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
