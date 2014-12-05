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

	void OnGUI()
	{
		if(messageOpacity > 0 && Network.isServer && false)
		{
			Color gui = Color.white;
			gui.a = messageOpacity;
			GUI.color = gui;

			int width = 250;
			if(Network.isServer) width = 500;

			int y = 0;

			int start = messages.Count-21;
			if(start < 0)
				start = 0;

			for(int i = start;i < messages.Count; i++)
			{
				GUI.Label(new Rect(14,14+(20*y),width,20), messages[i]);

				y++;
			}

			GUI.color = Color.white;
		}
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

	private static string ServerPlayerPathFormat(string server)
	{
		return Application.persistentDataPath + "/" + server + "/Players/";
	}

	public static string ServerPlayerPath()
	{
		return ServerPlayerPathFormat ("def");
	}

	public static string ServerPlayerPath(string serverName)
	{
		return ServerPlayerPathFormat (serverName);
	}

	[RPC]
	void Message(string message)
	{
		messages.Add (message);
		if (messages.Count > 120)
			messages.RemoveAt (0);

		if (HUD.Instance != null)
			HUD.Instance.ChatUpdate ();
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

	//GUI
	private float lastMessage = 0;
	private float messageTime = 2.5f;
	private float messageFadeTime = 2.5f;

	private float messageOpacity = 0f;

}
