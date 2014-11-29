using UnityEngine;
using System.Collections;

[AddComponentMenu ("EffectsSystem/Enviroment")]
public class Effect : MonoBehaviour {

    private static int OrderInLayer = 0;

	public bool RandomAngle = false;
	public bool Permanent = false;

	public float DieTimer = 10f;
	private float timeStarted = 0;

	// Use this for initialization
	void Start () {
	
		timeStarted = Time.time;

		if(RandomAngle)
		{
			float randAngle = Random.Range(0f,360f);

			transform.rotation = Quaternion.AngleAxis(randAngle, Vector3.forward);
		}

        SpriteRenderer s = GetComponent<SpriteRenderer>();

        if(s != null)
        {
            s.sortingOrder = OrderInLayer;
            OrderInLayer++;
        }
	}
	
	// Update is called once per frame
	void Update () {
		if(!Permanent && Time.time - timeStarted > DieTimer)
			Destroy(gameObject);
	}
}
