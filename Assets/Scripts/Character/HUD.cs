using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using Blurift;

public class HUD : MonoBehaviour {

	public static HUD Instance;
	public PlayerController Controller;
	public Inventory Inventory;

    #region Variables

    //Textures
	public Texture2D EquipmentRack;
	public Texture2D SelectedEquip;
	public Texture2D IconDropItem;
	public Texture2D Filler;

	//Window ID's
	private const int windowInv = 1;
	private const int windowCrafting = 2;
	private const int windowMenu = 3;

	//Drag and drop GUI variables
	private int dragIndex = -1;
	private int dragSource = -1; //0 = Inventory; 1 = Equipment; 2 = Container;
	private Item dragItem = null;
	private Texture2D dragIcon = null;
	private bool mouseDown = false;
	private Vector2 mousePosition = Vector2.zero;

	//Menu
	public bool ShowMenu
	{
		get { return showMenu; }
	}
	[System.NonSerialized]
	private bool showMenu = false;

	//GUI variables
    float width = 0;
    float height = 0;

	private Rect inventoryWindow = new Rect(0,0,216,300);

	private Rect craftWindow = new Rect(Screen.width-200,0,200,300);
	private Rect craftScrollRect = new Rect (8, 28, 200, 237);
	private Vector2 craftScrollPosition = Vector2.zero;
	private int craftIndex = -1;

	public float ChatWidthPercent = 0.25f;

	private Vector2 chatScrollPosition = Vector2.zero;

	private int itemHover = -1;
	private int craftingHover = -1;
	
	//GUI style variables
	private Color boxStandard = new Color(0.3f,0.3f,0.3f);
	private Color boxHover = new Color(0.5f,0.5f,0.5f);
	
	
    private GUIStyle invDescStyle;
	public GUISkin InvSkin;

    //Inventory Window
    float invScrollViewWidth = 0;
    float invButtonSize = 0;
    float invButtonMargin = 0;
    float invButtonInner = 0;
    float invScrollViewHeight = 0;

    private Rect invScrollView;
    private Rect invScrollInner;
    private Vector2 inventoryScrollPosition = Vector2.zero;

    private GUIStyle invStackStyle;

	//Chat Variables
	private float chatOpacity = 0;
	private float chatStartTween = 0;
	private float chatEndTween = 0;
	private float chatTarget = 0;
	private float chatOrigin = 0;
	private string chatText = "";
	private bool chatPreviousReturnState = false;
	private GUIStyle chatFont;
	private GUIStyle chatInputFont;
	private string chatFull = "";

    #endregion

    #region Properties

    public bool ShowInventory
	{
		get { return showInventory; }
	}
	[System.NonSerialized]
	private bool showInventory = false;

	[System.NonSerialized]
	public bool HUDFocus = false;

	public bool ShowChat
	{
		get { return showChat; }
	}
	[System.NonSerialized]
	private bool showChat;

    #endregion

    void Awake()
    {
        Controller = GetComponent<PlayerController>();
    }

    // Use this for initialization
	void Start () {
		if (networkView.isMine)
		{
            Instance = this;
			MenuSetup();
			EquipmentSetup();

			HelperSetup(equipPanelRect.x,equipPanelRect.y-10,equipPanelRect.width);

			chatFont = new GUIStyle(InvSkin.label);
			chatFont.fontSize = 16;
			chatFont.alignment = TextAnchor.UpperLeft;

			chatInputFont = new GUIStyle(InvSkin.textArea);
			chatInputFont.fontSize = 15;
			chatInputFont.alignment = TextAnchor.UpperLeft;

		}
	}

	void Update()
	{
		mousePosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
	}

