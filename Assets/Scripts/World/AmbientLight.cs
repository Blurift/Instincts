using UnityEngine;
using System.Collections;

public class AmbientLight : MonoBehaviour {

	private bool setting = false;

	public float timeSync = 0.15f;
	private float lastTime = 0;
	private float lightValue = 0.9f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Time.time - lastTime > timeSync && Network.isServer)
		{
			float timeLapse = 0;

			if(lightValue <= 0.2f || lightValue >= 0.8f)
			{
				timeLapse = 0.0003f;
			}
			else if((lightValue > 0.2f && lightValue <= 0.4f) || (lightValue >= 0.6f && lightValue <= 0.8f))
			{
				timeLapse = 0.002f;
			}
			else
			{
				timeLapse = 0.004f;
			}

			if(setting)
			{
				lightValue -= timeLapse;
			}
			else
			{
				lightValue += timeLapse;
			}

			if(lightValue < 0 || lightValue > 1) setting = !setting;

			light.color = new Color(lightValue,lightValue,lightValue,1);

			networkView.RPC("ChangeColor", RPCMode.All, lightValue);

			lastTime = Time.time;
		}
	}

	[RPC]
	void ChangeColor(float color)
	{
		lightValue = color;
		light.color = new Color(lightValue,lightValue,lightValue,1);
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		float syncLight = lightValue;
		if(stream.isWriting)
		{
			stream.Serialize(ref syncLight);
		}
		else
		{
			stream.Serialize(ref syncLight);
			lightValue = syncLight;
			light.color = new Color(lightValue,lightValue,lightValue,1);
		}
	}


}
