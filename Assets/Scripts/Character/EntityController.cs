using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HealthSystem), typeof(Rigidbody2D), typeof(NetworkView))]
public class EntityController : MonoBehaviour {

	#region Fields

	//Movement
	private float speed = 5;
	private float maxRotation = 360;

	//Network syncs
	private float lastSynchronationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;

	//Player Vitals
	private HealthSystem health;

	#endregion

	#region Properties

	public HealthSystem Health
	{
		get { return health; }
	}

	#endregion

	void Awake ()
	{
		SetupEntity ();
	}

	protected virtual void SetupEntity()
	{
		health = GetComponent<HealthSystem> ();
		syncStartPosition = transform.position;
		syncEndPosition = transform.position;
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		UpdateMovement ();
	}

	protected virtual void UpdateMovement()
	{
		if(!networkView.isMine)
			SyncedMovement();
	}

	/// <summary>
	/// Updates the movement by moving the gameObject towards
	/// the target vector by its maximum speed.
	/// </summary>
	/// <param name="target">Target vector position</param>
	protected void UpdateMovement(Vector3 target)
	{
		if(target == null) return;

		if(target != Vector3.zero)
			target.Normalize();

		transform.position += target * Time.deltaTime * speed;
        //rigidbody2D.MovePosition(transform.position + target * Time.deltaTime * speed);
	}

	/// <summary>
	/// Rotate towards the specified angle
	/// </summary>
	/// <param name="Angle">Angle to rotate towards</param>
	protected void Rotate(float angle)
	{
		float current = transform.rotation.eulerAngles.z;
		float max = maxRotation * Time.deltaTime;

		float difference = angle - current;
		float neg = 1;

		if(difference < 0)
		{
			difference *= -1;
			neg = -1;
		}
		if(difference > 180)
			difference -= 360;

		difference = Mathf.Min (difference, max);

		transform.Rotate (Vector3.forward, difference * neg);
	}

	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		transform.position = Vector3.Lerp (syncStartPosition, syncEndPosition, syncTime / syncDelay);
	}

	public void ResetPosition(Vector3 pos )
	{
		transform.position = pos;
		syncStartPosition = pos;
		syncEndPosition = pos;
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 syncPosition = rigidbody2D.transform.position;
		Vector3 syncVelocity = rigidbody2D.velocity;
		Quaternion syncRotation = rigidbody2D.transform.rotation;
		if (stream.isWriting)
		{
			syncPosition = rigidbody2D.transform.position;
			stream.Serialize(ref syncPosition);
			
			syncVelocity = rigidbody2D.velocity;
			stream.Serialize(ref syncVelocity);
			
			syncRotation = rigidbody2D.transform.rotation;
			stream.Serialize(ref syncRotation);
		}
		else
		{
			stream.Serialize(ref syncPosition);
			stream.Serialize(ref syncVelocity);
			stream.Serialize(ref syncRotation);
			
			syncTime = 0f;
			syncDelay = Time.time - lastSynchronationTime;
			lastSynchronationTime = Time.time;
			
			syncStartPosition = rigidbody2D.transform.position;
			syncEndPosition = syncPosition +syncVelocity * syncDelay;
			rigidbody2D.transform.rotation = syncRotation;
		}
	}

}