    void Resize()
    {
        #region Inventory Window
        {
            invScrollView = new Rect(8, 88, 200, 204);
            invScrollViewWidth = invScrollView.width - 12;

            invButtonSize = invScrollViewWidth / 6;
            invButtonMargin = invButtonSize * 0.1f;
            invButtonInner = invButtonSize - invButtonMargin * 2;
            invScrollViewHeight = invButtonSize * Inventory.Items.Count;

            if (invScrollViewHeight < invScrollView.height)
                invScrollViewHeight = invScrollView.height;
            invScrollInner = new Rect(0, 0, invScrollViewWidth, invScrollViewHeight);

            invStackStyle = Blurift.BluStyle.CustomStyle(GUI.skin.label, invButtonSize / 2);
            invStackStyle.alignment = TextAnchor.LowerLeft;

        }
        #endregion
        
        #region Equipment Bar
        {

        }
        #endregion

        #region Crafting Window
        {

        }
        #endregion

        #region Chatwindow
        {

        }
        #endregion

        DebugResize();
        PlayerListResize();
        DeadResize();
    }

	private void MouseCapture()
	{
		if(Input.GetMouseButtonDown(0))
			mouseDown = true;
		
		if(mouseDown && Input.GetMouseButtonUp(0))
		{
			dragIcon = null;
			dragItem = null;
			dragIndex = -1;
			dragSource = -1;
		}
	}

	private float chatScrollInnerH = 0;
	private float chatScrollHeight = 0;

