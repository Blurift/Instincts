using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuJoinGame {

    //Size
    float width = 0;
    float height = 0;

	public string hostListStatus;
	private string LanIP = "";
	private string LanPort = "";
	public HostData[] hostList = new HostData[0];
	private Dictionary<string, Ping> hostPings = new Dictionary<string, Ping> ();
	private NetworkSearchType networkSearchType = NetworkSearchType.Internet;

	//UI
	private Color scrollItemColor = new Color(0.4f,0.4f,0.4f);
	private Color scrollItemColorAlt = new Color(0.2f,0.2f,0.2f);
	private Color scrollItemFocus = new Color(0.7f,0.4f,0.4f);
	private Color scrollItemFocusAlt = new Color(0.6f,0.2f,0.2f);

    //GUI Measurements
    float buttonWidth;
    float buttonHeight;
    float buttonMargin;
    float buttonSizeW;
    float buttonSizeH;

    //Server list
    private GUIStyle serverLineStyle;

    private Rect serverListRect;
	
	private Vector2 serverScrollPosition = Vector2.zero;

    private float serverListInnerWidth;
	private float serverLineHeight = 25;

	private int selectedServer = -1;
	private int buttonI = 0;

    float scrollItemNameWidth;
    float scrollItemPlayersWidth;
    float scrollItemPingWidth;

	public MenuJoinGame()
	{
		selectedServer = -1;
		RefreshHostList ();
	}

    private void Resize()
    {
        height = Screen.height;
        width = Screen.width;

        //GUI Measurements
        buttonWidth = width * 0.18f;
        buttonHeight = buttonWidth * 0.2f;
        buttonMargin = width * 0.01f;
        buttonSizeW = width * 0.2f;
        buttonSizeH = buttonHeight + buttonMargin * 2;

        float serverListY = buttonHeight + buttonMargin * 2;

        serverListRect = new Rect(0, serverListY, width, height - serverListY - buttonSizeH);
        serverLineHeight = serverListRect.height / 20;
        serverLineStyle = Blurift.BluStyle.CustomStyle(GUI.skin.label, serverLineHeight * 0.8f);

        serverListInnerWidth = serverListRect.width - 20;

        scrollItemNameWidth = serverListInnerWidth * 0.6f;
        scrollItemPlayersWidth = serverListInnerWidth * 0.2f;
        scrollItemPingWidth = serverListInnerWidth * 0.2f;
    }

	public void Draw()
	{
        if(Screen.height != height || Screen.width != width)
        {
            Resize();
        }

		

		float screenY = buttonSizeH + buttonMargin * 2;
		
		//Network Search buttons and messages;
		{
			float buttonNetworkSearchWidth = buttonWidth/2;
			float networkSearchTypeOffset = buttonMargin;
			
			Rect networkSearchTypeButtonInternet = new Rect(networkSearchTypeOffset,buttonMargin,buttonNetworkSearchWidth,buttonHeight);
			networkSearchTypeOffset += buttonNetworkSearchWidth + buttonMargin;
			Rect networkSearchTypeButtonLAN = new Rect(networkSearchTypeOffset,buttonMargin,buttonNetworkSearchWidth,buttonHeight);
			networkSearchTypeOffset += buttonNetworkSearchWidth + buttonMargin;
			Rect networkSearchTypeButtonDirect = new Rect(networkSearchTypeOffset,buttonMargin,buttonNetworkSearchWidth,buttonHeight);
			networkSearchTypeOffset += buttonNetworkSearchWidth + buttonMargin;
			Rect backButton = new Rect (Screen.width - buttonMargin - buttonWidth, Screen.height - buttonHeight - buttonMargin, buttonWidth, buttonHeight);
			Rect networkSearchStatus = new Rect(networkSearchTypeOffset, buttonSizeH/2, 300,30);
			
			if(GUI.Button(networkSearchTypeButtonInternet, "Internet") && networkSearchType != NetworkSearchType.Internet)
			{
				RefreshHostList();
				networkSearchType = NetworkSearchType.Internet;
			}
			
			bool oldEnabled = GUI.enabled;
			GUI.enabled = false;
			if(GUI.Button(networkSearchTypeButtonLAN, "LAN"))
			{
				hostList = null;
				networkSearchType = NetworkSearchType.LAN;
			}
			GUI.enabled = oldEnabled;
			
			
			if(GUI.Button(networkSearchTypeButtonDirect, "Direct"))
			{
				hostList = null;
				hostListStatus = "Direct Connect";
				networkSearchType = NetworkSearchType.Direct;
			}

			if(GUI.Button(backButton, "Back"))
			{
				Menu.Instance.SwitchScreen(ScreenType.MainMenu);
			}
			
			GUI.Label(networkSearchStatus, "Status: "+hostListStatus);
		}

		if(networkSearchType == NetworkSearchType.Internet)
		{
            /* Test list 
            hostList = new HostData[25];
            for (int i = 0; i < hostList.Length; i++ )
            {
                hostList[i] = new HostData();
                hostList[i].gameName = "Game" + i;
                hostList[i].playerLimit = 4;
                hostList[i].connectedPlayers = 2;
            }*/
			
			
			//hostList = MasterServer.PollHostList();
			float serverListHeight = 200;
			if(hostList != null) serverListHeight = hostList.Length * (serverLineHeight+2);
			
			//If rect is too short it will not display properly in the scrollview. make it the scroll view height.
			if(serverListHeight < serverListRect.height) serverListHeight = serverListRect.height;
			
			Rect ServerListInside = new Rect(0,0,serverListInnerWidth,serverListHeight);
			
			serverScrollPosition = GUI.BeginScrollView(serverListRect, serverScrollPosition, ServerListInside, false, true);

			if(hostList != null && hostList.Length > 0)
			{				
				int iMeasure = 0;
				for(int i = 0; i < hostList.Length; i++)
				{

                    Rect scrollItem = new Rect(0, iMeasure * (serverLineHeight + 2), ServerListInside.width, serverLineHeight);
                    Rect scrollItemName = new Rect(serverLineHeight, scrollItem.y, scrollItemNameWidth - 2 - serverLineHeight, serverLineHeight);
                    Rect scrollItemPlayers = new Rect(scrollItemNameWidth, scrollItem.y, scrollItemPlayersWidth - 2, serverLineHeight);
                    Rect scrollItemPing = new Rect(scrollItemNameWidth + scrollItemPlayersWidth, scrollItem.y, scrollItemPingWidth, serverLineHeight);
						
					//Draw Server name
					Color boxColor = scrollItemColor;
					if(selectedServer == i)
						boxColor = scrollItemFocus;
					GUI.color = boxColor;
					GUI.DrawTexture(scrollItemName, Menu.Instance.filler);
					GUI.DrawTexture(scrollItemPing, Menu.Instance.filler);
					boxColor = scrollItemColorAlt;
					if(selectedServer == i)
						boxColor = scrollItemFocusAlt;
					GUI.color = boxColor;
					GUI.DrawTexture(scrollItemPlayers, Menu.Instance.filler);
						
						
					GUI.color = Color.white;
						
					GUI.Label(scrollItemName, hostList[i].gameName, serverLineStyle);
                    GUI.Label(scrollItemPlayers, hostList[i].connectedPlayers + " / " + hostList[i].playerLimit, serverLineStyle);
						
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
			else
			{
				GUI.Label(new Rect(10,10,300,50), "No servers");
			}
			
			GUI.EndScrollView(true);

			Rect joinButton = new Rect(buttonMargin,Screen.height - buttonMargin - buttonHeight, buttonWidth,buttonHeight);

			bool oldEnabled = GUI.enabled;
			GUI.enabled = oldEnabled && selectedServer > -1;
			if(GUI.Button(joinButton, "Connect"))
			{
					Network.Connect(hostList[selectedServer]);
					Menu.Instance.SwitchScreen(ScreenType.Connecting);
			}
			GUI.enabled = oldEnabled;
		}


		if(networkSearchType == NetworkSearchType.LAN)
		{
			Rect joinButton = new Rect(buttonMargin,Screen.height - buttonMargin - buttonHeight, buttonWidth,buttonHeight);
			
			if(GUI.Button(joinButton, "Connect"))
			{
				Network.Connect(LanIP, int.Parse(LanPort));
				Menu.Instance.SwitchScreen(ScreenType.Connecting);
			}
		}

		if(networkSearchType == NetworkSearchType.Direct)
		{
			int ir = 0;
			Rect ipLabel = new Rect(buttonMargin+((buttonWidth+buttonMargin)*ir), screenY, buttonWidth, buttonHeight);
			ir++;
			Rect ipRect = new Rect(buttonMargin+((buttonWidth+buttonMargin)*ir), screenY, buttonWidth, buttonHeight);
			ir++;
			Rect portLabel = new Rect(buttonMargin+((buttonWidth+buttonMargin)*ir), screenY, buttonWidth, buttonHeight);
			ir++;
			Rect portRect = new Rect(buttonMargin+((buttonWidth+buttonMargin)*ir), screenY, buttonWidth, buttonHeight);
			ir++;

			GUIStyle custom = Blurift.BluStyle.CustomStyle(GUI.skin.textField, 0.03f);

			GUI.Label(ipLabel, "IP Address", Menu.LabelLeft);
			GUI.Label(portLabel, "Port", Menu.LabelLeft);
			LanIP = GUI.TextField(ipRect, LanIP,custom);
			LanPort = GUI.TextField(portRect, LanPort,custom);

			Rect joinButton = new Rect(buttonMargin,Screen.height - buttonMargin - buttonHeight, buttonWidth,buttonHeight);
			
			if(GUI.Button(joinButton, "Connect"))
			{
				Network.Connect(LanIP, int.Parse(LanPort));
				Menu.Instance.SwitchScreen(ScreenType.Connecting);
			}
		}
	}

	private void RefreshHostList()
	{
		MasterServer.RequestHostList (Game.TypeName);
		hostPings = new Dictionary<string, Ping> ();
		hostListStatus = "Searching...";
	}
	
	private void NetworkSearchLanGames()
	{
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
}
