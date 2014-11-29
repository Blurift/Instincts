using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HUD), typeof(HealthSystem), typeof(NetworkView))]
public class PlayerController : EntityController {

	public string PlayerName = "Player";
	private float playerDistance = 100;
	
	public Transform EquipPosition;
    public Transform CameraTarget;
	private GameObject Equipped;
	
	//Movement
	private bool absolute = true;
	
	//Timers
	private float lastFootstep = 0;
	public float FootstepInterval = 1;
    private float deadTime = 0;
	
	//Effects
	public string[] FootStepEffects;
	
	//States
	bool idleWalk = true;
    bool dead = false;
    bool respawned = true;
	
	//Tips
	public bool FirstMovement = false;
	public bool FirstItem = false;
	public bool FirstCraft = false;
	
	
	//Attachments
	
    public GameObject[] Arms;
    public Animator Animator;
	//public Animator ArmsAnimator;
	public GameObject Feet;
	//public Animator FeetAnimator;

    public SpriteRenderer Hair;
    private CameraController cameraController;
    private HUD hud;
    public Inventory Inventory;

	void Awake()
	{
		SetupEntity ();
	}

	// Use this for initialization
	void Start () {
		Health.Death += DeathPlayer;
        Health.Hit += IsHit;
		
		if(networkView.isMine)
		{
			gameObject.AddComponent(typeof(AudioListener));
		}

        Animator = GetComponent<Animator>();
        //ArmsAnimator = (Animator)Arms.GetComponent (typeof(Animator));
        //FeetAnimator = (Animator)Feet.GetComponent (typeof(Animator));
		//Screen.showCursor = false;
		
		if(!Menu.GameTips)
		{
			FirstCraft = true;
			FirstItem = true;
			FirstMovement = true;
		}

        //Set up the camera follow
        if (networkView.isMine)
        {
            Camera.main.transform.position = new Vector3(CameraTarget.position.x, CameraTarget.position.y, Camera.main.transform.position.z);
            cameraController = Camera.main.GetComponent<CameraController>();
            cameraController.Target = CameraTarget;
            cameraController.Set(CameraTarget.position);
            hud = GetComponent<HUD>();
            HUD.Instance = hud;
        }
            
	}

    void IsHit(GameObject s, int d)
    {
        if (networkView.isMine)
            HitRPC();
        else if (Network.isServer)
            networkView.RPC("HitRPC", networkView.owner);
    }

    [RPC]
    void HitRPC()
    {
        if (networkView.isMine)
            cameraController.Shake();
    }
	
    void Update ()
    {
        if (dead)
        {
            if(deadTime < Time.time && respawned == false)
            {
                Respawn();
                respawned = true;
            }
            return;
        }
        if(networkView.isMine)
        {
            if (ItemDrop.ItemToPickUp != null)
            {
                hud.HelperInput("Pickup " + ItemDrop.ItemToPickUp.item.Name, "F", 0.1f);

                if (Vector3.Distance(transform.position, ItemDrop.ItemToPickUp.transform.position) < 2)
                {
                    if (Input.GetButtonDown("Interact") && !hud.HUDFocus)
                    {
                        if (!FirstItem)
                        {
                            FirstItem = true;
                            hud.HelperMessage("You just picked up your first item, Press (I) to open up the inventory and crafting screens. Pick up more items to find new crafting recipes.", 10, 3);
                        }
                        Inventory.AddToInventory(ItemDrop.ItemToPickUp.item.name, ItemDrop.ItemToPickUp.ItemStack, ItemDrop.ItemToPickUp.ItemCharges);
                        ItemManager.RemoveDropFromWorld(ItemDrop.ItemToPickUp.DropID);
                    }
                }
                else
                {
                    ItemDrop.ItemToPickUp = null;
                }
            }

            if (Input.GetButtonDown("Inventory") && !hud.ShowChat)
            {
                hud.ToggleInventory();
            }
            if (Input.GetButtonDown("Start") && hud.ShowChat)
            {
                hud.ToggleChat();

            }
            else if (Input.GetButtonDown("Start") && hud.ShowInventory)
            {
                hud.ToggleInventory();
            }
            else if (Input.GetButtonDown("Start"))
            {
                hud.ToggleMenu();
            }

            if (Input.GetKeyDown(KeyCode.L))
                hud.DebugToggle();

            if (Input.GetKeyDown(KeyCode.Tab))
                hud.PlayerListToggle();
        }
    }

