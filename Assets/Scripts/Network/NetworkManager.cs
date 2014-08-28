using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Blurift;

public class NetworkManager : MonoBehaviour {

	public static NetworkManager Instance = null;

	//Static info
	private const string typeName = Menu.TypeName;

	public static ServerSettings Settings;
	private string serverSysFile = "";

	public static bool Server = false;

	//PrefabInfo
	public GameObject PlayerPrefab;
	public GameObject[] PlayerPrefabList;

	//Players
	//private Dictionary<NetworkPlayer,GameObject> Players;
	//private Dictionary<NetworkPlayer, string> PlayerNames;
	//private Dictionary<NetworkPlayer, string> PlayerUIDs;
	private Dictionary<NetworkPlayer, NetworkPlayerState> PlayerStates;

	//StateSave
	private float stateLastSync = 0;
	private float stateSyncTime = 30;

	public static void SendRPC(NetworkView view, string name, params object[] values)
	{
		SendRPC(view, null, name, values);
	}

	public static void SendRPC(NetworkView view, NetworkPlayer except, string name, params object[] values)
	{
		foreach(KeyValuePair<NetworkPlayer, NetworkPlayerState> pair in Instance.PlayerStates)
		{
			if(pair.Value.GameObject.networkView.owner != except && pair.Value.Ready && pair.Value.GameObject.networkView.owner != Network.player)
			{
				view.RPC(name, pair.Key, values);
			}
		}
	}

	public static GameObject GetPlayer(NetworkPlayer player)
	{
		foreach(KeyValuePair<NetworkPlayer, NetworkPlayerState> pair in Instance.PlayerStates)
		{
			if(pair.Key == player)
				return pair.Value.GameObject;
		}	
		return null;
	}

	public static string GetPlayerName(NetworkPlayer player)
	{
		foreach(KeyValuePair<NetworkPlayer, NetworkPlayerState> pair in Instance.PlayerStates)
		{
			if(pair.Key == player)
				return pair.Value.Name;
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
		}
		else
			JoinServer(Menu.MainProfile.Name,Menu.MainProfile.UID);
	}
	
	// Update is called once per frame
	void Update () {
		if(Network.isServer)
		{
			/*Vector3 move = ((Input.GetAxis ("Vertical") * transform.up) + (Input.GetAxis ("Horizontal") * transform.right)).normalized;
			
			move*= 15*Time.deltaTime;
			Camera.main.transform.position += move;
			*/

			if(Time.time - stateSyncTime > stateLastSync)
			{
				SaveState ();
				stateLastSync = Time.time;
			}
		}
	}

	private void SaveState()
	{
		foreach(KeyValuePair<NetworkPlayer,NetworkPlayerState> pl in PlayerStates)
		{
			SavePlayerState(pl.Key);
		}
	}

	//Server Methods
	public static void StartServer()
	{
		Debug.Log ("Server starting");
		GameManager.WriteMessage("Attempting to start server");

		//Instance.Players = new Dictionary<NetworkPlayer, GameObject> ();
		//Instance.PlayerNames = new Dictionary<NetworkPlayer, string> ();
		//Instance.PlayerUIDs = new Dictionary<NetworkPlayer, string> ();
		Instance.PlayerStates = new Dictionary<NetworkPlayer, NetworkPlayerState> ();

		ServerSettings settings = Settings;
		bool setup = false;
		
		if(settings == null)
		{
			settings = ServerSettings.Default;
		}

		//Fix some settings;
		if(settings.MaxPlayers > 16)
			settings.MaxPlayers = 16;

		if(!settings.Dedicated)
		{
			settings.MaxPlayers--;
		}
		else
			AudioListener.volume = 0;
		//	Camera.main.gameObject.AddComponent (typeof(AudioListener));

		MasterServer.dedicatedServer = settings.Dedicated;
		Network.InitializeServer(settings.MaxPlayers, settings.MaxPlayers, settings.UseNAT);

		if(!settings.LAN)
		{
			MasterServer.RegisterHost (typeName, settings.ServerName);
		}

		if(settings.Dedicated)
			GameManager.WriteMessage ("Starting server with " + settings.MaxPlayers + " max players on port " + settings.Port);
		else
			GameManager.WriteMessage ("Starting server with " + (settings.MaxPlayers+1) + " max players on port " + settings.Port);
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
		//TODO
		//Check if dedicated server
		LoginToServer (Menu.MainProfile.Name, Menu.MainProfile.UID, Network.player, Network.AllocateViewID ());
	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		GameManager.ServerMessage ("Player " + player.ToString () + " connecting");
        GameEventManager.PushEvent(this, "PlayerJoined", new GameEvent());
	}

	[RPC]
	void RandomMessage(string mes)
	{
		Debug.Log (mes);
	}

