using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public enum AIBehaviourType
{
	Melee,
	Ranged,
	Suicidal,
}

[AddComponentMenu("AI/AI")]
[RequireComponent(typeof(CircleCollider2D))]
public class AI : MonoBehaviour {

	private HealthSystem HealthSystem;

	public float VisionRange = 15f;
	public LayerMask Avoid;

	public SpriteRenderer SpriteRender;
	public Animator Animator;

	//Misc
	public string[] ItemDrops = new string[0];
	public float GlobalAttackCooldownUntil = 0;
	public float StunnedUntil = 0;

	//AI
	public AIBehaviourType AIType = AIBehaviourType.Melee;

	public GameObject Target;
	public List<AIAction> Actions = new List<AIAction>();
	public AIAbility[] Abilities;

	public float ActionCoolDown = 1;
	private float lastActionCooldown = 0;


	//Pathfinding
	public Path CurrentPath;
	public bool CalculatingPath;
	public Seeker seeker;
	private int currentWaypoint = 0;
	private Vector3 currentWaypointPos = Vector3.zero;

	//Movement
	public float Speed = 1f;
	public float BaseSpeed = 1f;
	public float MaxRotation = 90f;
	public float ChaseModifier = 1.5f;

	public AIPatrolPath PatrolPath;
	public int PatrolPathIndex = 0;

	//Target profiling
	private Dictionary<GameObject, int> targetDamages = new Dictionary<GameObject, int> ();
	private float hitLast = 0;
	private float hitReset = 10;

	//NetworkSyncs
	private float lastSynchronationTime = 0f;
	private float syncDelay = 0f;
	private float syncTime = 0f;
	private Vector3 syncStartPosition = Vector3.zero;
	private Vector3 syncEndPosition = Vector3.zero;


	// Use this for initialization
	void Start () {

		name = name.Replace ("(Clone)", "");

		HealthSystem = (HealthSystem)GetComponent (typeof(HealthSystem));

		HealthSystem.Death += AIDie;
		HealthSystem.Hit += AIHit;

		syncStartPosition = transform.position;
		syncEndPosition = transform.position;

		if(Network.isServer)
		{
			AIManager.Enemies ++;

			switch (AIType)
			{
				case AIBehaviourType.Melee:
					AddAction(new AILookForPlayer());
					AddAction(new AIWander());
					break;

			}

			seeker = (Seeker)GetComponent(typeof(Seeker));
			seeker.pathCallback += OnPathComplete;
		}
	}

	private void AIDie()
	{
		if(Network.isServer)
		{
			AIManager.Enemies --;

			if(ItemDrops != null)
			{
				if(ItemDrops.Length > 0)
				{
					int drop = Random.Range(0,ItemDrops.Length);
					ItemManager.SpawnItem(ItemDrops[drop], transform.position);
				}
			}

			NetworkManager.RemoveNetworkBuffer(networkView.viewID);
			Network.Destroy(gameObject);
		}
	}

