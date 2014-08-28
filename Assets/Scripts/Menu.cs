using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Blurift.BluRMF;

public class Menu : MonoBehaviour {

	//Static info
	public const string TypeName = "BluriftInstinctsPreview04";
	public const string VERSION = "0.4.4";

	public static Profile MainProfile;
	public static Menu Instance;

	//Gameinfo
	private string serverSysFile = "";

	//ScreenInfo
	private MenuProfile menuProfile;
	private MenuJoinGame menuJoinGame;
	private MenuHost menuHost;

	//NetworkTesting
	private bool doneTesting = false;
	private bool probingPublicIP = false;
	private ConnectionTesterStatus connectionTestResult = ConnectionTesterStatus.Undetermined;
	private string testStatus = "Press test button";
	private string testMessage = "Press test button";
	private string shouldEnableNat = "";
	public static bool UseNat = false;
	private int serverPort = 7868;
	private float testTimer = 0;
	private bool testing = false;

	//ScreenInfo
	public static bool log = false;
	public static string ScreenMessage = "";
	public static ScreenType CurrentScreen = ScreenType.MainMenu;
	public static OptionsScreenType OptionsScreen = OptionsScreenType.Graphics;

	//Styleinfo
	public GUISkin MenuSkin;
	public static GUIStyle LabelCenter;
	public static GUIStyle LabelRight;
	public static GUIStyle LabelLeft;
	public static GUIStyle LowerLeft;
	public static GUIStyle LowerCenter;
	public static GUIStyle LowerRight;
	public static GUIStyle UpperLeft;
	public static GUIStyle UpperCenter;
	public static GUIStyle UpperRight;

	public Texture2D BackgroundImage;
	public Texture2D filler;
	public Texture2D logo;
	public Texture2D build;

	private Color scrollItemColor = new Color(0.4f,0.4f,0.4f);
	private Color scrollItemColorAlt = new Color(0.2f,0.2f,0.2f);
	private Color scrollItemFocus = new Color(0.7f,0.4f,0.4f);
	private Color scrollItemFocusAlt = new Color(0.6f,0.2f,0.2f);

	//Player prefs
	public static bool GameTips;

	//GUI info
	private Rect creditsRect;
	private Rect creditsScrollView;
	private Vector2 creditsScrollPos = Vector2.zero;

	private string creditsText = "<b>Lead Designer/Programmer</b>\nKeirron Stach\n\n" +
		"<b>Sound Assets</b>\nFreesound.org Creative Commons\n\n" +
		"<b>Concept Art</b>\nLisa Hignett\nZakodia Art Studio\n\n" +
		"<b>Music</b>\nThis could be YOU\n\n" +
		"<b>A* Pathfinding</b>\nhttp://www.arongranberg.com/\n\n" +
		"<b>QA Testing</b>\nSam Dalby\nJoey Testo\n\n" +
		"<b>Special Thanks</b>\nKarleigh Brown\nLuke Cross\nJessica Mueller";

	float buttonWidth = 1;
	float buttonHeight = 1;
	float buttonSizeW = 1;
	float buttonSizeH = 1;
	float buttonMargin = 0.1f;
	int buttonI = 0;

