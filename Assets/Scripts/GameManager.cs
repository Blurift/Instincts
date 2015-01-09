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

    public static string WorldPath()
    {
        string path = Application.persistentDataPath + "/Servers/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        return path;
    }

    public static bool WorldPathExist(string world)
    {
        string path = WorldPath() + world;

        if (!Directory.Exists(path))
            return false;
        return true;
    }

	public static string WorldPathFormat(string server)
	{
        string path = WorldPath() +server;

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

		return path;
	}

	public static string WorldPlayerPath(string serverName)
	{
		return WorldPathFormat (serverName) + "/Players/";
	}

    public static string WorldStatePath(string serverName)
    {
        return WorldPathFormat(serverName) + "/States/";
    }

    public static string[] Servers()
    {
        return Directory.GetDirectories(WorldPath());
    }

    public static bool WorldDelete(string world)
    {
        Directory.Delete(WorldPathFormat(world), true);
        return true;
    }

    public static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
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
