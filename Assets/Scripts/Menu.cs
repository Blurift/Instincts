using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public class Menu : MonoBehaviour {

	//Static info
	private const string typeName = "BluriftInstinctsPreview";
	public static string VERSION = "0.2.0.2";

	//Gameinfo
	private HostData[] hostList = new HostData[0];
	private Dictionary<string, Ping> hostPings = new Dictionary<string, Ping> ();
	private string hostListStatus = "Searching...";
	private string LanIP = "";
	private string LanPort = "";
	private string serverSysFile = "";

	//PlayerInfo
	private static string Username = "Player";
	private string password = "";
	private static bool Authenticated = false;
	private string authError = "";

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

	//Styleinfo
	public GUISkin MenuSkin;
	public static GUIStyle LabelCenter;
	public static GUIStyle LabelRight;
	public static GUIStyle LowerLeft;
	public static GUIStyle UpperLeft;

	public Texture2D filler;
	public Texture2D logo;
	public Texture2D build;

	private Color scrollItemColor = new Color(0.4f,0.4f,0.4f);
	private Color scrollItemColorAlt = new Color(0.2f,0.2f,0.2f);
	private Color scrollItemFocus = new Color(0.7f,0.4f,0.4f);
	private Color scrollItemFocusAlt = new Color(0.6f,0.2f,0.2f);

	private Vector2 serverScrollPosition = Vector2.zero;
	private int scrollItemHeight = 22;
	private int selectedServer = -1;

	//Player prefs
	public static bool GameTips;

	//GUI info
	private Rect creditsRect;
	private Rect creditsScrollView;
	private Vector2 creditsScrollPos = Vector2.zero;

	private Rect patchNotesRect;

	private string creditsText = "<b>Lead Designer/Programmer</b>\nKeirron Stach\n\n" +
		"<b>Sound Assetes</b>\nFreesound.org Creative Commons\n\n" +
		"<b>Music</b>\nChristopher Morrish\n\n" +
		"<b>A* Pathfinding</b>\nhttp://www.arongranberg.com/\n\n" +
		"<b>QA Testing</b>\nSam Dalby\nDamien Raines\nJordan Green\nEmily Rich" +
		"\n\n<b>Special Thanks</b>\nKarleigh Brown\nLuke Cross\nJessica Muller";

	float buttonWidth = 1;
	float buttonHeight = 1;
	float buttonSizeW = 1;
	float buttonSizeH = 1;
	float buttonMargin = 0.1f;
	int buttonI = 0;

	// Use this for initialization
	void Start () {
		serverSysFile = Logger.Path() + "server.sys";

		GetPlayerName ();

		//if (Authenticated)
			CurrentScreen = ScreenType.MainMenu;
		//else
			//CurrentScreen = ScreenType.Login;

		float boxWidth = Screen.width * 0.25f;
		float boxHeight = Screen.height * 0.5f;

		creditsRect = new Rect (0, Screen.height - boxHeight, boxWidth, boxHeight);
		creditsScrollView = new Rect (creditsRect.x+8, creditsRect.y+8, boxWidth - 16, boxHeight - 16);

		if (PlayerPrefs.GetInt ("GameTips", 1) == 1)
			GameTips = true;
		else
			GameTips = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI()
	{

		GUI.skin = MenuSkin;
		if(LabelCenter == null)
		{
			LabelCenter = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LabelCenter.alignment = TextAnchor.UpperCenter;
		}
		if(LabelRight == null)
		{
			LabelRight = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LabelRight.alignment = TextAnchor.UpperRight;
		}
		if(UpperLeft == null)
		{
			UpperLeft = new GUIStyle (GUI.skin.GetStyle ("Label"));
			UpperLeft.alignment = TextAnchor.UpperLeft;
		}
		if(LowerLeft == null)
		{
			LowerLeft = new GUIStyle (GUI.skin.GetStyle ("Label"));
			LowerLeft.alignment = TextAnchor.LowerLeft;
		}

		Screen.showCursor = true;

		if(CurrentScreen == ScreenType.Connecting)
		{
			GUI.Label(new Rect(Screen.width/2-150,Screen.height/2,300,20),"Connecting...", LabelCenter);
			if(GUI.Button(new Rect(Screen.width/2-25,Screen.height/2+50,50,20),"Stop"))
			{
				CurrentScreen = ScreenType.MainMenu;
			}
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
			hostList = MasterServer.PollHostList();

			//GUI Measurements
			buttonWidth = Screen.width*0.18f;
			buttonHeight = buttonWidth*0.2f;
			buttonMargin = Screen.width*0.01f;
			buttonSizeW = Screen.width*0.2f;
			buttonSizeH = buttonHeight + buttonMargin*2;
			
			Rect ServerListRect = new Rect(buttonSizeW,buttonSizeH,Screen.width-buttonSizeW,Screen.height-buttonHeight*2);
			float serverListHeight = 200;
			if(hostList != null) serverListHeight = hostList.Length * buttonHeight;

			//If rect is too short it will not display properly in the scrollview. make it the scroll view height.
			if(serverListHeight < ServerListRect.height) serverListHeight = ServerListRect.height;
			
			Rect ServerListInside = new Rect(0,0,ServerListRect.width-20,serverListHeight);
			
			serverScrollPosition = GUI.BeginScrollView(ServerListRect, serverScrollPosition, ServerListInside, false, true);
			
			if(hostList != null)
			{
				float scrollItemNameWidth = ServerListInside.width*0.6f;
				float scrollItemPlayersWidth = ServerListInside.width*0.2f;
				float scrollItemPingWidth = ServerListInside.width*0.2f;
				
				int iMeasure = 0;
				for(int i = 0; i < hostList.Length; i++)
				{
					//if(hostPings[i].isDone)
					if(true)
					{
						
						Rect scrollItem = new Rect(0,iMeasure*(scrollItemHeight+2), ServerListInside.width,scrollItemHeight);
						Rect scrollItemName = new Rect(0,scrollItem.y,scrollItemNameWidth-2,scrollItemHeight);
						Rect scrollItemPlayers = new Rect(scrollItemNameWidth, scrollItem.y, scrollItemPlayersWidth-2, scrollItemHeight);
						Rect scrollItemPing = new Rect(scrollItemNameWidth + scrollItemPlayersWidth, scrollItem.y, scrollItemPingWidth, scrollItemHeight);
						
						//Draw Server name
						Color boxColor = scrollItemColor;
						if(selectedServer == i)
							boxColor = scrollItemFocus;
						GUI.color = boxColor;
						GUI.DrawTexture(scrollItemName, filler);
						GUI.DrawTexture(scrollItemPing, filler);
						boxColor = scrollItemColorAlt;
						if(selectedServer == i)
							boxColor = scrollItemFocusAlt;
						GUI.color = boxColor;
						GUI.DrawTexture(scrollItemPlayers, filler);
						
						
						GUI.color = Color.white;
						
						GUI.Label(scrollItemName, hostList[i].gameName);
						GUI.Label(scrollItemPlayers, hostList[i].connectedPlayers + " / " + hostList[i].playerLimit);

						Ping ping;

						if(hostPings.ContainsKey(hostList[i].gameName))
						{
							ping  = hostPings[hostList[i].gameName];
							if(ping != null && hostPings[hostList[i].gameName].isDone)
								GUI.Label(scrollItemPing, ping.time.ToString());
						}
						else
							GUI.Label(scrollItemPing, "--");
						
						if(Input.GetMouseButtonDown(0) && scrollItem.Contains(Event.current.mousePosition))
						{
							selectedServer = i;
							LanIP = "";
						}
						
						iMeasure++;
					}
				}
			}
			
			GUI.EndScrollView(true);

			buttonI = 0;

			if(GUI.Button(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight), "Join Game"))
			{
				
				NetworkManager.playerName = Username;
				
				Network.isMessageQueueRunning = false;
				if(LanIP == "" && selectedServer > -1)
				{
					Network.Connect(hostList[selectedServer]);
					CurrentScreen = ScreenType.Connecting;
				}
				else if(LanIP != "")
				{
					Network.Connect(LanIP, int.Parse(LanPort));
					CurrentScreen = ScreenType.Connecting;
				}
			}

			GUI.Label(new Rect(buttonWidth + buttonMargin*2, buttonHeight/2, 300,30), "Status: "+hostListStatus);

			buttonI++;

			if(GUI.Button(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight), "Refresh"))
			{
				MasterServer.ClearHostList();
				hostListStatus = "Refreshing... ";
				RefreshHostList();
			}
			buttonI++;

			GUI.Label(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight/2), "Name");
			Username = GUI.TextArea(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH) + buttonHeight/2, buttonWidth,buttonHeight/2), Username);
			buttonI++;

			GUI.Label(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight/2), "JoinIP");
			LanIP = GUI.TextArea(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH) + buttonHeight/2, buttonWidth,buttonHeight/2), LanIP);
			LanPort = GUI.TextArea(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH) + (buttonHeight/2)*2, buttonWidth,buttonHeight/2), LanPort);
			buttonI+=2;

			GameTips = GUI.Toggle(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight), GameTips, " Use in game hints");

			if(GameTips)
				PlayerPrefs.SetInt("GameTips",1);
			else
				PlayerPrefs.SetInt("GameTips",0);

			buttonI ++;

			if(GUI.Button(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight), "Back"))
			{
				CurrentScreen = ScreenType.MainMenu;
			}


		}

		if(CurrentScreen == ScreenType.Login)
		{
			buttonWidth = Screen.width*0.15f;
			buttonHeight = buttonWidth*0.3f;
			float buttonDivider = buttonHeight *0.2f;
			
			buttonI = 0;

			float estY = (buttonHeight*3) + (buttonDivider*2);

			float x = Screen.width/2 - buttonWidth/2;
			float y = Screen.height/2-estY/2 + buttonHeight;
			
			float logoRatio = (float)logo.height / (float)logo.width;
			float logoW = buttonWidth*3;
			float logoH = logoW*logoRatio;
			float logoX = Screen.width/2 - logoW/2;
			float logoY = y + buttonHeight - logoH;
			
			float buildRatio = (float)build.height / (float)build.width;
			float buildW = Screen.width/3;
			float buildH = buildW * buildRatio;
			float buildX = Screen.width/2 - buildW/2;
			float buildY = logoY - buttonDivider - buildH;
			
			GUI.DrawTexture(new Rect(logoX,logoY,logoW,logoH), logo);
			GUI.DrawTexture(new Rect(buildX,buildY,buildW,buildH), build);

			buttonI++;
			GUI.Label(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)), buttonWidth,buttonHeight/2), "Username", LabelCenter);
			Username = GUI.TextField(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)) + buttonHeight/2, buttonWidth,buttonHeight/2), Username);
			buttonI++;

			GUI.Label(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)), buttonWidth,buttonHeight/2), "Password", LabelCenter);
			password = GUI.PasswordField(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)) + buttonHeight/2, buttonWidth,buttonHeight/2), password, '*');
			buttonI++;

			if(GUI.Button(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)), buttonWidth,buttonHeight/2), "Login"))
			{
				//Boolean check whether or not the username and password could be verified.
				bool verified = false;

				const string SERVER_VALID_DATA_HEADER = "LOGGED__";
				const int GID = 1;
				
				string hash = HashString(Username + password + GID + "BLULOG");
				string postString = "user=" + Username +
					"&pass=" + password +
						"&gid=" + GID +
						"&hash=" + hash;

				string logString = "";
				
				try
				{
					logString = WebPost("http://blurift.com/scripts/game_auth.php", postString);
				}
				catch (System.Exception e)
				{
					throw e;
				}

				if(logString.Trim().Length > SERVER_VALID_DATA_HEADER.Length)
				{
					if (logString.Trim().Substring(0, SERVER_VALID_DATA_HEADER.Length).Equals(SERVER_VALID_DATA_HEADER))
					{
						string toParse = logString.Trim().Substring(SERVER_VALID_DATA_HEADER.Length);
						
						string[] licences = Regex.Split(toParse, "__L__");
						
						int count = 0;

						//Check each licence to make sure its correct.
						for (int i = 0; i < licences.Length; i++)
						{
							string[] licenceData = Regex.Split(licences[i], "__D__");
							if(licenceData.Length > 1)
								count++;
						}
						
						if (count > 0)
						{
							verified = true;

							SavePlayerName();
							/*
							if (File.Exists(playerFile))
								File.Delete(playerFile);
							StreamWriter writer = new StreamWriter(playerFile);
							
							writer.WriteLine(user);
							writer.WriteLine(pass);
							writer.WriteLine(count);
							
							writer.Close();
							*/
							
						}
						else
						{
							authError = "No game licence.";
						}
					}
				}

				if(logString.Trim().Equals("__NO_AUTH__"))
					authError = "Wrong username/password";
				else if(logString.Trim().Equals("__NO_GAME_PASS__"))
					authError = "No game licence.";

				if(verified)
				{
					CurrentScreen = ScreenType.MainMenu;
				}
			}
			buttonI++;

			GUI.Label(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)), buttonWidth,buttonHeight/2), authError, LabelCenter);

		}

		if(CurrentScreen == ScreenType.MainMenu)
		{
			buttonWidth = Screen.width*0.15f;
			buttonHeight = buttonWidth*0.3f;
			float buttonDivider = buttonHeight *0.2f;
			
			buttonI = 0;

			float estY = (buttonHeight*4) + (buttonDivider*3);

			if(File.Exists(serverSysFile))
				estY += buttonHeight + buttonDivider;

			float x = Screen.width/2 - buttonWidth/2;
			float y = Screen.height/2-estY/2 + buttonHeight;

			float logoRatio = (float)logo.height / (float)logo.width;
			float logoW = buttonWidth*3;
			float logoH = logoW*logoRatio;
			float logoX = Screen.width/2 - logoW/2;
			float logoY = y + buttonHeight - logoH;

			float buildRatio = (float)build.height / (float)build.width;
			float buildW = Screen.width/3;
			float buildH = buildW * buildRatio;
			float buildX = Screen.width/2 - buildW/2;
			float buildY = logoY - buttonDivider - buildH;

			GUI.DrawTexture(new Rect(logoX,logoY,logoW,logoH), logo);
			GUI.DrawTexture(new Rect(buildX,buildY,buildW,buildH), build);

			//GUI.Label(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)), buttonWidth,buttonHeight/2), Username, LabelCenter);
			buttonI+=1;

			if(File.Exists(serverSysFile))
			{
				if(GUI.Button(new Rect(x,y + (buttonI*(buttonHeight+buttonDivider)),buttonWidth,buttonHeight), "Start Server"))
				{
					NetworkManager.Server = true;
					Network.SetLevelPrefix(1);
					Application.LoadLevel(1);
					//NetworkManager.StartServer();
				}
				buttonI+=1;
			}

			if(GUI.Button(new Rect(x, y + (buttonI*(buttonHeight+buttonDivider)),buttonWidth,buttonHeight), "Play"))
			{
				SavePlayerName();
				selectedServer = -1;
				hostListStatus= " Finding...";
				RefreshHostList();

				CurrentScreen = ScreenType.FindGame;
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



			buttonI = 0;

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

			if(GUI.Button(new Rect(buttonDivider,buttonI*(buttonHeight+buttonDivider)+buttonDivider, buttonWidth,buttonHeight), "Main Menu"))
			{
				CurrentScreen = ScreenType.MainMenu;
			}
			buttonI++;
		}
	}

	private void RefreshHostList()
	{
		MasterServer.RequestHostList (typeName);
		hostPings = new Dictionary<string, Ping> ();
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.HostListReceived)
		{
			hostList = MasterServer.PollHostList ();

			hostListStatus = "Found " + hostList.Length + " servers.";
		}


	}

	void OnConnectedToServer()
	{

		NetworkManager.playerName = Username;
		NetworkManager.Server = false;

		LevelLoader.Instance.LoadLevel (1);

	}

	void OnFailedToConnect(NetworkConnectionError error) 
	{
		CurrentScreen = ScreenType.Error;
		ScreenMessage = "Could not connect to server. " + error.ToString ();
		Logger.Write (ScreenMessage);
	}

	void SavePlayerName()
	{
		string file = Logger.Path() + "player.dat";
		if(File.Exists(file))
			File.Delete(file);
		StreamWriter writer = new StreamWriter(file);
		writer.WriteLine(Username);
		writer.WriteLine(password);
		writer.Close();
	}
	
	void GetPlayerName()
	{
		string file = Logger.Path() + "player.dat";
		if(File.Exists(file))
		{
			StreamReader reader = new StreamReader(file);
			Username = reader.ReadLine();
			if(!reader.EndOfStream)
				password = reader.ReadLine();
			else
				password = "";
			reader.Close();
		}
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

	private void SetPings()
	{
		for (int i = 0; i < hostList.Length; i++)
		{
			if(!hostPings.ContainsKey(hostList[i].gameName))
			{
				hostPings.Add(hostList[i].gameName, new Ping(hostList[i].ip[0]));
			}
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
}

public enum ScreenType
{
	MainMenu,
	FindGame,
	Options,
	Error,
	Connecting,
	Login,
}
