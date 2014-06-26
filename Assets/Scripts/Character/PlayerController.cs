using UnityEngine;
using System.Collections;

public class PlayerController : EntityController {
	
	public HUD HUD;
	
	public string PlayerName = "Player";
	private float playerDistance = 100;
	
	public Transform EquipPosition;
	private GameObject Equipped;
	
	//Movement
	private bool absolute = true;
	
	//Timers
	private float lastFootstep = 0;
	public float FootstepInterval = 1;
	
	//Effects
	public string[] FootStepEffects;
	
	//States
	bool idleWalk = true;
	
	//Tips
	public bool FirstMovement = false;
	public bool FirstItem = false;
	public bool FirstCraft = false;
	
	
	//Attachments
	public Inventory Inventory;
	public GameObject Arms;
	public Animator ArmsAnimator;
	public GameObject Feet;
	public Animator FeetAnimator;

	void Awake()
	{
		SetupEntity ();
	}

	// Use this for initialization
	void Start () {
		Health.Death += DeathRespawn;
		
		if(networkView.isMine)
		{
			gameObject.AddComponent(typeof(AudioListener));
		}
		
		ArmsAnimator = (Animator)Arms.GetComponent (typeof(Animator));
		FeetAnimator = (Animator)Feet.GetComponent (typeof(Animator));
		Screen.showCursor = false;
		
		if(!Menu.GameTips)
		{
			FirstCraft = true;
			FirstItem = true;
			FirstMovement = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateMovement ();

		if (networkView.isMine)
		{
			if(!FirstMovement)
			{
				FirstMovement = true;
				HUD.HelperMessage ("Welcome to Instincts " + PlayerName + ". Use the WASD keys to move your character. Pressing tab will switch movement between absolute and relative types of movement.", 10, 3);
			}

			UpdateInput ();
			
			if(!HUD.HUDFocus && Inventory != null && !(Inventory.SelectedIndex < 0 || Inventory.SelectedIndex > 5) )
			{
				Item item = Inventory.Equipment[Inventory.SelectedIndex];
				if(item != null)
				{
					if(Input.GetButtonDown("UseItem"))
					{
						//item.UseItem(this);
					}

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
			
			if(ItemDrop.ItemToPickUp != null)
			{
				HUD.HelperInput("Pickup " + ItemDrop.ItemToPickUp.Name,"F",0.1f);
				
				if(Vector3.Distance(transform.position, ItemDrop.ItemToPickUp.transform.position) < 2)
				{
					if(Input.GetButtonDown("Interact") && !HUD.HUDFocus)
					{
						if(!FirstItem)
						{
							FirstItem = true;
							HUD.HelperMessage("You just picked up your first item, Press (I) to open up the inventory and crafting screens. Pick up more items to find new crafting recipes.", 10,3);
						}
						Inventory.AddToInventory(ItemDrop.ItemToPickUp);
						ItemDrop.ItemToPickUp.GetComponent<ItemDrop>().RemoveFromWorld();
					}
				}
				else
				{
					ItemDrop.ItemToPickUp = null;
				}
			}
			
			if(Input.GetButtonDown("Inventory") && !HUD.ShowChat)
			{
				HUD.ToggleInventory();
			}
			
			if(Input.GetButtonDown("MovementToggle"))
			{
				absolute = !absolute;
			}
			
			if(Input.GetButtonDown("Start") && HUD.ShowChat)
			{
				HUD.ToggleChat();
				
			}
			else if(Input.GetButtonDown("Start") && HUD.ShowInventory)
			{
				HUD.ToggleInventory();
			}
			else if(Input.GetButtonDown("Start"))
			{
				HUD.ToggleMenu();
			}
			
		}
		
		//Process Effects
		if(!idleWalk && Time.time - lastFootstep > FootstepInterval)
		{
			int soundI = Random.Range(0, FootStepEffects.Length);
			
			EffectManager.CreateEffect(transform.position, FootStepEffects[soundI]);
			
			lastFootstep = Time.time;
		}
	}

	[RPC]
	public void SetEquipped(string name)
	{
		UnsetEquipped ();
		GameObject obj = ItemInUseManager.Create (name);

		if (obj == null)
						return;

		obj.transform.position = EquipPosition.position;
		obj.transform.rotation = transform.rotation;
		obj.transform.parent = transform;

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
		
		obj.transform.position = EquipPosition.position;
		obj.transform.rotation = transform.rotation;
		obj.transform.parent = transform;
		if(obj.networkView != null)
			obj.networkView.viewID = netID;
		
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

	[RPC]
	public void UseEquipped()
	{
		if(Network.isServer)
		{
			NetworkManager.SendRPC(networkView, networkView.owner, "UseEquipped");
			return;
		}
		/*
		 * TODO
		 * Refactring needed / Placeholder mostly
		 */

		ItemRangedEquip r = Equipped.GetComponent<ItemRangedEquip>();
		if(r != null)
		{
			r.RangedFireEffects();
		}
	}
	
	private void UpdateInput()
	{

		//Handle Movement Input
		{
			float vert = Input.GetAxis ("Vertical");
			float hori = Input.GetAxis ("Horizontal");

			SetAnimIdle (vert == 0 && hori == 0);
			
			Vector3 move = Vector3.zero;
			if (absolute)
				move = (vert * Vector3.up) + (hori * Vector3.right);
			else
				move = (vert * transform.up) + (hori * transform.right);
			if (move != Vector3.zero)
				move.Normalize ();

			UpdateMovement (move);
			
			Camera.main.transform.position = new Vector3 (transform.position.x, transform.position.y, Camera.main.transform.position.z);
			
			Vector3 worldMouse = Camera.main.ScreenToWorldPoint (Input.mousePosition) - transform.position;
			
			float angle = Mathf.Atan2 (-worldMouse.x, worldMouse.y) * Mathf.Rad2Deg;

			Rotate (angle);
		}
	}
	
	[RPC]
	void DeathRespawn ()
	{
		if(Network.isServer)
		{
			networkView.RPC("DeathRespawn", networkView.owner);
		}
		else if(networkView.isMine)
		{
			//Drop player items;
			Inventory.DropAll();
			
			//Respawn player
			NetworkManager.Respawn();
			
			
		}
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
			Color guiColor = Color.white;
			GUI.Label(new Rect(10,10,200,30), transform.rotation.eulerAngles.z.ToString());
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
			if(Crosshair != null && !HUD.HUDFocus)
				GUI.DrawTexture(new Rect(Input.mousePosition.x-12,Screen.height-Input.mousePosition.y-12,24,24), Crosshair);
			
			//Draw Stats
			//GUI.Label(KillsPlayerRect, KillsPlayer.ToString());
			//GUI.Label(KillsAlienRect, KillsAliens.ToString());
			
			//GUI.Label(new Rect(Screen.width-100,Screen.height-20,100,20), Menu.VERSION, Menu.LabelRight);
			
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
		ArmsAnimator.SetBool ("Idle", idle);
		FeetAnimator.SetBool ("Idle", idle);
		idleWalk = idle;
	}
	
	[RPC]
	public void SetAnimEquipped(bool equip)
	{
		if(networkView.isMine)
		{
			networkView.RPC("SetAnimEquipped", RPCMode.Others, equip);
		}
		ArmsAnimator.SetBool ("Equipped", equip);
	}
}