	[RPC]
	void LoginToServer(string user, string uid, NetworkPlayer player, NetworkViewID id)
	{
		if(Network.isServer)
		{
			GameManager.NetMessage(user + " joined the game");
			//Logger.Write("Player " + player.ToString() + " : " + user + " - " +player.ipAddress + " trying to log in");
			
			//Loop through all connected players and send details.
			//networkView.RPC("RandomMessage", player, "Sending players through");
			foreach(KeyValuePair<NetworkPlayer, NetworkPlayerState> pair in PlayerStates)
			{
				if(pair.Key != player)
				{
					Debug.Log("Sending player data : " + pair.Value.ToString() + " to " + player.ToString() + " : " + id.ToString());
                    networkView.RPC("SpawnPlayer", player, pair.Value.GameObject.networkView.viewID, pair.Value.GameObject.transform.position, PlayerStates[pair.Key].Name);
					//networkView.RPC("RandomMessage", player, pair.Value.networkView.viewID.ToString() + "being sent through");
					pair.Value.GameObject.networkView.SetScope(player, true);
					pair.Value.GameObject.networkView.RPC("ResetNetworkView", pair.Key);
				}
			}

			NetworkPlayerState playerState = new NetworkPlayerState();
			playerState.Name =user;
			playerState.UID = uid;

			PlayerStates[player] = playerState;


            /* Make the player
             * If the player has played before reload there last state.
             */
			PlayerController.PlayerState state = LoadPlayerState(id,player);
			if(state == null)
			{
				SpawnPlayer(id, GetSpawnLocation(), user);
				PlayerStates[player].GameObject.GetComponent<Inventory>().AddToInventory("WoodStick",1,-1,"E");
				PlayerStates[player].GameObject.GetComponent<Inventory>().AddToInventory("Cloth",1);
			}
			else
			{
				SpawnPlayer(id, state.Position, user);
				PlayerStates[player].GameObject.GetComponent<PlayerController>().SetPlayerState(state);
			}

			playerState.Ready = true;

			//Loop through all items
			ItemManager.InitializeDropsForPlayer(player);
			//GameManager.SendMessage("Sent " + itemCount + " itemSpawns to " + PlayerNames[player], true);

			//Loop through all AI
			//networkView.RPC("RandomMessage", player, "Sending AI through");
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
				//Respawn player
				GameObject newPlayer = (GameObject)GameObject.Instantiate(PlayerPrefab, pos, Quaternion.identity);
				
				newPlayer.networkView.viewID = id;
				newPlayer.name = name;
				PlayerController p = newPlayer.GetComponent<PlayerController>();
				p.PlayerName = name;
				
				//Players[id.owner] = newPlayer;
				PlayerStates[id.owner].GameObject = newPlayer;
				
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
			//GameManager.WriteMessage("Spawned as " + pos.ToString());
		}


	}

	[RPC]
	void RespawnPlayer(NetworkPlayer player, NetworkViewID id)
	{
		if(Network.isServer)
		{
			Debug.Log("Respawning player");
			DeletePlayerState(player);

			Network.Destroy (PlayerStates[player].GameObject);

			SpawnPlayer (id, GetSpawnLocation (), PlayerStates[player].Name);

			//Give player default weapon.
			//Players[player].GetComponent<Inventory>().AddToInventory("WoodStick",1);
			PlayerStates[player].GameObject.GetComponent<Inventory>().AddToInventory("WoodStick",1,-1,"E");
			PlayerStates[player].GameObject.GetComponent<Inventory>().AddToInventory("Cloth",1);

			GameManager.ServerMessage(PlayerStates[player].Name + " has been killed");
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
		GameManager.ServerMessage(PlayerStates[player].Name + " left the game");
		//Logger.Write ("Player " + player.ToString () + " has left or been disconnected.");
		Network.RemoveRPCs (player);
		Network.DestroyPlayerObjects (player);
		//Players.Remove (player);
		PlayerStates.Remove(player);

        GameEventManager.PushEvent(this, "PlayerLeft", new GameEvent());
	}

	public static void Disconnect()
	{
		if(Network.isServer)
		{
			Instance.SaveState();
			for (int i = 0; i < Network.connections.Length; i++) {
				Network.CloseConnection(Network.connections[i], true);
			}
		}
		Network.Disconnect(200);
		Application.LoadLevel(LevelLoader.LEVEL_MENU);
	}
	
	public static void RemoveNetworkBuffer(NetworkViewID viewID)
	{
		Instance.RemoveNetworkBufferedRPC(viewID);
	}

	public static void Respawn()
	{
		if(Network.isServer)
			Instance.RespawnPlayer(Network.player, Network.AllocateViewID());
		else
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
		string path = GameManager.ServerPlayerPath() + "/States";

		if(!Directory.Exists(path))
			Directory.CreateDirectory(path);

		return path;
	}

	private void DeletePlayerState(NetworkPlayer player)
	{
		string file = GetStatePath() + "/" + PlayerStates[player].UID + ".pla";
		if(File.Exists(file))
			File.Delete(file);
	}

	private void SavePlayerState(NetworkPlayer player)
	{
        string file = GetStatePath() + "/" + PlayerStates[player].UID + ".pla";
		if(File.Exists(file))
			File.Delete(file);


		GameObject p = PlayerStates[player].GameObject;
		PlayerController pc = p.GetComponent<PlayerController> ();

		StreamWriter writer = new StreamWriter (file);
		XmlSerializer serializer = new XmlSerializer (typeof(PlayerController.PlayerState));
		serializer.Serialize (writer, pc.GetPlayerState ());
		writer.Close ();


	}

	private PlayerController.PlayerState LoadPlayerState(NetworkViewID id, NetworkPlayer player)
	{
        string file = GetStatePath() + "/" + PlayerStates[player].UID + ".pla";

		PlayerController.PlayerState state;



		if(File.Exists(file))
		{
			StreamReader reader = new StreamReader(file);
			XmlSerializer serializer = new XmlSerializer(typeof(PlayerController.PlayerState));
			state = (PlayerController.PlayerState)serializer.Deserialize(reader);
			reader.Close();

			return state;
		}

		return null;
	}

	private class NetworkPlayerState
	{
		public string Name;
		public GameObject GameObject;
		public string UID;
		public bool Ready = false;
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
			val.Port = 7868;
			val.MaxPlayers = 8;
			val.Dedicated = false;
			val.LAN = false;

			return val;
		}
	}
}