	// Update is called once per frame
	void FixedUpdate () {
        if (dead)
            return;
		UpdateMovement ();

		if (networkView.isMine)
		{
			if(!FirstMovement)
			{
				FirstMovement = true;
				hud.HelperMessage ("Welcome to Instincts " + PlayerName + ". Use the WASD keys to move your character.", 10, 3);
			}

			UpdateInput ();
			
			if(!hud.HUDFocus && Inventory != null && !(Inventory.SelectedIndex < 0 || Inventory.SelectedIndex > 5) )
			{
				Item item = Inventory.Equipment[Inventory.SelectedIndex];
				if(item != null)
				{
					if(Input.GetButton("UseItem"))
					{
						item.UseItem(this,Input.GetButtonDown("UseItem"));
					}
					
					if(Input.GetButtonDown("UseItemAlt"))
					{
						item.UseItemAlt(this);
					}
					
					if(Input.GetButtonDown("Reload"))
					{
						item.Reload(this);
					}
				}

			}

            IsRunning = Input.GetButton("Run");
		}
		else
		{
			playerDistance = Vector2.Distance(Camera.main.transform.position, transform.position);
		}
		
		//Process Effects
		if(!idleWalk && Time.time - lastFootstep > FootstepInterval)
		{
			int soundI = Random.Range(0, FootStepEffects.Length);
			
			EffectManager.CreateEffect(transform.position, FootStepEffects[soundI]);
			
			lastFootstep = Time.time;
		}

        if (Equipped != null)
            Equipped.transform.position = EquipPosition.position;

        //Vector3 v = rigidbody2D.velocity;
        //float feetAngle = Mathf.Atan2(-v.x, v.y);

        //Feet.transform.rotation = Quaternion.Euler(new Vector3(0, 0, feetAngle));

		if (Network.isServer)
			Inventory.VerifyItems ();
	}

    public void SetTopColor(Color color)
    {
        foreach (GameObject go in Arms)
        {
            SpriteRenderer r = go.GetComponent<SpriteRenderer>();
            r.color = color;
        }
    }

    #region Equipped Item

	[RPC]
	public void SetEquipped(string name)
	{
		UnsetEquipped ();
		GameObject obj = ItemInUseManager.Create (name);

		if (obj == null)
						return;

        obj.transform.parent = transform;
        obj.transform.position = EquipPosition.position;
		obj.transform.rotation = transform.rotation;

		Equipped = obj;
	}

	[RPC]
	public void SetEquippedNet(string name, NetworkViewID netID)
	{
		if(Network.isServer)
		{
			NetworkManager.SendRPC(networkView, networkView.owner, "SetEquipped", name);
		}
		UnsetEquipped ();
		GameObject obj = ItemInUseManager.Create (name);
		
		if (obj == null)
			return;

        obj.transform.parent = transform;
        obj.transform.position = EquipPosition.position;
        obj.transform.rotation = transform.rotation;

		if(obj.networkView != null)
			obj.networkView.viewID = netID;

        ItemMeleeEquipped m = obj.GetComponent<ItemMeleeEquipped>();
        if (m != null)
            m.Owner = gameObject;
        ItemRangedEquip r = obj.GetComponent<ItemRangedEquip>();
        if (r != null)
            r.Owner = gameObject;
		
		Equipped = obj;
	}

	public GameObject GetEquipped()
	{
		return Equipped;
	}

	[RPC]
	public void UnsetEquipped()
	{
		if(Equipped != null)
		{
			Destroy (Equipped);
		}
	}

    public void HitEquipped(Vector3 aim)
    {
        if (!Network.isServer)
            networkView.RPC("HitEquippedRPC", RPCMode.Server, aim);
        else
            HitEquippedRPC(aim);
    }

    [RPC]
    void HitEquippedRPC(Vector3 aim)
    {
        if (!Network.isServer)
            return;

        ItemRangedEquip r = Equipped.GetComponent<ItemRangedEquip>();
        if (r != null)
        {
            r.Fire(aim.x, aim.y);
        }
        ItemMeleeEquipped m = Equipped.GetComponent<ItemMeleeEquipped>();
        if (m != null)
        {
            m.Use(aim);
        }
    }

