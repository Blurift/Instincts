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
	private const string typeName = Game.TypeName;

	public static ServerSettings Settings;
	private string serverSysFile = "";

	public static bool Server = false;

	//PrefabInfo
	public GameObject PlayerPrefab;
	public GameObject[] PlayerPrefabList;

	//Players
	public Dictionary<NetworkPlayer, NetworkPlayerState> PlayerStates;

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

    /// <summary>
    /// Return integer value of the amount of players.
    /// </summary>
    public static int PlayerCount()
    {
        return Instance.PlayerStates.Count;
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
        PlayerStates = new Dictionary<NetworkPlayer, NetworkPlayerState>();

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
            //Dedicated testing.
			/*Vector3 move = ((Input.GetAxis ("Vertical") * transform.up) + (Input.GetAxis ("Horizontal") * transform.right)).normalized;
			
			move*= 15*Time.deltaTime;
			Camera.main.transform.position += move;
			*/

			if(Time.time - stateSyncTime > stateLastSync)
			{
				SaveState ();
				stateLastSync = Time.time;
			}

            SetPlayersPing();
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

		Instance.PlayerStates = new Dictionary<NetworkPlayer, NetworkPlayerState> ();

		ServerSettings settings = Settings;
		
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
			GameManager.WriteMessage ("Starting server with " + (settings.MaxPlayers) + " max players on port " + settings.Port);
	}

	public static void JoinServer(string user, string pass)
	{
		Instance.networkView.RPC ("LoginToServer", RPCMode.Server, user, pass, Network.player, Network.AllocateViewID (), Menu.HairColor, Menu.HairStyle, Menu.TopColor);
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
        LoginToServer(Menu.MainProfile.Name, Menu.MainProfile.UID, Network.player, Network.AllocateViewID(), Menu.HairColor, Menu.HairStyle, Menu.TopColor);
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
    void LoginToServer(string user, string uid, NetworkPlayer player, NetworkViewID id, Vector3 hair, int hairStyle, Vector3 top)
	{
		if(Network.isServer)
		{
            //user = id.ToString() + ": " + user; //Testing to identify against self
			GameManager.NetMessage(user + " joined the game");
			//Logger.Write("Player " + player.ToString() + " : " + user + " - " +player.ipAddress + " trying to log in");

            networkView.RPC("RegisterPlayer", RPCMode.Others, player, user);

			//Loop through all connected players and send details.
			//networkView.RPC("RandomMessage", player, "Sending players through");
			foreach(KeyValuePair<NetworkPlayer, NetworkPlayerState> pair in PlayerStates)
			{
                //Check the server is not sending information to itself.
                if(player != Network.player)
                    networkView.RPC("RegisterPlayer", player, pair.Key, pair.Value.Name);
				if(pair.Key != player)
				{
                    Color hC = pair.Value.HairColor;
                    Vector3 hV = new Vector3(hC.r, hC.g, hC.b);

                    Color tC = pair.Value.TopColor;
                    Vector3 tV = new Vector3(tC.r, tC.g, tC.b);

					Debug.Log("Sending player data : " + pair.Value.ToString() + " to " + player.ToString() + " : " + id.ToString());
                    networkView.RPC("SpawnPlayer", player, pair.Value.GameObject.networkView.viewID, pair.Value.GameObject.transform.position, PlayerStates[pair.Key].Name,
                        hV, pair.Value.HairStyle, tV);


					pair.Value.GameObject.networkView.SetScope(player, true);
					//pair.Value.GameObject.networkView.RPC("ResetNetworkView", pair.Key);
				}
			}

            //Set Up player state;
			NetworkPlayerState playerState = new NetworkPlayerState();
			playerState.Name =user;
			playerState.UID = uid;
            playerState.HairColor = new Color(hair.x,hair.y,hair.z);
            playerState.TopColor = new Color(top.x,top.y,top.z);
            playerState.HairStyle = hairStyle;

			PlayerStates[player] = playerState;


            /* Make the player
             * If the player has played before reload there last state.
             */
			PlayerController.PlayerState state = LoadPlayerState(id,player);
			if(state == null)
			{
				SpawnPlayer(id, GetSpawnLocation(), user, hair,hairStyle,top);
				PlayerStates[player].GameObject.GetComponent<Inventory>().AddToInventory("WoodStick",1,-1,"E");
				PlayerStates[player].GameObject.GetComponent<Inventory>().AddToInventory("Cloth",1);
			}
			else
			{
				SpawnPlayer(id, state.Position, user, hair, hairStyle, top);
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

    #region Player State system

    [RPC]
    void RegisterPlayer(NetworkPlayer player, string name)
    {
        NetworkPlayerState state = new NetworkPlayerState();
        state.Name = name;

        PlayerStates.Add(player, state);
    }

    [RPC]
    void DeregisterPlayer(NetworkPlayer player)
    {
        if (PlayerStates.ContainsKey(player))
            PlayerStates.Remove(player);
    }

    float pingNext = 0;

    void SetPlayersPing()
    {
        if (pingNext > Time.time)
            return;

        foreach(KeyValuePair<NetworkPlayer, NetworkPlayerState> p in PlayerStates)
        {
            p.Value.Ping = Network.GetAveragePing(p.Key);
            networkView.RPC("SetPlayersPingRPC", RPCMode.Others, p.Key, p.Value.Ping);
        }

        pingNext = Time.time + 1;
    }

    [RPC]
    void SetPlayersPingRPC(NetworkPlayer player, int ping)
    {
        if (PlayerStates.ContainsKey(player))
            PlayerStates[player].Ping = ping;
    }

    #endregion

    [RPC]
    void SpawnPlayer(NetworkViewID id, Vector3 pos, string name, Vector3 hair, int hairStyle, Vector3 top)
	{
		Debug.Log ("Spawning Player " + pos.ToString());

		


        GameObject newPlayer = (GameObject)GameObject.Instantiate(PlayerPrefab, pos, Quaternion.identity);
        newPlayer.networkView.viewID = id;
        newPlayer.name = name;

        PlayerController p = newPlayer.GetComponent<PlayerController>();
        p.PlayerName = name;
        p.name = name;
        p.Hair.color = new Color(hair.x, hair.y, hair.z);
        p.Hair.sprite = Menu.HairStyles[hairStyle];
        p.SetTopColor(new Color(top.x, top.y, top.z));

        if (Network.isServer)
        {
            PlayerStates[id.owner].GameObject = newPlayer;
            networkView.RPC("SpawnPlayer", RPCMode.Others, id, pos, name, hair, hairStyle, top);
        }

        newPlayer.SetActive(true);

	}

    public static void PlayerDied(NetworkPlayer player)
    {
        if (Network.isServer)
            Instance.PlayerDiedRPC(player);
    }

    [RPC]
    void PlayerDiedRPC(NetworkPlayer player)
    {
        GameManager.ServerMessage(PlayerStates[player].Name + " has been killed");
    }

    public static void Respawn()
    {
        if (Network.isServer)
            Instance.RespawnPlayer(Network.player);
        else
            Instance.networkView.RPC("RespawnPlayer", RPCMode.Server, Network.player);
    }

	[RPC]
	void RespawnPlayer(NetworkPlayer player)
	{
		if(Network.isServer)
		{
			

            Vector3 l = GetSpawnLocation();
            PlayerController p = PlayerStates[player].GameObject.GetComponent<PlayerController>();

            Debug.Log("Respawning player " + p.name);

            p.PlayerRespawn(l);

			//Give player default weapon.
			PlayerStates[player].GameObject.GetComponent<Inventory>().AddToInventory("WoodStick",1,-1,"E");
			PlayerStates[player].GameObject.GetComponent<Inventory>().AddToInventory("Cloth",1);

			
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
        networkView.RPC("DeregisterPlayer", RPCMode.Others, player);
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

	public class NetworkPlayerState
	{
		public string Name;
		public GameObject GameObject;
		public string UID;
        public Color HairColor;
        public Color TopColor;
        public int HairStyle;
		public bool Ready = false;
        public int Ping = 0;
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
