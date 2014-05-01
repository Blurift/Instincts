using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public static GameManager Instance;
	public static Inventory ControllingInventory;

	public static Vector3 MousePosition = Vector3.zero;

	void Start()
	{
		Instance = this;

	}

	void Update()
	{
		MousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		if(Network.isServer)
			messageOpacity = 1;
	}

	void OnGUI()
	{
		if(messageOpacity > 0 && Network.isServer)
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

	[RPC]
	void Message(string message)
	{
		messages.Add (message);
		if (!Network.isServer && HUD.Instance != null)
			HUD.Instance.ChatUpdate ();

		if (messages.Count > 120)
			messages.RemoveAt (0);
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

	public static void NetMessage(string message)
	{
		if(Network.isServer)
		{
			Instance.Message(message);
			Instance.networkView.RPC("Message", RPCMode.Others, message);
		}
		else
		{
			Instance.networkView.RPC("SendMessageServer", RPCMode.Server, message, Network.player);
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
