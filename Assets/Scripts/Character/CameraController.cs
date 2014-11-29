using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform Target;
    public float Smoothness = 5f;
    public float ShakeAmount = 1f;
    public float ShakeTime = 0.5f;

    private Vector3 realPosition;
    private Vector3 shakeOffset = Vector3.zero;

    private float shakeStart = 0;
    private Vector3 shakePos = Vector3.zero;

    private float zLock;


	// Use this for initialization
	void Start () {

        realPosition = transform.position;
        zLock = realPosition.z;
	}
	
	// Update is called once per frame
	void Update () {
        if (Target == null)
            return;

        Vector3 targetPos = Target.position;
        targetPos.z = transform.position.z;

        realPosition = Vector3.Lerp(realPosition, targetPos, Smoothness * Time.deltaTime);

        transform.position = realPosition + shakePos;

        if (Time.time < shakeStart + ShakeTime)
        {
            float p = Random.Range(0f, 1f);
            if (p > 0.5f)
            {
                CreateOffset();
            }

            shakePos = Vector3.Lerp(shakePos, shakeOffset, Time.deltaTime);
        }
        else if (shakePos != Vector3.zero) ;
            shakePos = Vector3.Lerp(shakePos, Vector3.zero, Time.deltaTime);
	}

    private void CreateOffset()
    {
        float x = Random.Range(-ShakeAmount, ShakeAmount);
        float y = Random.Range(-ShakeAmount, ShakeAmount);

        shakeOffset = new Vector3(x, y, 0);
    }

    public void Shake()
    {
        shakeStart = Time.time;
        CreateOffset();
    }

    public void Set(Vector2 position)
    {
        realPosition = new Vector3(position.x, position.y, zLock);
        transform.position = realPosition;
    }
}
