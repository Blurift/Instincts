using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;
	public static Inventory ControllingInventory;

    public static List<Sprite> HairStyles;

	public static Vector3 MousePosition = Vector3.zero;

    public static Random Random = new Random();

    #region GlobalGame Variables

    public static float MusicLevel = 1;
    public static float SoundLevel = 1;

    #endregion

    void Start()
	{
		Instance = this;

		if(Debug.isDebugBuild)
		{
			//Debug.Log("Logger: " + Application.dataPath);
			//Debug.Log("Logger: " + Application.persistentDataPath);
		}

	}

	void Update()
	{
		MousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		//if(Network.isServer)
		//	messageOpacity = 1;
	}

	public static string SavePath()
	{
		string dataPath = Application.dataPath;
		dataPath = dataPath.Substring (0, dataPath.LastIndexOf ("/")+1);
		return dataPath;
	}

	public static string ProfilePath()
	{
		string path = Application.persistentDataPath;
		path += "/Profiles/";

		if (!Directory.Exists (path))
			Directory.CreateDirectory (path);

		return path;
	}

    private static string ServerPath()
    {
        string path = Application.persistentDataPath + "/Servers/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }

	private static string ServerPathFormat(string server)
	{
        string path = ServerPath() +server;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

		return path;
	}

	public static string ServerPlayerPath(string serverName)
	{
		return ServerPathFormat (serverName) + "/Players/";
	}

    public static string ServerStatePath(string serverName)
    {
        return ServerPathFormat(serverName) + "/States/";
    }

    public static string[] Servers()
    {
        return Directory.GetDirectories(ServerPath());
    }

	[RPC]
	void Message(string message)
	{
		messages.Add (message);
		if (messages.Count > 120)
			messages.RemoveAt (0);

		//if (HUD.Instance != null)
			//HUD.Instance.ChatUpdate ();
	}

	[RPC]
	void SendMessageServer(string message, NetworkPlayer player)
	{
		if(Network.isServer)
		{
			string formatMessage = NetworkManager.GetPlayerName(player) + ": " + message;
			Message(formatMessage);
			networkView.RPC("Message", RPCMode.Others, formatMessage);
		}
	}

	/// <summary>
	/// Sends a message to everyone connected to the server
	/// </summary>
	/// <param name="message">Message to be sent.</param>
	public static void NetMessage(string message)
	{
		if(Network.isServer)
		{
			//Instance.Message(message);
			//Instance.networkView.RPC("Message", RPCMode.Others, message);
			Instance.SendMessageServer(message, Network.player);
		}
		else
		{
			Instance.networkView.RPC("SendMessageServer", RPCMode.Server, message, Network.player);
		}
	}

	public static void ServerMessage(string message)
	{
		if(Network.isServer)
		{
			Instance.Message("Server: " + message);
			Instance.networkView.RPC("Message", RPCMode.Others, "Server: " + message);
		}
	}

	public static void WriteMessage(string message)
	{
		Instance.Message (message);
	}

	//Message System
	public static List<string> messages = new List<string> ();



}
