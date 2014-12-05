using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplodeChildren : MonoBehaviour {
    private List<Child> pieces = new List<Child>();
    private float startTime;

    public string ExceptName;
    public float DistanceModifier = 2;

	// Use this for initialization
	void Start () {
        Transform[] children = GetComponentsInChildren<Transform>();

        startTime = Time.time;

        for(int i = 0; i < children.Length; i++)
        {
            if (children[i].transform.parent != transform || children[i].name == ExceptName)
                continue;
            Child c = new Child();

            c.transform = children[i].transform;

            c.time = startTime + Random.Range(0.3f, 0.6f);
            c.duration = c.time - startTime;

            //Random position
            float distance = Vector2.Distance(Vector2.zero, c.transform.localPosition);
            Vector3 v = new Vector3(Random.Range(-distance, distance), Random.Range(-distance, distance), 0);
            c.fromPos = c.transform.localPosition;
            c.toPos = c.fromPos * DistanceModifier + v;
            pieces.Add(c);

            //Angle
            c.fromRot = c.transform.localRotation;
            c.toRot = Quaternion.AngleAxis(Random.Range(-365f, 365f), Vector3.forward);

            //Debug.Log("Explode From: " + c.fromPos + ", To: " + c.toPos + ", Duration: " + c.duration);
        }

	}
	
	// Update is called once per frame
	void Update () {
        bool delete = true;

        for (int i = 0; i < pieces.Count; i++)
        {
            Child c = pieces[i];

            if (c.time > Time.time)
            {
                delete = false;
            }
            else continue;
            float frac = 1-((Time.time - startTime) / c.duration);
            float timer = 1 - ((frac) * (frac));

            c.transform.localPosition = Vector3.Lerp(c.fromPos, c.toPos, timer);
            c.transform.localRotation = Quaternion.Lerp(c.fromRot, c.toRot, timer);
        }

        if (delete)
            Destroy(this);
	}

    private class Child
    {
        public Transform transform;
        public Vector3 fromPos;
        public Vector3 toPos;
        public Quaternion toRot;
        public Quaternion fromRot;
        public float time;
        public float duration;
    }
}