    public void EquippedSetDamage(string t, int d, int ad)
    {
        if (!Network.isServer)
            networkView.RPC("EquippedSetDamageRPC", RPCMode.Server, t, d, ad);
        else
            EquippedSetDamageRPC(t, d, ad);
            
    }

    [RPC]
    public void EquippedSetDamageRPC(string t, int d, int ad)
    {
        if (!Network.isServer)
            return;

        ItemRangedEquip r = Equipped.GetComponent<ItemRangedEquip>();
        if (r != null)
        {
            r.SetDamage(t, d, ad);
        }
        ItemMeleeEquipped m = Equipped.GetComponent<ItemMeleeEquipped>();
        if (m != null)
        {
            m.SetDamage(t, d, ad);
        }
    }

	[RPC]
	public void UseEquipped()
	{
		if(Network.isServer)
		{
			//Send to everyone but the owner
			NetworkManager.SendRPC(networkView, networkView.owner, "UseEquipped");
			//return;
		}

        if (Equipped == null)
            return;
		ItemRangedEquip r = Equipped.GetComponent<ItemRangedEquip>();
		if(r != null)
		{
			r.RangedFireEffects();
		}
		ItemMeleeEquipped m = Equipped.GetComponent<ItemMeleeEquipped> ();
		if(m != null)
		{
			m.UseEffects();
		}
	}
    #endregion

    private void UpdateInput()
	{

		//Handle Movement Input
		{
			float vert = Input.GetAxisRaw ("Vertical");
			float hori = Input.GetAxisRaw ("Horizontal");

			SetAnimIdle (vert == 0 && hori == 0);
			
			Vector3 move = Vector3.zero;
			if (absolute)
				move = (vert * Vector3.up) + (hori * Vector3.right);
			else
				move = (vert * transform.up) + (hori * transform.right);
			if (move != Vector3.zero)
				move.Normalize ();

			UpdateMovement (move);
			
			//Camera.main.transform.position = new Vector3 (transform.position.x, transform.position.y, Camera.main.transform.position.z);
			
			Vector2 worldMouse = Camera.main.ScreenToWorldPoint (Input.mousePosition) - transform.position;
			
            float angle = transform.eulerAngles.z;

            if (new Vector2(hori,vert) != Vector2.zero)
                angle = Mathf.Atan2(-hori, vert) * Mathf.Rad2Deg;

            if(Inventory.SelectedIndex != -1)
                angle = Mathf.Atan2(-worldMouse.x, worldMouse.y) * Mathf.Rad2Deg;

			Rotate (angle);
		}
	}

    private void Respawn()
    {
        //Respawn player
        NetworkManager.Respawn();
    }
	
	[RPC]
	void DeathPlayer ()
	{
        if (dead)
            return;
        dead = true;
        deadTime = Time.time + 2;
        respawned = false;

		if(networkView.isMine)
		{
            hud.SetDead(true);

            UnsetEquipped();

			//Drop player items;
			Inventory.DropAll();
            
		}
		else if(Network.isServer)
		{
			networkView.RPC("DeathPlayer", networkView.owner);
            NetworkManager.PlayerDied(networkView.owner);
		}
	}

    [RPC]
    public void PlayerRespawn(Vector3 position)
    {
        respawned = true;
        dead = false;
        transform.position = position;
        cameraController.Set(position);
        Health.Respawn();
        hud.SetDead(false);

        if (Network.isServer)
            networkView.RPC("PlayerRespawn", RPCMode.Others, position);
    }
	
	//GUI
	public Texture2D BloodScreen;
	public Texture2D HungerIcon;
	private float bloodOpacity = 0;
	private float bloodStartTime = 0;
	private float bloodStartValue = 0;
	private float bloodEndTime = 0;
	private float bloodEndValue = 0;
	private int bloodHealth = 0;
	private float hungerPercent;
	public Texture2D Crosshair;
	
	private Rect KillsPlayerRect = new Rect(10,10,100,20);
	private Rect KillsAlienRect = new Rect(10,30,100,20);
	
