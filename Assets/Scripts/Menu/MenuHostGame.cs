using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Blurift.UI;

public class MenuHostGame : MonoBehaviour {

    public ToggleList ToggleList;
    public InputField ServerName;
    public InputField Port;
    public Slider MaxPlayers;

    private ServerSettings settings = ServerSettings.Default;
    private string serverName;
    private int serverPort = 7000;
    private int maxPlayers = 4;

	// Use this for initialization
	void Start () {
	    //Get all world names
        //TODO

        if (PlayerPrefs.HasKey("ServerName"))
        {
            serverName = PlayerPrefs.GetString("ServerName");
            
            ServerName.text = serverName;
        }
        if (PlayerPrefs.HasKey("ServerPort"))
        {
            serverPort = PlayerPrefs.GetInt("ServerPort");
            Port.text = serverPort.ToString();
        }
        if (PlayerPrefs.HasKey("ServerMax"))
        {
            maxPlayers = PlayerPrefs.GetInt("ServerMax");
            MaxPlayers.value = maxPlayers;
        }

        int subStart = GameManager.WorldPath().Length;
        string[] worlds = GameManager.Servers();
        for (int i = 0; i < worlds.Length; i++)
        {
            ToggleList.AddItem(worlds[i].Substring(subStart));
        }
	}
	
	// Update is called once per frame
	void Update () {

    }

    #region Changes

    public void OnServerNameChange(string value)
    {
        serverName = value;
        PlayerPrefs.SetString("ServerName", value);
    }
    public void OnServerPortChange(string value)
    {
        try
        {
            serverPort = int.Parse(value);
            PlayerPrefs.SetInt("ServerPort", serverPort);
        }
        catch (System.Exception e)
        {
            Port.text = "";
        }
    }

    public void OnMaxPlayersChange(float value)
    {
        maxPlayers = (int)value;
        PlayerPrefs.SetInt("ServerMax", maxPlayers);
    }

    #endregion

    #region Buttons

    public void Create()
    {
        string world = serverName;
        if (GameManager.WorldPathExist(world))
            return;
        ToggleList.AddItem(world);
        GameManager.WorldPathFormat(world);        
    }

    public void Delete()
    {
        int index = ToggleList.GetCurrentIndex();

        GameManager.WorldDelete(ToggleList.GetCurrentValue());

        ToggleList.DeleteCurrentItem();
    }

    public void StartGame()
    {
        settings.MaxPlayers = maxPlayers;
        settings.Port = serverPort;
        settings.ServerName = serverName;
        

        NetworkManager.Settings = settings;
        NetworkManager.Server = true;
        Network.SetLevelPrefix(LevelLoader.LEVEL_GAME);
        Application.LoadLevel(LevelLoader.LEVEL_GAME);
    }

    #endregion
}
