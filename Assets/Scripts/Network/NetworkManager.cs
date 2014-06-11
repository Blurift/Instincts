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
			Vector3 move = ((Input.GetAxis ("Vertical") * transform.up) + (Input.GetAxis ("Horizontal") * transform.right)).normalized;
			
			move*= 15*Time.deltaTime;
			Camera.main.transform.position += move;
			

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

		string serverName = "Server";
		int maxPlayers = 8;
		int port = 7000;
		bool setup = false;
		bool nat = true;

		StreamReader reader = new StreamReader (Instance.serverSysFile);
		try
		{
			serverName = reader.ReadLine ();
			maxPlayers = int.Parse(reader.ReadLine());
			port = int.Parse(reader.ReadLine());
			if(!reader.ReadLine().Equals("True"))
				nat = false;
			setup = true;
		}
		catch (System.Exception)
		{
			GameManager.WriteMessage("Couldnt read server profile");
			return;
		}
		reader.Close ();

		//Fix some settings;
		if(maxPlayers > 16)
			maxPlayers = 16;

		GameManager.WriteMessage ("Starting server with " + maxPlayers + " max players on port " + port);
		Network.InitializeServer( maxPlayers, port, nat);
		MasterServer.dedicatedServer = true;
		MasterServer.RegisterHost (typeName, serverName);
		Camera.main.gameObject.AddComponent (typeof(AudioListener));

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

			GameObject itemGO = ItemManager.CraftItem("WoodStick", player);
			Players[player].networkView.RPC("ItemAttach", player, itemGO.networkView.viewID);

			//Loop through all items
			int itemCount = 0;
			foreach(GameObject go in GameObject.FindGameObjectsWithTag("Item"))
			{
				Item item = (Item)go.GetComponent(typeof(Item));

				if(!item.IsOwned)
				{
					ItemManager.Instance.networkView.RPC("SpawnItemRPC", player, item.name, go.transform.position, go.networkView.viewID);
					item.VerifyItemWithServer(player);
					itemCount++;
				}
			}
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

			GameObject itemGO = ItemManager.CraftItem("WoodStick", player);
			Players[player].networkView.RPC("ItemAttach", player, itemGO.networkView.viewID);
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