	// Use this for initialization
	void Start () {
		Instance = this;
		serverSysFile = GameManager.SavePath() + "server.sys";

		CurrentScreen = ScreenType.MainMenu;

		float boxWidth = Screen.width * 0.25f;
		float boxHeight = Screen.height * 0.5f;

		creditsRect = new Rect (0, Screen.height - boxHeight, boxWidth, boxHeight);
		creditsScrollView = new Rect (creditsRect.x+8, creditsRect.y+8, boxWidth - 16, boxHeight - 16);

		if (PlayerPrefs.GetInt ("GameTips", 1) == 1)
			GameTips = true;
		else
			GameTips = false;

		menuProfile = new MenuProfile ();

		if (!menuProfile.ProfileAvailable ())
			SwitchScreen (ScreenType.CreateProfile);

		if(PlayerPrefs.HasKey("Profile"))
		{
			MainProfile = Profile.LoadProfile(MenuProfile.GetProfilePath(PlayerPrefs.GetString("Profile")));
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public static void SetFontSize(GUISkin skin)
	{
		float h = Screen.height;
		skin.textField.fontSize = (int)(0.07f * h);
		skin.button.fontSize = (int)(0.02f * h);
		skin.label.fontSize = (int)(0.02f * h);
	}

	public static GUIStyle GetCustomStyleFont(GUIStyle original, float size)
	{
		GUIStyle s = new GUIStyle (original);
		s.fontSize = (int)(size * Screen.height);
		return s;
	}

	public static void SetTextAlignments(GUISkin skin)
	{
		if(LabelLeft == null)
		{
			LabelLeft = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LabelLeft.alignment = TextAnchor.MiddleLeft;
		}
		if(LabelCenter == null)
		{
			LabelCenter = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LabelCenter.alignment = TextAnchor.MiddleCenter;
		}
		if(LabelRight == null)
		{
			LabelRight = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LabelRight.alignment = TextAnchor.MiddleRight;
		}
		if(UpperLeft == null)
		{
			UpperLeft = new GUIStyle (GUI.skin.GetStyle ("Label"));
			UpperLeft.alignment = TextAnchor.UpperLeft;
		}
		if(UpperCenter == null)
		{
			UpperCenter = new GUIStyle (GUI.skin.GetStyle ("Label"));
			UpperCenter.alignment = TextAnchor.UpperCenter;
		}
		if(UpperRight == null)
		{
			UpperRight = new GUIStyle (GUI.skin.GetStyle ("Label"));
			UpperRight.alignment = TextAnchor.UpperRight;
		}
		if(LowerLeft == null)
		{
			LowerLeft = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LowerLeft.alignment = TextAnchor.LowerLeft;
		}
		if(LowerCenter == null)
		{
			LowerCenter = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LowerCenter.alignment = TextAnchor.LowerCenter;
		}
		if(LowerRight == null)
		{
			LowerRight = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LowerRight.alignment = TextAnchor.LowerRight;
		}
	}

	void OnGUI()
	{

		GUI.skin = MenuSkin;
		SetFontSize (GUI.skin);

		SetTextAlignments (GUI.skin);

		Screen.showCursor = true;

		if(CurrentScreen == ScreenType.CreateProfile)
		{
			if(menuProfile != null)
				menuProfile.DrawCreateProfileScreen();
			else
				CurrentScreen = ScreenType.MainMenu;
		}

		if(CurrentScreen == ScreenType.SelectProfile)
		{
			if(menuProfile != null)
				menuProfile.DrawSelectProfileScreen();
			else
				CurrentScreen = ScreenType.MainMenu;
		}

		if(CurrentScreen == ScreenType.Connecting)
		{
			GUI.Label(new Rect(Screen.width/2-150,Screen.height/2,300,20),"Connecting...", LabelCenter);
			if(GUI.Button(new Rect(Screen.width/2-25,Screen.height/2+50,50,20),"Stop"))
			{
				CurrentScreen = ScreenType.MainMenu;
			}
		}

		if(CurrentScreen == ScreenType.HostGame)
		{
			if(menuHost != null)
				menuHost.Draw();
			else
				CurrentScreen = ScreenType.MainMenu;
		}

		
		if(CurrentScreen == ScreenType.Error)
		{			
			GUI.Label(new Rect(Screen.width/2-150,Screen.height/2,300,20),ScreenMessage, LabelCenter);
			if(GUI.Button(new Rect(Screen.width/2-15,Screen.height/2+50,30,20),"Ok"))
			{
				CurrentScreen = ScreenType.MainMenu;
			}
		}

		if(CurrentScreen == ScreenType.FindGame)
		{
			if(menuJoinGame != null)
				menuJoinGame.Draw();
			else
				SwitchScreen(ScreenType.MainMenu);
		}

		if(CurrentScreen == ScreenType.MainMenu)
		{
			buttonHeight = (Screen.height/2)/7;
			buttonWidth = buttonHeight * 3.5f;;
			float buttonDivider = buttonHeight * 0.2f;


			
			buttonI = 0;

			float estY = (buttonHeight*4) + (buttonDivider*3);

			if(File.Exists(serverSysFile))
				estY += buttonHeight + buttonDivider;

			float x = Screen.width/2 - buttonWidth/2;
			float y = Screen.height/2-estY/2 + buttonHeight;

			float logoRatio = (float)logo.height / (float)logo.width;
			float logoW = buttonWidth*3.5f;
			float logoH = logoW*logoRatio;
			float logoX = Screen.width/2 - logoW/2;
			float logoY = logoH/2;

			float welcomeW = Screen.width/3;
			float welcomeH = 50;
			float welcomeX = Screen.width/2 - welcomeW/2;
			float welcomeY = logoY + logoH*1.1f;

			GUI.DrawTexture(new Rect(logoX,logoY,logoW,logoH), logo);
			GUI.Label(new Rect(welcomeX,welcomeY,welcomeW,welcomeH), "Version: " + VERSION + "\nHello " + MainProfile.Name, LabelCenter);


			//GUI.Label(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)), buttonWidth,buttonHeight/2), Username, LabelCenter);
			buttonI+=1;

			if(GUI.Button(new Rect(x,y + (buttonI*(buttonHeight+buttonDivider)),buttonWidth,buttonHeight), "Host Game"))
			{
				SwitchScreen(ScreenType.HostGame);
				//NetworkManager.Server = true;
				//Network.SetLevelPrefix(LevelLoader.LEVEL_GAME);
				//Application.LoadLevel(LevelLoader.LEVEL_GAME);
				//NetworkManager.StartServer();
			}
			buttonI+=1;

			if(GUI.Button(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)),buttonWidth,buttonHeight), "Join Game"))
			{
				SwitchScreen(ScreenType.FindGame);
			}
			buttonI+=1;

			if(GUI.Button(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)),buttonWidth,buttonHeight), "Create Profile"))
			{
				this.SwitchScreen(ScreenType.CreateProfile);
			}
			buttonI+=1;

			if(GUI.Button(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)),buttonWidth,buttonHeight), "Select Profile"))
			{
				this.SwitchScreen(ScreenType.SelectProfile);
			}
			buttonI+=1;


			
			if(GUI.Button(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)),buttonWidth,buttonHeight), "Options"))
			{
				CurrentScreen = ScreenType.Options;
			}
			buttonI+=1;

			if(GUI.Button(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)),buttonWidth,buttonHeight), "Quit"))
			{
				Application.Quit();
			}
			

			//Patch Notes



			//Credits

			float creditsScrollWidth = creditsScrollView.width-12;
			float creditsScrollH = LabelCenter.CalcHeight(new GUIContent(creditsText), creditsScrollWidth);

			if(creditsScrollH < creditsScrollView.height)
				creditsScrollH = creditsScrollView.height;

			Rect creditsScollInner = new Rect(0,0,creditsScrollWidth,creditsScrollH);

			creditsScrollPos = GUI.BeginScrollView(creditsScrollView, creditsScrollPos,creditsScollInner, false,true);

			GUI.Label(creditsScollInner, creditsText, LabelCenter);

			GUI.EndScrollView(true);
		}

		if(CurrentScreen == ScreenType.Options)
		{
			buttonWidth = Screen.width*0.15f;
			buttonHeight = buttonWidth*0.3f;
			float buttonDivider = buttonHeight *0.2f;
			float subScreen = buttonWidth + buttonDivider*2;
			float subScreenWidth = Screen.width - subScreen;

			buttonI = 0;

			/* NETWORK TESTING
			if(GUI.Button(new Rect(buttonDivider,buttonI*(buttonHeight+buttonDivider)+buttonDivider, buttonWidth,buttonHeight), "Test Network"))
			{
				Network.InitializeServer(0,serverPort,UseNat);
				testing = true;
				doneTesting = false;
				testStatus = "Testing...";
			}
			if(!doneTesting && testing)
				TestConnection();
			if(Network.isServer && doneTesting)
				Network.Disconnect();

			float messageHeight = 0;
			string message = "Current Status: " + testStatus +
				"\n\nTest Result: " + testMessage +
				"\n\n" + shouldEnableNat;

			messageHeight = GUI.skin.GetStyle("Label").CalcHeight(new GUIContent( message),Screen.width/2);

			GUI.Label(new Rect(buttonWidth + buttonDivider*2, buttonDivider, Screen.width/2,messageHeight), message);
			buttonI++;
			*/



			switch (OptionsScreen) {
			case OptionsScreenType.Graphics:
				GUI.Label(new Rect(subScreen,buttonDivider, subScreenWidth,Screen.height * 0.2f), "Graphics", GUI.skin.customStyles[2]);
				break;
			case OptionsScreenType.Sound:
				GUI.Label(new Rect(subScreen,buttonDivider, subScreenWidth,Screen.height * 0.2f), "Sound", GUI.skin.customStyles[2]);
				break;
			case OptionsScreenType.Controls:
				GUI.Label(new Rect(subScreen,buttonDivider, subScreenWidth,Screen.height * 0.2f), "Controls", GUI.skin.customStyles[2]);
				break;
			}

			if(GUI.Button(new Rect(buttonDivider,buttonI*(buttonHeight+buttonDivider)+buttonDivider, buttonWidth,buttonHeight), "Graphics"))
			{
				OptionsScreen = OptionsScreenType.Graphics;
			}

			buttonI++;

			if(GUI.Button(new Rect(buttonDivider,buttonI*(buttonHeight+buttonDivider)+buttonDivider, buttonWidth,buttonHeight), "Sound"))
			{
				OptionsScreen = OptionsScreenType.Sound;
			}
			
			buttonI++;

			if(GUI.Button(new Rect(buttonDivider,buttonI*(buttonHeight+buttonDivider)+buttonDivider, buttonWidth,buttonHeight), "Controls"))
			{
				OptionsScreen = OptionsScreenType.Controls;
			}
			
			buttonI++;

			if(GUI.Button(new Rect(buttonDivider,buttonI*(buttonHeight+buttonDivider)+buttonDivider, buttonWidth,buttonHeight), "Main Menu"))
			{
				CurrentScreen = ScreenType.MainMenu;
				OptionsScreen = OptionsScreenType.Graphics;
			}
			buttonI++;
		}
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
		{
			menuJoinGame.hostList = MasterServer.PollHostList ();
			menuJoinGame.hostListStatus = "Found " + menuJoinGame.hostList.Length + " servers.";
		}
	}

	void OnConnectedToServer()
	{
		NetworkManager.Server = false;

		LevelLoader.Instance.LoadLevel (LevelLoader.LEVEL_GAME);

	}

	void OnFailedToConnect(NetworkConnectionError error) 
	{
		CurrentScreen = ScreenType.Error;
		ScreenMessage = "Could not connect to server. " + error.ToString ();
	}

	private void TestConnection()
	{
		// Start/Poll the connection test, report the results in a label and 
		// react to the results accordingly
		connectionTestResult = Network.TestConnection();
		switch (connectionTestResult) {
		case ConnectionTesterStatus.Error: 
			testMessage = "Problem determining NAT capabilities";
			doneTesting = true;
			break;
			
		case ConnectionTesterStatus.Undetermined: 
			testMessage = "Undetermined NAT capabilities";
			doneTesting = false;
			break;
			
		case ConnectionTesterStatus.PublicIPIsConnectable:
			testMessage = "Directly connectable public IP address.";
			UseNat = false;
			doneTesting = true;
			break;
			
			// This case is a bit special as we now need to check if we can 
			// circumvent the blocking by using NAT punchthrough
		case ConnectionTesterStatus.PublicIPPortBlocked:
			testMessage = "Non-connectable public IP address (port " +
				serverPort +" blocked), running a server is impossible.";
			UseNat = false;
			// If no NAT punchthrough test has been performed on this public 
			// IP, force a test
			if (!probingPublicIP) {
				connectionTestResult = Network.TestConnectionNAT();
				probingPublicIP = true;
				testStatus = "Testing if blocked public IP can be circumvented";
				testTimer = Time.time + 10;
			}
			// NAT punchthrough test was performed but we still get blocked
			else if (Time.time > testTimer) {
				probingPublicIP = false; 		// reset
				UseNat = true;
				doneTesting = true;
			}
			break;
		case ConnectionTesterStatus.PublicIPNoServerStarted:
			testMessage = "Public IP address but server not initialized, "+
				"it must be started to check server accessibility. Restart "+
					"connection test when ready.";
			break;
			
		case ConnectionTesterStatus.LimitedNATPunchthroughPortRestricted:
			testMessage = "Limited NAT punchthrough capabilities. Cannot "+
				"connect to all types of NAT servers. (Restricted) Running a server "+
					"is ill advised as not everyone can connect.";
			UseNat = true;
			doneTesting = true;
			break;
			
		case ConnectionTesterStatus.LimitedNATPunchthroughSymmetric:
			testMessage = "Limited NAT punchthrough capabilities. Cannot "+
				"connect to all types of NAT servers. (Symmetric) Running a server "+
					"is ill advised as not everyone can connect.";
			UseNat = true;
			doneTesting = true;
			break;
			
		case ConnectionTesterStatus.NATpunchthroughAddressRestrictedCone:
		case ConnectionTesterStatus.NATpunchthroughFullCone:
			testMessage = "NAT punchthrough capable. Can connect to all "+
				"servers and receive connections from all clients. Enabling "+
					"NAT punchthrough functionality.";
			UseNat = true;
			doneTesting = true;
			break;
			
		default: 
			testMessage = "Error in test routine, got " + connectionTestResult;
			break;
		}
		if (doneTesting) {
			if (UseNat)
				shouldEnableNat = "When starting a server the NAT "+
					"punchthrough feature should be enabled (useNat parameter)";
			else
				shouldEnableNat = "NAT punchthrough not needed";
			testStatus = "Done testing";
		}
	}
	
	public static string WebPost(string _URL, string _postString)
	{
		const string REQUEST_METHOD_POST = "POST";
		const string CONTENT_TYPE = "application/x-www-form-urlencoded";
		
		Stream dataStream = null;
		StreamReader reader = null;
		WebResponse response = null;
		string responseString = null;
		
		WebRequest request = WebRequest.Create(_URL);
		
		request.Method = REQUEST_METHOD_POST;
		
		string postData = _postString;
		byte[] byteArray = Encoding.UTF8.GetBytes(postData);
		
		request.ContentType = CONTENT_TYPE;
		request.ContentLength = byteArray.Length;
		
		dataStream = request.GetRequestStream();
		dataStream.Write(byteArray, 0, byteArray.Length);
		dataStream.Close();
		
		response = request.GetResponse();
		
		dataStream = response.GetResponseStream();
		
		reader = new StreamReader(dataStream);
		
		responseString = reader.ReadToEnd();
		
		if (reader != null) reader.Close();
		if (dataStream != null) dataStream.Close();
		if (response != null) response.Close();
		
		return responseString;
	}
	
	public static string HashString(string _value)
	{
		System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] data = System.Text.Encoding.ASCII.GetBytes(_value);
		data = x.ComputeHash(data);
		string ret = "";
		for (int i = 0; i < data.Length; i++) ret += data[i].ToString("x2").ToLower();
		return ret;
	}

	public enum OptionsScreenType
	{
		Sound,
		Graphics,
		Controls,
		Other,
	}

	public void SwitchScreen(ScreenType s)
	{
		switch(s)
		{
		case ScreenType.CreateProfile:
			menuProfile = new MenuProfile();
			break;
		case ScreenType.SelectProfile:
			menuProfile = new MenuProfile();
			break;
		case ScreenType.FindGame:
			menuJoinGame = new MenuJoinGame();
			break;
		case ScreenType.HostGame:
			menuHost = new MenuHost();
			break;
		}

		CurrentScreen = s;
	}
}

public static class StyleHelper
{
	public static GUISkin SetAllFontSizes(GUISkin skin, float size)
	{
		return skin;
	}

	public static GUIStyle SetFontSize(GUIStyle style, float size)
	{
		style.fontSize = (int)size;
		return style;
	}
}

public enum ScreenType
{
	MainMenu,
	FindGame,
	HostGame,
	Options,
	Error,
	Connecting,
	CreateProfile,
	SelectProfile,
}

public enum NetworkSearchType
{
	Internet,
	LAN,
	Direct,
}