	void OnGUI()
	{
        

		if(networkView.isMine)
		{
            GUI.skin = InvSkin;

            if (width != Screen.width && height != Screen.height)
            {
                width = Screen.width;
                height = Screen.height;
                Resize();
            }

            if (isDead)
            {
                deadDraw();
                return;
            }


			if(Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
			{
				if(showChat && chatText != "")
				{
					GameManager.NetMessage(chatText);
					chatText = "";
				}

				ToggleChat();
			}

			

			//Get some variables from the inventory.
			GUI.tooltip = "";
			int EquipmentIndex = Inventory.SelectedIndex;
			

			
			if(showInventory)
			{
				inventoryWindow = GUI.Window(windowInv,inventoryWindow, WindowFunction, "Inventory");
				craftWindow = GUI.Window(windowCrafting,craftWindow, WindowFunction, "Crafting");
				//GUI.Window(2,overlayWindow, WindowFunction, "", GUIStyle.none);
			}
			else if(showMenu)
			{
				GUI.Window(windowMenu, menuWindow, WindowFunction, "Menu");
			}
			

			HelperDraw();
			EquipmentDraw();
			EquippedInfoDraw();




			//Chatbox
			if(chatStartTween < Time.time && chatEndTween > Time.time)
			{
				float tweenPercent = (Time.time - chatStartTween) / (chatEndTween - chatStartTween);

				chatOpacity = chatOrigin + (chatTarget - chatOrigin) * tweenPercent;
			}
			else if(Time.time < chatStartTween)
				chatOpacity = chatOrigin;
			else if(Time.time > chatEndTween)
				chatOpacity = chatTarget;

			if(chatOpacity > 0)
			{
				Color chatColor = Color.white;
				chatColor.a = chatOpacity;
				GUI.color = chatColor;

				float chatWidth = width * ChatWidthPercent;
				float chatHeight = chatWidth * 0.8f;
				float chatY = height - chatHeight;
				float chatTextHeight = chatHeight * 0.2f;
				float chatTextY = height - chatTextHeight;
				chatScrollHeight = chatHeight * 0.8f;
				chatScrollInnerH = chatFont.CalcHeight(new GUIContent(chatFull), chatWidth-12);
				if(chatScrollInnerH < chatScrollHeight) chatScrollInnerH = chatScrollHeight;

				Rect chatRect = new Rect (0, chatY, chatWidth, chatHeight);
				Rect chatTextRect = new Rect (0 , chatTextY, chatWidth, chatTextHeight);
				Rect chatScrollRect = new Rect (0, chatY, chatWidth, chatScrollHeight);
				Rect chatScrollInner = new Rect (0, 0, chatWidth - 12, chatScrollInnerH);

				chatScrollPosition = GUI.BeginScrollView(chatScrollRect, chatScrollPosition, chatScrollInner, false, true);

				GUI.Label(new Rect(0,0, chatScrollInner.width,chatScrollInnerH),chatFull,chatFont);


				GUI.EndScrollView(true);
				if(showChat)
				{
					GUI.SetNextControlName("ChatTextBox");


					chatText = GUI.TextField (chatTextRect, chatText, chatInputFont);
					GUI.FocusControl("ChatTextBox");
					FixScroll();
				}
				else
				{
					GUI.TextField (chatTextRect, chatText, chatInputFont);
					GUI.FocusControl("");
				}

				GUI.color = Color.white;
			}
			
			if(showInventory)
				Overlay(false);
			craftingHover = -1;

            PlayerListDraw();
            DebugDraw();
		}
	}

	void WindowFunction(int windowID)
	{
		if(windowID == windowInv)
		{
			//If Item dropped into Inventory from equipment bar
			if(invScrollView.Contains(Event.current.mousePosition) && Input.GetMouseButtonUp(0) && dragSource==1)
			{
				//Debug.Log("Inventory Contains Mouse: " + dragIndex.ToString() + " : " + dragSource.ToString() + " : " + (dragIcon != null).ToString());
				if(dragIndex > -1 && dragItem != null)
				{
                    Inventory.MoveToInventory(dragIndex);
                    MouseCapture();
				}
			}
			
            //TODO - Fix bound of drop

			Rect dropRect = new Rect (inventoryWindow.width-53, 38, 45, 45);
			
			if(dropRect.Contains(Event.current.mousePosition) && Input.GetMouseButtonUp(0))
			{
				if(dragSource == 0 || dragSource == 1)
				{
					Inventory.DropItem(dragItem);
					MouseCapture();
				}
			}
			
			GUI.DrawTexture (dropRect, IconDropItem);
			
			
			
			inventoryScrollPosition = GUI.BeginScrollView (invScrollView,inventoryScrollPosition, invScrollInner, false, true);
			{
				//Inventory Slots
				int y = 0;
				while(y*6 < Inventory.Items.Count)
				{
					int hoverIndex = -1;
					for (int x = 0; x < 6; x++)
					{
						if(y*6+x < Inventory.Items.Count)
						{
							int itemIndex = y*6+x;
							Item item = Inventory.Items[itemIndex];
							
							Rect buttonRect = new Rect((invButtonSize*x)+invButtonMargin,(invButtonSize*y)+invButtonMargin,invButtonInner,invButtonInner);
							
							//if(GUI.Button(new Rect((37*x),(37*y),32,32), new GUIContent(item.Icon,item.Name)))
							//GUI.Button(buttonRect, new GUIContent(item.Icon,item.Name));
							
							
							if(buttonRect.Contains(Event.current.mousePosition))
							{
								hoverIndex = itemIndex;
								// Draw hover
								GUI.color = boxHover;
								
								if(Input.GetMouseButtonDown(0))
								{
									GUI.tooltip = item.Name;
									if(Input.GetMouseButtonDown(0))
									{
										mouseDown = true;
										
										dragSource = 0;
										dragIcon = item.Icon;
										dragIndex = itemIndex;
										dragItem = item;
									}
								}
							}
							else
							{
								GUI.color = boxStandard;
							}
							
							GUI.DrawTexture(buttonRect, Filler);
							
							GUI.color = Color.white;
							
							GUI.DrawTexture(buttonRect, item.Icon);
							if(item.Stackable)
							{
								GUI.Label(buttonRect, item.StackAmount.ToString(), invStackStyle);
							}
						}
					}
					itemHover = hoverIndex;
					y++;
				}
			}
			GUI.EndScrollView ();
		}
		
		if(windowID == windowCrafting)
		{
			CraftableItem[] craftItems = CraftingManager.AvailableCrafts(Inventory);

			GUIStyle craftFont = new GUIStyle (InvSkin.label);


			float craftItemHeight = craftScrollRect.height * 0.15f;
			StyleHelper.SetFontSize(craftFont,craftScrollRect.height*0.7f);

			if(craftIndex >= craftItems.Length)
				craftIndex = -1;
			
			int craftScrollHeight = (int)(craftItems.Length*craftItemHeight);
			if(craftScrollHeight < (int)craftScrollRect.height)
				craftScrollHeight = (int)craftScrollRect.height;
			
			craftScrollPosition = GUI.BeginScrollView(craftScrollRect, craftScrollPosition, new Rect(0,0,craftScrollRect.width-16,craftScrollHeight), false,true);
			
			for(int i = 0 ; i < craftItems.Length; i++)
			{
				Rect craftItemRect = new Rect(0,i*craftItemHeight,(int)craftScrollRect.width,craftItemHeight);
				
				if(craftItemRect.Contains(Event.current.mousePosition)&& Input.GetMouseButtonDown(0))
					craftIndex = i;
				
				if(i == craftIndex)
				{
					GUI.color = new Color(0.7f,0.4f,0.4f);
					GUI.DrawTexture(craftItemRect, Filler);
					GUI.color = Color.white;
				}
				
				GUI.Label(craftItemRect, craftItems[i].Item.Name);
				
			}
			
			GUI.EndScrollView(true);
			
			if(craftIndex > -1)
			{
				if(GUI.Button(new Rect(8,265,craftWindow.width-16,25), "Craft"))
				{
					bool craftIt = true;
					CraftableItem item = craftItems[craftIndex];
					for(int j = 0; j < item.ItemNeeded.Length; j++)
					{
						if(!Inventory.IsStackAvailable(item.ItemNeeded[j].name,item.ItemQNeeded[j]))
						{
							craftIt = false;
						}
					}
					
					if(craftIt)
					{
						for(int j = 0; j < item.ItemNeeded.Length; j++)
						{
							Inventory.RetrieveFromStack(item.ItemNeeded[j].name,item.ItemQNeeded[j]);
						}
						Inventory.AddToInventory(item.Item.name,item.Item.StackAmount);
						//networkView.RPC("CraftItem", RPCMode.Server, item.Item.name, Network.player);
					}
				}
			}


		}
		
		if(windowID == windowMenu)
		{
			MenuDraw();
		}

		Overlay (true);
		MouseCapture ();
		
		GUI.DragWindow (new Rect (0, 0, 226, 20));
	}

	private void Overlay(bool window)
	{
		//Show current dragged Item if any
		if(mouseDown && dragIcon != null)
		{
			GUI.DrawTexture(new Rect(Event.current.mousePosition.x,Event.current.mousePosition.y,dragIcon.width,dragIcon.height), dragIcon);
		}
		else if(itemHover > -1)
		{
			string desc = Inventory.Items[itemHover].ItemDesc();
			float x;
			float y;
			if(window)
			{
				x = Event.current.mousePosition.x;
				y = Event.current.mousePosition.y;;
			}
			else
			{
				x = mousePosition.x;
				y = mousePosition.y;
			}
			float width = Screen.width*0.2f;
			float height = GUI.skin.GetStyle("Label").CalcHeight(new GUIContent(desc), width);
			
			//if(x > Screen.width - (width+10))
			//	x -= (width+10);
			//if(y > Screen.height - (height+10))
			//	y -= (height+10);
			
			GUI.color = new Color(0.22f,0.22f,0.22f);
			GUI.DrawTexture(new Rect(x,y,width+10,height+10),Filler);
			GUI.color = Color.white;
			
			//Name
			GUI.Label(new Rect(x+5,y+5,width,height), desc);
		}
		else if(craftingHover > -1)
		{
			
		}
		
		
		GUI.Label(new Rect(Input.mousePosition.x, Screen.height-Input.mousePosition.y, 130,30), GUI.tooltip);
	}

	public void ToggleInventory()
	{
		showInventory = !showInventory;
		ToggleHud ();
	}

	public void ToggleChat()
	{
		showChat = !showChat;
		if(showChat)
		{
			TweenChat(chatOpacity, 1,Time.time,Time.time+0.5f);
		}
		else
			TweenChat(chatOpacity, 0,Time.time+1,Time.time+2.5f);
		ToggleHud();
	}

	public void ToggleMenu()
	{
		showMenu = !showMenu;
		ToggleHud ();
	}

	private void ToggleHud()
	{
		HUDFocus = showChat || showInventory || showMenu;
		Screen.showCursor = HUDFocus;
	}

	private void TweenChat(float origin, float target, float start, float end)
	{
		chatOrigin = origin;
		chatTarget = target;
		chatStartTween = start;
		chatEndTween = end;
	}

	public void ChatUpdate()
	{
		chatFull = "";
		for (int i = 0; i < GameManager.messages.Count; i++)
		{	
			chatFull += GameManager.messages[i];
			if(i != GameManager.messages.Count -1)
				chatFull += "\n";
		}

		if(!showChat)
		{
			TweenChat(1, 0,Time.time+2,Time.time + 3.5f);
		}
		FixScroll ();
	}

	public void FixScroll()
	{
		chatScrollPosition = new Vector2(0,chatScrollInnerH - chatScrollHeight);
	}

	/// -- On Screen Helper Functions --
	/// 
	/// Will allow different systems to display a
	/// helpful message or reminder on teh screen
	/// for the user.

	private Rect helperInputRect;
	private string helperInputText = "";
	private string helperInputCommand = "";
	private float helperInputTime = 0;

	private Rect helperMessageRect;
	private string helperMessageText = "";
	private float helperMessageTime = 0;
	private float helperMessageFade = 0;
	private float helperMessageOpacity = 0;

	/// <summary>
	/// Set up the on screen helper displays to assist with interacting with the world
	/// co-ordinates are set up to use the bottom left as the anchor to align with.
	/// </summary>
	/// <param name="x">The x coordinate. (Left)</param>
	/// <param name="y">The y coordinate. (Bottom).</param>
	/// <param name="width">Width.</param>
	private void HelperSetup(float x, float y, float width)
	{
		helperInputRect = new Rect (x, y - 30, width, 30);
	}

	private void HelperDraw()
	{
		//Draw the input helper
		string helperInputLabel = "";
		if(helperInputText != "")
			helperInputLabel += helperInputText;
		if(helperInputCommand != "" && Time.time < helperInputTime)
		{
			if(helperInputText != "")
				helperInputLabel += " ";

			helperInputLabel += "(" + helperInputCommand + ")";
			GUI.Label (helperInputRect, helperInputLabel, Menu.LabelCenter);
		}


		//Draw the message helper
		if(Time.time < helperMessageFade)
		{
			float messageHeight = Menu.LabelCenter.CalcHeight (new GUIContent (helperMessageText), helperInputRect.width);
			helperMessageRect = new Rect (helperInputRect.x, helperInputRect.y - 40 - messageHeight, helperInputRect.width, messageHeight);

			if(Time.time < helperMessageTime)
			{
				helperMessageOpacity = 1;
			}
			else
			{
				helperMessageOpacity = (helperMessageFade - Time.time) / (helperMessageFade - helperMessageTime);
			}

			Color color = Color.white;
			color.a = helperMessageOpacity;
			GUI.color = color;
			GUI.Label(helperMessageRect, helperMessageText, Menu.LabelCenter);
			GUI.color = Color.white;
		}
	}

	/// <summary>
	/// Show a input option like to pick up an item or open a door.
	/// </summary>
	/// <param name="text">Text.</param>
	/// <param name="input">Input.</param>
	/// <param name="time">Time.</param>
	public void HelperInput(string text, string input, float time)
	{
		helperInputText = text;
		helperInputCommand = input;
		helperInputTime = Time.time + time;
	}

	/// <summary>
	/// Shows a helpful message like a tutorial on the screen
	/// </summary>
	/// <param name="text">Text.</param>
	/// <param name="time">Time.</param>
	/// <param name="fade">Fade.</param>
	public void HelperMessage(string text, float time, float fade)
	{
		helperMessageText = text;
		helperMessageTime = Time.time + time;
		helperMessageFade = Time.time + time + fade;
	}

	/// -- On Screen equipment Functions --
	/// 
	/// Will show each equipped item and what is currently
	/// equipped and quantites

	public float EquipmentWidthPercent = 0.25f;

	private float equipPanelWidth;
	private float equipButtonSize;
	private float equipMargin;
	private float equipButtonInner;
	private Rect equipPanelRect;

	private void EquipmentSetup()
	{
		equipPanelWidth = Screen.width * EquipmentWidthPercent;
		equipButtonSize = equipPanelWidth / 6;
		equipMargin = equipButtonSize*0.1f;
		equipButtonInner = equipButtonSize - equipMargin * 2;
		equipPanelRect = new Rect( Screen.width/2 - equipPanelWidth/2,  Screen.height-equipButtonSize, equipPanelWidth, equipButtonSize);

	}

	private void EquippedInfoDraw()
	{
		int equipmentIndex = Inventory.SelectedIndex;

		//Equipped weapon Display
		if(equipmentIndex > -1)
		{
			if(Inventory.Equipment[equipmentIndex] != null)
			{
				//Create text font
				GUIStyle label = Blurift.BluStyle.CustomStyle(GUI.skin.label,Screen.height*0.015f);
				string text = Inventory.Equipment[equipmentIndex].HUDDisplay();

				//Rect Bounds
				float x = Screen.width/2+equipPanelWidth/2+equipMargin;
				float width = Screen.width*0.25f;
				
				float height = label.CalcHeight(new GUIContent(text), width);
				float y = Screen.height - height - Screen.height*0.01f;;



				GUI.Label(new Rect(x,y,width,height), text, label);

				//string[] HudDisplay = Regex.Split(Inventory.Equipment[equipmentIndex].HUDDisplay(), "\n");
				//for(int i = 0; i < HudDisplay.Length; i++)
				//{
				//	GUI.Label(new Rect(Screen.width/2+equipPanelWidth/2+equipMargin, Screen.height - 14 - (HudDisplay.Length*22) + (i*22),200,20), HudDisplay[i]);
				//}
			}
		}
	}

	private void EquipmentDraw()
	{
		int equipmentIndex = Inventory.SelectedIndex;


		
		if(EquipmentRack != null)
		{
			GUI.DrawTexture(equipPanelRect, EquipmentRack);
		}
		for (int i = 0; i < Inventory.Equipment.Length; i++)
		{
			float buttonY = equipPanelRect.y + equipMargin;
			if(i == equipmentIndex)
				buttonY -= 5;
			float buttonX = equipPanelRect.x + (equipButtonSize*i) + equipMargin;
			
			Rect equipBar = new Rect(buttonX,buttonY,equipButtonInner,equipButtonInner);


			if(Inventory.Equipment[i] != null)
			{
				GUI.DrawTexture( equipBar, Inventory.Equipment[i].Icon);
				
				if(Inventory.Equipment[i].Stackable)
				{
					GUI.Label(equipBar, Inventory.Equipment[i].StackAmount.ToString(), invStackStyle);
				}
			}
			GUI.Label(equipBar, (i+1).ToString(), Menu.UpperLeft);

			//If the equipment slot has been clicked on
			if(equipBar.Contains(Event.current.mousePosition) && Input.GetMouseButtonDown(0) && Inventory.Equipment[i] != null)
			{
				if(showInventory)
				{
					dragSource = 1;
					dragIcon = Inventory.Equipment[i].Icon;
					dragIndex = i;
					dragItem = Inventory.Equipment[i];
				}
			}
			
            //Check to see if item is being dragged from inventory to equipment bar.
			if(equipBar.Contains(Event.current.mousePosition) && Input.GetMouseButtonUp(0) && Inventory.Equipment[i] == null && dragItem != null && dragSource == 0)
			{
                Inventory.MoveToEquipment(dragIndex, i);
				
				MouseCapture();
			}
		}
	}

	/// -- Menu Functions --
	/// 
	/// Will display the menu for the player to access

	private Rect menuWindow;
	private float menuWidth = 1;
	private float menuButtonWidth = 1;
	private float menuButtonHeight = 1;

	private void MenuSetup()
	{
		float w = Screen.width*0.2f;
		float h = w*0.9f;
		float x = Screen.width/2-w/2;
		float y = Screen.height/2-h/2;
		
		menuWindow = new Rect(x,y,w,h);
		menuButtonHeight = (menuWindow.height-33)/4;
		menuButtonWidth = menuWindow.width-10;

	}

	private void MenuDraw()
	{
		float divider = (menuButtonHeight * 3) / 2;
		if(GUI.Button(new Rect(5,28,menuButtonWidth,menuButtonHeight), "Resume"))
		{
			ToggleMenu();
		}
		
		if(GUI.Button(new Rect(5,28+divider,menuButtonWidth,menuButtonHeight), "Options"))
		{
			//NetworkManager.Instance.
		}

		if(GUI.Button(new Rect(5,28+divider*2,menuButtonWidth,menuButtonHeight), "Quit"))
		{
			NetworkManager.Disconnect();
		}
	}

    #region Debug

    /// --- Debug Screen ---
    /// 
    /// Display debug stuff

    private bool debugOn = false;

    private float debugLineMargin = 0;
    private float debugLineSize = 0;
    private Rect debugRect;
    private GUIStyle debugLabelStyle;

    public void DebugToggle()
    {
        debugOn = !debugOn;
    }

    private void DebugResize()
    {
        float w = Screen.width * 0.2f;
        float lh = w * 0.04f;
        debugLineSize = lh * 1.5f;
        debugLineMargin = lh * 0.1f;

        debugRect = new Rect(0, 0, w, debugLineSize * 8);
        debugLabelStyle = Blurift.BluStyle.CustomStyle(GUI.skin.label, lh);

    }

    private void DebugDraw()
    {
        if(debugOn)
        {
            GUI.color = Color.black;
            GUI.DrawTexture(debugRect, Filler);
            GUI.color = Color.white;

            int hLock = 0;

            //Draw Debug Text;
            GUI.Label(new Rect(10, (debugLineSize * hLock) + debugLineMargin, debugRect.width, debugLineSize), "Rotation: " + transform.rotation.eulerAngles.z.ToString(), debugLabelStyle); hLock++;
            GUI.Label(new Rect(10, (debugLineSize * hLock) + debugLineMargin, debugRect.width, debugLineSize), "Position: " + transform.position.ToString(), debugLabelStyle); hLock++;
            GUI.Label(new Rect(10, (debugLineSize * hLock) + debugLineMargin, debugRect.width, debugLineSize), "Health: " + Controller.Health.Health + " / " + Controller.Health.HealthMax, debugLabelStyle); hLock++;
            GUI.Label(new Rect(10, (debugLineSize * hLock) + debugLineMargin, debugRect.width, debugLineSize), "Hunger: " + Controller.Health.Hunger + " / " + Controller.Health.HungerMax, debugLabelStyle); hLock++;
            GUI.Label(new Rect(10, (debugLineSize * hLock) + debugLineMargin, debugRect.width, debugLineSize), "Stamina: " + (int)Controller.Health.Stamina + " / " + (int)Controller.Health.StaminaMax, debugLabelStyle); hLock++;
            GUI.Label(new Rect(10, (debugLineSize * hLock) + debugLineMargin, debugRect.width, debugLineSize), "Velocity: " + rigidbody2D.velocity.ToString(), debugLabelStyle); hLock++;
            GUI.Label(new Rect(10, (debugLineSize * hLock) + debugLineMargin, debugRect.width, debugLineSize), "Ping: " + NetworkManager.Instance.PlayerStates[Network.player].Ping, debugLabelStyle); hLock++;
            GUI.Label(new Rect(10, (debugLineSize * hLock) + debugLineMargin, debugRect.width, debugLineSize), "FPS: " + (1.0f / Time.deltaTime), debugLabelStyle);
        }
        else
        {
            GUI.Label(new Rect(10, debugLineMargin, debugRect.width, debugLineSize), "Press (L) to show debug", debugLabelStyle);
        }
    }
    #endregion

    #region Player List

    private bool playerListOn = false;

    private int playerListCount = 0;
    private int playerListIndex = 0;
    private float playerListLineHeight;
    private float playerListLeftMargin;
    private float playerListLineMargin;
    private float playerListLineMax = 0;
    private float playerListPingX = 0;
    private float playerListPingWidth = 0;
    private float playerListLineWidth = 0;

    private Rect playerListRect;
    private Rect playerListInner;
    private GUIStyle playerListLabel;
    private GUIStyle playerListTitle;

    private Color playerListColor = new Color(0.2f, 0.2f, 0.2f);

    private void PlayerListResize()
    {
        float width = Screen.width * 0.4f;
        float height = Screen.height * 0.5f;



        playerListRect = new Rect(width*0.1f, Screen.height * 0.125f, width, height);
        playerListInner = new Rect(playerListRect.x + playerListRect.width * 0.1f, playerListRect.y + playerListRect.height * 0.3f, playerListRect.width * 0.8f, playerListRect.height * 0.6f);

        playerListLineMax = playerListInner.height * 0.125f;

        playerListPingX = playerListInner.width * 0.71f + playerListInner.x;
        playerListPingWidth = playerListInner.width * 0.27f;
        playerListLineWidth = playerListInner.width * 0.67f;
        playerListLeftMargin = playerListInner.x + playerListRect.width*0.05f;
    }

    private void PlayerListDraw()
    {

        if (!playerListOn)
            return;
        GUI.Box(playerListRect, GUIContent.none);
        GUI.color = playerListColor;
        GUI.DrawTexture(playerListInner, Filler);
        GUI.color = Color.white;

        if (playerListCount != NetworkManager.PlayerCount())
        {
            playerListCount = NetworkManager.PlayerCount();

            playerListLineHeight = playerListInner.height / NetworkManager.PlayerCount();
            if (playerListLineHeight > playerListLineMax)
                playerListLineHeight = playerListLineMax;

            playerListLabel = Blurift.BluStyle.CustomStyle(GUI.skin.label, playerListLineHeight * 0.8f);
            playerListLineMargin = playerListLineHeight * 0.1f;


        }

        playerListIndex = 0;
        //PlayerListDrawLine(Network.player, "ME");
        //playerListIndex++;
        foreach(KeyValuePair<NetworkPlayer, NetworkManager.NetworkPlayerState> p in NetworkManager.Instance.PlayerStates)
        {
            PlayerListDrawLine(p.Value.Ping, p.Value.Name);
            playerListIndex++;
        }

    }

    private void PlayerListDrawLine(int ping, string name)
    {
        GUI.Label(new Rect(playerListLeftMargin, playerListInner.y + (playerListIndex * playerListLineHeight) + playerListLineMargin, playerListLineWidth, playerListLineHeight), name, playerListLabel);
        Rect pingRect = new Rect(playerListPingX, playerListInner.y + (playerListIndex * playerListLineHeight) + playerListLineMargin, playerListPingWidth, playerListLineHeight);
        PlayerListDrawPing(pingRect, ping);
    }

    private void PlayerListDrawPing(Rect rect, int ping)
    {
        Color original = GUI.color;
        if (ping > 200)
            GUI.color = new Color(1,0.3f, 0.3f);
        else if (ping > 100)
            GUI.color = new Color(1,0.5f, 0.5f);
        else
            GUI.color = new Color(0,1, 0.3f);
        GUI.Label(rect, ping.ToString(), playerListLabel);

        GUI.color = original;
    }

    public void PlayerListToggle()
    {
        playerListOn = !playerListOn;
    }

    #endregion

    #region Player Dead

    private bool isDead;

    public void SetDead(bool d)
    {
        isDead = d;

    }

    private Rect deadRect;

    private GUIStyle deadStyle;

    private Color deadColor;

    private void DeadResize()
    {
        deadRect = new Rect(width * 0.25f, height * 0.42f, width * 0.5f, height * 0.16f);
        deadStyle = Blurift.BluStyle.CustomStyle(GUI.skin.label, deadRect.height * 0.65f);
        deadStyle.alignment = TextAnchor.MiddleCenter;

        deadColor = Color.red;
    }

    private void deadDraw()
    {
        GUI.color = deadColor;
        GUI.Label(deadRect, "You are dead.", deadStyle);
        GUI.color = Color.white;
    }

    #endregion
}