	void OnGUI()
	{
		if (networkView.isMine)
		{
			if(false)
			{
				GUI.Label(new Rect(10,10,200,30), "Rotation: " + transform.rotation.eulerAngles.z.ToString());
				GUI.Label(new Rect(10,35,200,30), "Position: " + transform.position.ToString());
				GUI.Label(new Rect(10,60,200,30), "Health: " + Health.Health + " / " + Health.HealthMax);
                GUI.Label(new Rect(10, 85, 200, 30), "Hunger: " + Health.Hunger + " / " + Health.HungerMax);
                GUI.Label(new Rect(10, 110, 200, 30), "Stamina: " + (int)Health.Stamina + " / " + (int)Health.StaminaMax);
                GUI.Label(new Rect(10, 135, 200, 30), "Velocity: " + rigidbody2D.velocity.ToString());
			}
			Color guiColor = Color.white;

			if(bloodOpacity > 0)
			{
				guiColor = Color.white;
				guiColor.a = bloodOpacity;
				GUI.color = guiColor;
				GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height), BloodScreen);
				GUI.color = Color.white;
				
			}
			
			if(Time.time < bloodEndTime)
			{
				float timePassed = Time.time - bloodStartTime;
				float timePercent = timePassed / 0.2f;
				float bloodDifference = bloodEndValue - bloodStartValue;
				float newopacity = (bloodStartValue + bloodDifference*timePercent) / (float)Health.HealthMax;
				bloodOpacity = 1f-newopacity;
			}
			else
			{
				bloodOpacity = 1f-(bloodHealth / (float)Health.HealthMax);
			}
			
			if(Health.Health != bloodHealth)
			{
				bloodStartTime = Time.time;
				bloodStartValue = bloodHealth;
				bloodEndTime = Time.time + 0.2f;
				bloodEndValue = Health.Health;
				bloodHealth = Health.Health;
			}
			
			hungerPercent = 1f - ((float)Health.Hunger / (float)Health.HungerMax);
			guiColor = Color.white;
			guiColor.a = hungerPercent;
			GUI.color = guiColor;
			float hungerSize = Screen.width*0.04f;
			Rect hungerRect = new Rect(Screen.width-hungerSize,Screen.height-hungerSize,hungerSize,hungerSize);
			GUI.DrawTexture(hungerRect, HungerIcon);
			GUI.color = Color.white;
			//GUI.Label(new Rect(Screen.width-hungerSize-100,Screen.height-20,100,20), HealthSystem.Hunger.ToString() + "/" + hungerSize.ToString() + "/" + hungerPercent.ToString());
			
			float crosshairSize = Screen.width *0.01f;
			if(Crosshair != null && !hud.HUDFocus)
				GUI.DrawTexture(new Rect(Input.mousePosition.x-12,Screen.height-Input.mousePosition.y-12,24,24), Crosshair);
			
			//Draw Stats
			//GUI.Label(KillsPlayerRect, KillsPlayer.ToString());
			//GUI.Label(KillsAlienRect, KillsAliens.ToString());
			
		}
		else
		{
			if(playerDistance < 5)
			{
				GUIStyle label = GUI.skin.GetStyle("Label");
				label.alignment = TextAnchor.UpperCenter;
				Vector3 p = new Vector3(transform.position.x,transform.position.y+0.75f, 0);
				Vector2 s = Camera.main.WorldToScreenPoint(p);
				GUI.Label(new Rect(s.x-50,Screen.height-s.y+10,100,20),PlayerName,label);
				label.alignment = TextAnchor.UpperLeft;
			}
		}
	}
	
	[RPC]
	void SetAnimIdle(bool idle)
	{
		if(networkView.isMine)
		{
			networkView.RPC("SetAnimIdle", RPCMode.Others, idle);
		}

        if (Animator != null)
            Animator.SetBool("Idle", idle);

		idleWalk = idle;
	}
	
	[RPC]
	public void SetAnimEquipped(bool equip)
	{
		if(networkView.isMine)
		{
			networkView.RPC("SetAnimEquipped", RPCMode.Others, equip);
		}
		if(Animator != null)
			Animator.SetBool ("Equipped", equip);
	}

	public void SetPlayerState(PlayerState state)
	{
		transform.position = state.Position;
		Health.SetHealthState (state.Health);
		Inventory.SetInvState (state.Inventory);
	}

	public PlayerState GetPlayerState()
	{
		PlayerState state = new PlayerState ();
		state.Position = transform.position;
		state.Health = Health.GetHealthState ();
		state.Inventory = Inventory.GetInvState ();

		return state;
	}

	[System.Serializable]
	public class PlayerState
	{
		public Vector2 Position;
		public HealthSystem.HealthState Health;
		public Inventory.InventoryState Inventory;
	}
}
