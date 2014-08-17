using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuJoinGame {

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
	
	private Vector2 serverScrollPosition = Vector2.zero;
	private int scrollItemHeight = 22;
	private int selectedServer = -1;
	private int buttonI = 0;

	public MenuJoinGame()
	{
		selectedServer = -1;

		RefreshHostList ();
	}



	public void Draw()
	{
		//GUI Measurements
		float buttonWidth = Screen.width*0.18f;
		float buttonHeight = buttonWidth*0.2f;
		float buttonMargin = Screen.width*0.01f;
		float buttonSizeW = Screen.width*0.2f;
		float buttonSizeH = buttonHeight + buttonMargin*2;

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
			//Server List gui measurements
			Rect ServerListRect = new Rect(0,screenY,Screen.width,Screen.height - screenY - buttonSizeH);
			
			
			hostList = MasterServer.PollHostList();
			float serverListHeight = 200;
			if(hostList != null) serverListHeight = hostList.Length * buttonHeight;
			
			//If rect is too short it will not display properly in the scrollview. make it the scroll view height.
			if(serverListHeight < ServerListRect.height) serverListHeight = ServerListRect.height;
			
			Rect ServerListInside = new Rect(0,0,ServerListRect.width-20,serverListHeight);
			
			serverScrollPosition = GUI.BeginScrollView(ServerListRect, serverScrollPosition, ServerListInside, false, true);

			if(hostList != null && hostList.Length > 0)
			{
				float scrollItemNameWidth = ServerListInside.width*0.6f;
				float scrollItemPlayersWidth = ServerListInside.width*0.2f;
				float scrollItemPingWidth = ServerListInside.width*0.2f;
				
				int iMeasure = 0;
				for(int i = 0; i < hostList.Length; i++)
				{
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
						GUI.DrawTexture(scrollItemName, Menu.Instance.filler);
						GUI.DrawTexture(scrollItemPing, Menu.Instance.filler);
						boxColor = scrollItemColorAlt;
						if(selectedServer == i)
							boxColor = scrollItemFocusAlt;
						GUI.color = boxColor;
						GUI.DrawTexture(scrollItemPlayers, Menu.Instance.filler);
						
						
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

			GUIStyle custom = Menu.GetCustomStyleFont(GUI.skin.textField, 0.03f);

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

		/*

		
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
					GUI.DrawTexture(scrollItemName, Menu.Instance.filler);
					GUI.DrawTexture(scrollItemPing, Menu.Instance.filler);
					boxColor = scrollItemColorAlt;
					if(selectedServer == i)
						boxColor = scrollItemFocusAlt;
					GUI.color = boxColor;
					GUI.DrawTexture(scrollItemPlayers, Menu.Instance.filler);
					
					
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
			Network.isMessageQueueRunning = false;
			
			switch(networkSearchType)
			{
			case NetworkSearchType.Internet:
				Network.Connect(hostList[selectedServer]);
				Menu.Instance.SwitchScreen(ScreenType.Connecting);
				break;
			case NetworkSearchType.LAN:
				break;
			case NetworkSearchType.Direct:
				Network.Connect(LanIP, int.Parse(LanPort));
				Menu.Instance.SwitchScreen(ScreenType.Connecting);
				break;
			}
		}
		
		
		
		buttonI++;
		
		if(GUI.Button(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight), "Refresh"))
		{
			MasterServer.ClearHostList();
			hostListStatus = "Refreshing... ";
			RefreshHostList();
		}
		buttonI++;
		
		GUI.Label(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight/2), "Direct Connect");
		LanIP = GUI.TextArea(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH) + buttonHeight/2, buttonWidth,buttonHeight/2), LanIP);
		LanPort = GUI.TextArea(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH) + (buttonHeight/2)*2, buttonWidth,buttonHeight/2), LanPort);
		buttonI+=2;
		
		if(GUI.Button(new Rect(buttonMargin,buttonMargin + (buttonI*buttonSizeH), buttonWidth,buttonHeight), "Back"))
		{
			Menu.Instance.SwitchScreen(ScreenType.MainMenu);
		}

*/

	}

	private void RefreshHostList()
	{
		MasterServer.RequestHostList (Menu.TypeName);
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
