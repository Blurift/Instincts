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
    private float angleToTarget = 180;
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
			//AIManager.Enemies ++;

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
            AIManager.Instance.RemoveEnemy(this);

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

        if (angleToTarget > 20)
            return;

		AIAbility use = null;
        int abilityID = -1;

		for(int i = 0; i < Abilities.Length; i++)
		{
			if(use == null)
				use = Abilities[i];

			if(Abilities[i].Weight > use.Weight && Abilities[i].Usable(target))
			{
				//Debug.Log("Setting Ability");
				use = Abilities[i];
                
			}

            if(use != null)
                abilityID = i;
		}

		if (use != null && use.Use (target))
		{
			//Debug.Log("Using Ability");
			GlobalAttackCooldownUntil = Time.time + use.GlobalCooldown;

            if (abilityID > -1)
            {
                this.SetAbilityAnimRPC(abilityID);
                networkView.RPC("SetAbilityAnimRPC", RPCMode.Others, abilityID);
            }
		}

	}

	public bool AbilityRange(Vector2 target)
	{
		for(int i = 0 ; i < Abilities.Length; i++)
		{
			float distance = Vector2.Distance(transform.position, target);

			if(distance > Abilities[i].MinimumRange && distance < Abilities[i].MaximumRange)
			{
				return true;
			}
		}
		return false;
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
        if (Network.isServer)
        {
            AIManager.Instance.RemoveEnemy(this);
            seeker.pathCallback -= OnPathComplete;
        }
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
			networkView.RPC ("FireProjectileRPC", RPCMode.Others, src, eff, rotation);
		
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
		Debug.Log ("Projectile fired");
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

	// TODO
	// Need to work on updating movement a lot and  perform differently based on behaviour.
	private void calculateMovement()
	{
		if(Time.time < StunnedUntil)
			return;

		//Checking if current path is up
		if(CurrentPath != null)
		{
			if(currentWaypoint >= CurrentPath.vectorPath.Count)
			{
				CurrentPath = null;
				return;
			}
		}
		
		bool move = true;
		
		if (Vector3.Distance (currentWaypointPos, transform.position) < 1f)
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

				if(AbilityRange(Target.transform.position))
					move = false;
			}
		}

		Vector3 dir = (currentWaypointPos - transform.position).normalized;
		Rotate (dir);

        float currentAngle = transform.rotation.eulerAngles.z;
        float destAngle = calculateAngle(dir) * Mathf.Rad2Deg;

        angleToTarget = AngleDifferenceFixed(currentAngle, destAngle);
	
        //If able to move and facing the correct direction
		if(move && angleToTarget<45)
		{
			rigidbody2D.transform.position = transform.position += transform.up*Speed*Time.deltaTime;
		}


		SetMove (move);
		
	}

	private Vector2 MoveInRange(Vector2 target)
	{

		return Vector2.zero;
	}
	
	private float calculateAngle(Vector2 dir)
	{
		//Convert Vector2 Direction to a Radian Angle
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

		//Current Angle
		float ca = transform.rotation.eulerAngles.z;

		float diff = AngleDifference (ca, a);


		float n = MaxRotation * Time.deltaTime;

		if(diff<0)
		{
			n*=-1;
			if(diff > n)
				n = diff;
		}
		else
		{
			if(diff < n)
				n = diff;
		}


		return n;
	}

	public static float AngleDifference (float from, float to)
	{
		float difference = to - from;
		
		if(difference > 180)
			difference -= 360;
		if(difference < -180)
			difference += 360;

		return difference;
	}

    public static float AngleDifferenceFixed(float from, float to)
    {
        float dif = AngleDifference(from, to);

        if (dif < 0)
            dif *= -1;

        return dif;
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
	bool isMoving = false;

	public void SetMove(bool move)
	{
		if (move != isMoving)
		{
			isMoving = move;;
			if(Animator != null)
				Animator.SetBool ("Moving", move);
			networkView.RPC ("SetMoveRPC", RPCMode.Others, move);
		}
	}

	[RPC]
	private void SetMoveRPC(bool move)
	{
		if(Animator != null)
			Animator.SetBool ("Moving", move);
	}

	[RPC]
	private void SetAbilityAnimRPC(int ability)
	{
        if (Animator != null)
        {
            Debug.Log("AI Using Ability: " + ability);
            Animator.SetInteger("Ability", ability);
            Animator.SetTrigger("AbilityUse");
        }
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