using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform Target;
    public float Smoothness = 5f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Target == null)
            return;

        Vector3 targetPos = Target.position;
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, Smoothness * Time.deltaTime);
	}
}
