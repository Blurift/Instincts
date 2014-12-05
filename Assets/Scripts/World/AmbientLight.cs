using UnityEngine;
using System.Collections;

public class AmbientLight : MonoBehaviour {
   
    public Gradient Colors;
    public float MinutesPerDay = 10;

    private Light light;
    private float startTime;
    private float endTime;
    private float duration;

	// Use this for initialization
	void Start () {
        light = GetComponent<Light>();
        Restart();
	}

    private void Restart()
    {
        startTime = Time.time;
        duration = MinutesPerDay * 60;
        endTime = startTime + duration;
    }

    private void Calculate()
    {
        if (!Network.isServer)
            return;

        if (Time.time > endTime)
            Restart();

        float frac = (Time.time - startTime) / duration;
        light.color = Colors.Evaluate(frac);

        networkView.RPC("ChangeColor", RPCMode.Others, light.color.r, light.color.g, light.color.b);
    }

	
	// Update is called once per frame
	void Update () {
        Calculate();
        return;
	}

	[RPC]
	void ChangeColor(float r, float g, float b)
	{
		light.color = new Color(r,g,b);
	}



}
