using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NetworkManager : MonoBehaviour {

	public static NetworkManager Instance = null;

	//Static info
	private const string typeName = "BluriftInstinctsPreview";

	private string serverSysFile = "";
	public static string playerName = "";
	public static bool Server = false;

	//PrefabInfo
	public GameObject PlayerPrefab;
	public GameObject[] PlayerPrefabList;

	//Players
	private Dictionary<NetworkPlayer,GameObject> Players;
	private Dictionary<NetworkPlayer, string> PlayerNames;

	public static void SendRPC(NetworkView view, string name, params object[] values)
	{
		SendRPC(view, null, name, values);
	}

	public static void SendRPC(NetworkView view, NetworkPlayer except, string name, params object[] values)
	{
		foreach(KeyValuePair<NetworkPlayer, GameObject> pair in Instance.Players)
		{
			if( pair.Value.networkView.owner != except)
			{
				view.RPC(name, pair.Key, values);
			}
		}
	}

	public static GameObject GetPlayer(NetworkPlayer player)
	{
		foreach(KeyValuePair<NetworkPlayer, GameObject> pair in Instance.Players)
		{
			if(pair.Key == player)
				return pair.Value;
		}
		
		return null;
	}

	public static string GetPlayerName(NetworkPlayer player)
	{
		foreach(KeyValuePair<NetworkPlayer, string> pair in Instance.PlayerNames)
		{
			if(pair.Key == player)
				return pair.Value;
		}
		
		return null;
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		if(msEvent == MasterServerEvent.RegistrationFailedGameName)
			GameManager.WriteMessage("Server Registration Failed: No game name given");
		if(msEvent == MasterServerEvent.RegistrationFailedGameType)
			GameManager.WriteMessage("Server Registration Failed: No game tpye given");
		if(msEvent == MasterServerEvent.RegistrationFailedNoServer)
			GameManager.WriteMessage("Server Registration Failed: Server not running");
		if(msEvent == MasterServerEvent.RegistrationSucceeded)
			GameManager.WriteMessage("Server Registraion Suceeded");
	}

	// Use this for initialization
	void Start () {
		Instance = this;

		GameManager.WriteMessage("Deciding between host or client.");
		if(Server)
		{
			StartServer();
			AudioListener.volume = 0;
		}
		else
			JoinServer(playerName,"");
	}
	
	// Update is called once per frame
	void Update () {
		if(Network.isServer)
		{
			/*Vector3 move = ((Input.GetAxis ("Vertical") * transform.up) + (Input.GetAxis ("Horizontal") * transform.right)).normalized;
			
			move*= 15*Time.deltaTime;
			Camera.main.transform.position += move;
			*/
		}
	}

	//Server Methods
	public static void StartServer()
	{
		Debug.Log ("Server starting");
		GameManager.WriteMessage("Attempting to start server");

		Instance.serverSysFile = Logger.Path() + "server.sys";

		Instance.Players = new Dictionary<NetworkPlayer, GameObject> ();
		Instance.PlayerNames = new Dictionary<NetworkPlayer, string> ();

		ServerSettings settings = ServerSettings.Default;
		bool setup = false;


		StreamReader reader = new StreamReader (Instance.serverSysFile);
		try
		{
			settings.ServerName = reader.ReadLine ();
			settings.MaxPlayers = int.Parse(reader.ReadLine());
			settings.Port = int.Parse(reader.ReadLine());
			if(!reader.ReadLine().Equals("True"))
				settings.UseNAT = false;
			setup = true;
		}
		catch (System.Exception)
		{
			GameManager.WriteMessage("Couldnt read server profile");
			return;
		}
		reader.Close ();

		//Fix some settings;
		if(settings.MaxPlayers > 16)
			settings.MaxPlayers = 16;

		if(!settings.Dedicated)
		{
			settings.MaxPlayers--;
		}
		//else
		//	Camera.main.gameObject.AddComponent (typeof(AudioListener));

		MasterServer.dedicatedServer = settings.Dedicated;
		Network.InitializeServer(settings.MaxPlayers, settings.MaxPlayers, settings.UseNAT);

		if(!settings.LAN)
		{
			MasterServer.RegisterHost (typeName, settings.ServerName);
		}

		GameManager.WriteMessage ("Starting server with " + settings.MaxPlayers + " max players on port " + settings.Port);
	}

	public static void JoinServer(string user, string pass)
	{
		//Debug.Log(Instance.networkView.viewID.ToString ());
		//Debug.Log ("Logging to server " + user + " : " + pass + " : " + Network.player.ToString ());
		Instance.networkView.RPC ("LoginToServer", RPCMode.Server, user, pass, Network.player, Network.AllocateViewID ());
	}

	private void KillServer()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");

		for (int i = players.Length-1; i > -1; i--)
		{
			//Destroy player object.
			Destroy(players[i]);
		}

		GameObject[] ai = GameObject.FindGameObjectsWithTag ("AI");

		for (int i = 0; i < ai.Length; i++)
		{
			Destroy (ai[i]);
		}
	}

	//Server Events
	void OnServerInitialized()
	{
		GameManager.WriteMessage ("Server Started");
		LoginToServer (playerName, "", Network.player, Network.AllocateViewID ());
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		GameManager.WriteMessage ("Player " + player.ToString () + " connecting");
	}

	[RPC]
	void RandomMessage(string mes)
	{
		Debug.Log (mes);
	}

	[RPC]
	void LoginToServer(string user, string pass, NetworkPlayer player, NetworkViewID id)
	{
		if(Network.isServer)
		{
			GameManager.NetMessage(user + " joined the game");
			//Logger.Write("Player " + player.ToString() + " : " + user + " - " +player.ipAddress + " trying to log in");
			
			//Loop through all connected players and send details.
			networkView.RPC("RandomMessage", player, "Sending players through");
			foreach(KeyValuePair<NetworkPlayer, GameObject> pair in Players)
			{
				if(pair.Key != player)
				{
					Debug.Log("Sending player data : " + pair.Value.ToString() + " to " + player.ToString() + " : " + id.ToString());
					networkView.RPC("SpawnPlayer", player, pair.Value.networkView.viewID, pair.Value.transform.position, PlayerNames[pair.Key]);
					//networkView.RPC("RandomMessage", player, pair.Value.networkView.viewID.ToString() + "being sent through");
					pair.Value.networkView.SetScope(player, true);
					pair.Value.networkView.RPC("ResetNetworkView", pair.Key);
				}
			}

			//Test for player

			Players[player] = null;
			PlayerNames[player] = user;


			if(!LoadPlayerState(id,player))
			{
				SpawnPlayer(id, GetSpawnLocation(), user);
			}

			Players[player].GetComponent<Inventory>().AddToInventory("WoodStick",1);

			//Loop through all items
			ItemManager.InitializeDropsForPlayer(player);
			//GameManager.SendMessage("Sent " + itemCount + " itemSpawns to " + PlayerNames[player], true);

			//Loop through all AI
			networkView.RPC("RandomMessage", player, "Sending AI through");
			foreach(GameObject go in GameObject.FindGameObjectsWithTag("AI"))
			{
				go.networkView.SetScope(player, true);
				AIManager.Instance.SpawnEnemy(go, player);
			}

			//Sync All Props to new player.
			DestructableManager.SyncPropsNewPlayer(player);
		}
	}

	[RPC]
	void SpawnPlayer(NetworkViewID id, Vector3 pos, string name)
	{
		Debug.Log ("Spawning Player " + pos.ToString());

		if(Network.isServer)
		{
			if(PlayerPrefab != null)
			{
				Logger.Write("Player " + id.owner.ToString() + " spawned at " + pos.ToString());
				//Respawn player
				
				GameObject newPlayer = (GameObject)GameObject.Instantiate(PlayerPrefab, pos, Quaternion.identity);
				
				newPlayer.networkView.viewID = id;
				newPlayer.name = name;
				
				Players[id.owner] = newPlayer;
				
				networkView.RPC("SpawnPlayer", RPCMode.Others, id, pos, name);
			}
		}
		else if(Network.isClient)
		{
			GameObject newPlayer = (GameObject)GameObject.Instantiate(PlayerPrefab, pos, Quaternion.identity);

			PlayerController p = newPlayer.GetComponent<PlayerController>();
			p.PlayerName = name;
			p.name = name;

			newPlayer.networkView.viewID = id;
			GameManager.WriteMessage("Spawned as " + pos.ToString());
		}
	}

	[RPC]
	void RespawnPlayer(NetworkPlayer player, NetworkViewID id)
	{
		if(Network.isServer)
		{
			DeletePlayerState(player);

			Network.Destroy (Players [player]);

			SpawnPlayer (id, GetSpawnLocation (), PlayerNames[player]);

			//Give player default weapon.
			Players[player].GetComponent<Inventory>().AddToInventory("WoodStick",1);
		}
	}

	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		KillServer ();

		Menu.CurrentScreen = ScreenType.Error;
		Menu.ScreenMessage = "You have been disconnected from the server.";

		Application.LoadLevel (LevelLoader.LEVEL_MENU);
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		SavePlayerState (player);
		GameManager.NetMessage(PlayerNames[player] + " left the game");
		Logger.Write ("Player " + player.ToString () + " has left or been disconnected.");
		Network.RemoveRPCs (player);
		Network.DestroyPlayerObjects (player);
		Players.Remove (player);
	}

	public static void Disconnect()
	{
		if(Network.isClient)
		{
			Network.Disconnect(200);
			Application.LoadLevel(LevelLoader.LEVEL_MENU);
		}
	}
	
	public static void RemoveNetworkBuffer(NetworkViewID viewID)
	{
		Instance.RemoveNetworkBufferedRPC(viewID);
	}

	public static void Respawn()
	{
		Instance.networkView.RPC ("RespawnPlayer", RPCMode.Server, Network.player, Network.AllocateViewID());
	}

	[RPC]
	void RemoveNetworkBufferedRPC(NetworkViewID viewId)
	{
		if(Network.isServer)
		{
			Network.RemoveRPCs(viewId);
		}
		else
		{
			networkView.RPC ("RemoveNetworkBufferedRPC", RPCMode.Server, viewId);
		}
	}


	//Server related stuff
	private Vector3 GetSpawnLocation()
	{
		GameObject[] spawns = GameObject.FindGameObjectsWithTag("PlayerSpawn");

		Debug.Log ("Spawn locs " + spawns.Length);

		Vector3 spawnPos = new Vector3(0f,0f,0f);

		if(spawns.Length > 0)
		{
			int spawn = Random.Range(0,spawns.Length);
			spawnPos = spawns[spawn].transform.position;
		}

		return spawnPos;
	}

	//Player states

	private string GetStatePath()
	{
		string path = Logger.Path() + "/States";

		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);

		return path;
	}

	private void DeletePlayerState(NetworkPlayer player)
	{
		string file = GetStatePath() + "/" + PlayerNames[player] + ".pla";
		if(File.Exists(file))
			File.Delete(file);
	}

	private void SavePlayerState(NetworkPlayer player)
	{
		string file = GetStatePath() + "/" + PlayerNames[player] + ".pla";
		if(File.Exists(file))
			File.Delete(file);


		GameObject p = Players [player];

		StreamWriter writer = new StreamWriter (file);
		writer.WriteLine(p.transform.position.x);
		writer.WriteLine(p.transform.position.y);
		writer.WriteLine(p.GetComponent<HealthSystem>().Health);


		writer.Close ();


	}

	private bool LoadPlayerState(NetworkViewID id, NetworkPlayer player)
	{
		string file = GetStatePath() + "/" + PlayerNames[player] + ".pla";

		float x;
		float y;
		int health;

		if(File.Exists(file))
		{
			StreamReader reader = new StreamReader(file);

			bool created = false;

			try
			{
				x = float.Parse(reader.ReadLine());
				y = float.Parse(reader.ReadLine());
				health = int.Parse(reader.ReadLine());

				Vector3 pos = new Vector3(x,y,0);

				SpawnPlayer(id, pos, PlayerNames[player]);

				Players[player].GetComponent<HealthSystem>().Health = health;

				created = true;
			}
			catch( System.Exception e)
			{
				created = false;
			}
			reader.Close();

			return created;
		}

		return false;
	}
}

public class ServerSettings
{
	public string ServerName;
	public int Port;
	public int MaxPlayers;
	public bool Dedicated;
	public bool LAN = false;
	public bool UseNAT = true;

	public ServerSettings()
	{
	}

	public static ServerSettings Default
	{
		get {
			ServerSettings val = new ServerSettings ();

			val.ServerName = "Server";
			val.Port = 12777;
			val.MaxPlayers = 8;
			val.Dedicated = false;
			val.LAN = false;

			return val;
		}
	}
}