	//Event for when the AI gets hit allows it to calculate how much damage has been done by each
	//player and decide who to target
	private void AIHit(GameObject source, int damage)
	{
		//Check if player has hit before
		if(targetDamages.ContainsKey(source))
		{
			targetDamages[source] += damage;
		}
		else
			targetDamages.Add(source,damage);

		hitLast = Time.time;


		//Decide on new target;
		GameObject newTarget = source;
		int highestDamage = 0;

		foreach (KeyValuePair<GameObject, int> hit in targetDamages)
		{
			if(hit.Value > highestDamage)
				newTarget = hit.Key;
		}

		/*GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");

		for (int i = 0; i < players.Length; i++)
		{
			if(players[i].networkView.owner == newTarget)
				Target = players[i];
		}
		*/

		Target = newTarget;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Network.isServer)
		{
			AIAction[] actionList = Actions.ToArray();
			for (int i = 0; i < actionList.Length; i++)
			{
				actionList[i].Update();
				if(!actionList[i].transparent)
					break;
			}

			calculateMovement();

			if(Time.time - hitLast > hitReset)
				targetDamages = new Dictionary<GameObject, int>();
		}
		else
		{
			SyncedMovement();
		}
	}

	private float GetDistance(GameObject player)
	{
		Vector3 distanceVector = player.transform.position - transform.position;
		float distance = Mathf.Sqrt ((distanceVector.x * distanceVector.x) + (distanceVector.y * distanceVector.y));
		return distance;
	}



	/* UseAbility
	 * Works out most appropriate ability to use based on value of abilty and if ability can be used.
	 * Vector2 target: the target the ability is aiming at.
	 */
	public void UseAbility(Vector2 target)
	{
		if (Time.time < GlobalAttackCooldownUntil)
			return;

		AIAbility use = null;

		for(int i = 0; i < Abilities.Length; i++)
		{
			if(use == null)
				use = Abilities[i];

			if(Abilities[i].Weight > use.Weight && Abilities[i].Usable(target))
			{
				Debug.Log("Setting Ability");
				use = Abilities[i];
			}
		}

		if (use != null && use.Use (target))
		{
			Debug.Log("Using Ability");
			GlobalAttackCooldownUntil = Time.time + use.GlobalCooldown;
		}

	}

	public void AddAction(AIAction action)
	{
		if(!Actions.Contains(action))
		{
			action.Initialize(this);
			Actions.Add(action);
		}
	}

	public void RemoveAction(AIAction action)
	{
		if (Actions.Contains (action))
			Actions.Remove (action);
	}

	public void ClearActions(AIAction action)
	{
		if(action != null)
		{
			Actions.Clear();
			AddAction(action);
		}
	}

	void OnDestroy()
	{
		AIManager.Enemies --;
		seeker.pathCallback -= OnPathComplete;
	}

	public void FireProjecitle(Vector3 src, string eff)
	{
		FireProjectile (src, eff, null);
	}

	public void FireProjectile(Vector3 src, string eff, DamageType damage)
	{
		FireProjectile (src, eff, transform.rotation, damage);
	}

	public void FireProjectile(Vector3 src, string eff, Quaternion rotation, DamageType damage)
	{
		if (Network.isServer)
			networkView.RPC ("FireProjectile", RPCMode.Others, src, eff, rotation);
		
		GameObject proj = EffectManager.CreateLocal (src, eff, rotation);
		if(proj != null)
		{
			Projectile p = proj.GetComponent<Projectile>();
			p.Source = gameObject;
			p.Damage = damage;
		}
	}

	[RPC]
	public void FireProjectileRPC(Vector3 src, string eff, Quaternion rotation)
	{
		GameObject proj = EffectManager.CreateLocal (src, eff, rotation);
		if(proj != null)
		{
			Projectile p = proj.GetComponent<Projectile>();
			p.Source = gameObject;
		}
	}



	/*****************************
	 * Sync effects to other AI
	 ****************************/

	public void SyncEffect(Vector3 src, string eff)
	{
		SyncEffect (src, eff, Quaternion.identity);
	}

	public void SyncEffect(Vector3 src, string eff, Quaternion rotation)
	{
		if (Network.isServer)
			networkView.RPC ("FireProjectile", RPCMode.Others, src, eff);

		SyncEffectRPC (src, eff, rotation);
	}

	[RPC]
	public void SyncEffectRPC(Vector3 src, string eff, Quaternion rotation)
	{
		GameObject proj = EffectManager.CreateLocal (src, eff, rotation);
	}


	/*****************************
	 * Methods to move and control movement
	 ****************************/

	private void calculateMovement()
	{
		if(Time.time < StunnedUntil)
			return;



		if(CurrentPath != null)
		{
			if(currentWaypoint >= CurrentPath.vectorPath.Count)
			{
				CurrentPath = null;
				return;
			}
		}
		
		bool move = true;
		
		if (Vector3.Distance (currentWaypointPos, transform.position) < 1.5f)
		{
			
			if(CurrentPath != null)
			{
				currentWaypoint++;
				if(currentWaypoint < CurrentPath.vectorPath.Count)
				{
					currentWaypointPos = CurrentPath.vectorPath[currentWaypoint];
				}
			}
			else
				move = false;
			
			
		}
		
		if(Target != null)
		{
			float distance = Vector3.Distance(Target.transform.position, transform.position);
			
			Vector3 targetDir = (Target.transform.position - transform.position).normalized;
			
			RaycastHit2D hit = Physics2D.Raycast(transform.position, targetDir, distance, Avoid);
			
			if(hit.collider == null)
			{
				//Debug.Log("No obstructions chase player");
				currentWaypointPos = Target.transform.position;


				move = true;
			}
		}

		Vector3 dir = (currentWaypointPos - transform.position).normalized;
		Rotate (dir);
		
		if(move)
		{
			float currentAngle = transform.rotation.eulerAngles.z;
			float destAngle = calculateAngle (dir) * Mathf.Rad2Deg;
			
			float difference = destAngle - currentAngle;
			
			if(difference > 180)
				difference -= 360;
			if(difference < -180)
				difference += 360;
			
			if(difference<0)
				difference *= -1;
			
			if(difference<45)
			{
				rigidbody2D.transform.position = transform.position += transform.up*Speed*Time.deltaTime;

				//TODO: Set Animation To Moving.
			}
		}

		SetMove (move);
		
	}
	
	private float calculateAngle(Vector2 dir)
	{
		return Mathf.Atan2 (-dir.x, dir.y);
	}
	
	public void Rotate(Vector2 dir)
	{
		Rotate (calculateAngle (dir));
	}
	
	public void Rotate(float a)
	{
		float angle = a * Mathf.Rad2Deg;
		transform.Rotate (new Vector3 (0, 0, AngleToRotate (angle)));
	}
	
	public float AngleToRotate(float a)
	{
		float ca = transform.rotation.eulerAngles.z;
		if(a < 0) a+=360;
		float da = a - ca;
		
		if(da > 180 || da < -180)
			da*=-1;
		
		float n = MaxRotation;
		if(da<0)
			n*=-1;
		
		return n * Time.deltaTime;
	}

	public void OnPathComplete(Path p)
	{
		if(!p.error)
		{
			CurrentPath = p;
			currentWaypoint = 0;
			currentWaypointPos = p.vectorPath[0];
		}
		CalculatingPath = false;
	}

	public void Move(Vector3 target)
	{
		if(CalculatingPath)
			return;
		seeker.StartPath (transform.position, target);
		CalculatingPath = true;
	}

	public void Stun(float time)
	{
		SetMove (false);
		StunnedUntil = Time.time + time;
	}

	/*****************************
	 * Animation
	 ****************************/

	public void SetMove(bool move)
	{
		networkView.RPC ("SetMoveRPC", RPCMode.Others, move);
	}

	[RPC]
	private void SetMoveRPC(bool move)
	{
		Animator.SetBool ("Move", move);
	}

	/*****************************
	 * Network control Methods
	 ****************************/

	private void SyncedMovement()
	{
		syncTime += Time.deltaTime;
		transform.position = Vector3.Lerp (syncStartPosition, syncEndPosition, syncTime / syncDelay);
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