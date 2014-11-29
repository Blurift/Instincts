using UnityEngine;
using System.Collections;

public class MenuHost {

	private ServerSettings settings = ServerSettings.Default;

	private Rect backButton;
	private Rect startButton;

	public MenuHost()
	{
		if (PlayerPrefs.HasKey ("ServerName"))
			settings.ServerName = PlayerPrefs.GetString ("ServerName");
		if (PlayerPrefs.HasKey ("ServerPort"))
			settings.Port = PlayerPrefs.GetInt ("ServerPort");
		if (PlayerPrefs.HasKey ("ServerPlayers"))
			settings.MaxPlayers = PlayerPrefs.GetInt ("ServerPlayers");
	}

	private void Setup()
	{
		//GUI Measurements
		float buttonWidth = Screen.width*0.18f;
		float buttonHeight = buttonWidth*0.2f;
		float buttonMargin = Screen.width*0.01f;
		float buttonSizeW = Screen.width*0.2f;
		float buttonSizeH = buttonHeight + buttonMargin*2;
	}

	public void Draw()
	{
		//GUI Measurements
		float buttonWidth = Screen.width*0.18f;
		float buttonHeight = buttonWidth*0.2f;
		float buttonMargin = Screen.width*0.01f;
		float buttonSizeW = Screen.width*0.2f;
		float buttonSizeH = buttonHeight + buttonMargin*2;

		backButton = new Rect (Screen.width - buttonMargin - buttonWidth, Screen.height - buttonHeight - buttonMargin, buttonWidth, buttonHeight);
		startButton = new Rect (buttonMargin, Screen.height - buttonHeight - buttonMargin, buttonWidth, buttonHeight);

		float textFieldWidth = Screen.width * 0.22f;
		float textFieldHeight = textFieldWidth * 0.15f;
		float textMargin = Screen.width * 0.015f;

		GUIStyle textField = Blurift.BluStyle.CustomStyle (GUI.skin.textField, 0.03f);
        GUIStyle labelFont = Blurift.BluStyle.CustomStyle (Menu.LabelLeft, 1);

		StyleHelper.SetFontSize (textField, textFieldHeight * 0.4f);
		StyleHelper.SetFontSize (labelFont, textFieldHeight * 0.7f);

		int optionPoint = 0;
		Rect serverNameRect = new Rect (Screen.width *0.265f, Screen.height *0.1f + (textFieldHeight+textMargin)*optionPoint, textFieldWidth, textFieldHeight);
		Rect serverNameLabel = new Rect (textMargin,Screen.height *0.1f + (textFieldHeight+textMargin)*optionPoint, textFieldWidth,textFieldHeight);
		optionPoint++;
		Rect serverPortRect = new Rect (Screen.width *0.2655f, Screen.height *0.1f + (textFieldHeight+textMargin)*optionPoint, textFieldWidth, textFieldHeight);
		Rect serverPortLabel = new Rect (textMargin,Screen.height *0.1f + (textFieldHeight+textMargin)*optionPoint, textFieldWidth,textFieldHeight);
		optionPoint++;
		Rect serverMaxRect = new Rect (Screen.width *0.265f, Screen.height *0.1f + (textFieldHeight+textMargin)*optionPoint + textFieldHeight/2, textFieldWidth, textFieldHeight);
		Rect serverMaxLabel = new Rect (textMargin,Screen.height *0.1f + (textFieldHeight+textMargin)*optionPoint, textFieldWidth,textFieldHeight);


		GUI.Label (serverNameLabel, "Server Name:", labelFont);
		GUI.Label (serverPortLabel, "Port:", labelFont);
		GUI.Label (serverMaxLabel, "Max Players: " + settings.MaxPlayers, labelFont);

		settings.ServerName = GUI.TextField (serverNameRect, settings.ServerName, textField);
		string port = GUI.TextField (serverPortRect, settings.Port.ToString(), textField);
		//string max = GUI.TextField (serverMaxRect, settings.MaxPlayers.ToString(), textField);
		settings.MaxPlayers = (int)GUI.Slider(serverMaxRect, settings.MaxPlayers, 1, 1,17, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 0);

		try {
			settings.Port = int.Parse(port);

		} catch (System.Exception e)
		{
		}


		//Display buttons
		if(GUI.Button(backButton, "Back"))
		{
			Menu.Instance.SwitchScreen(ScreenType.MainMenu);
		}

		//Display buttons
		if(GUI.Button(startButton, "Start"))
		{
			PlayerPrefs.SetInt("ServerPort", settings.Port);
			PlayerPrefs.SetInt("ServerPlayers", settings.MaxPlayers);
			PlayerPrefs.SetString("ServerName", settings.ServerName);


			NetworkManager.Settings = settings;
			NetworkManager.Server = true;
			Network.SetLevelPrefix(LevelLoader.LEVEL_GAME);
			Application.LoadLevel(LevelLoader.LEVEL_GAME);
		}
	}
}
