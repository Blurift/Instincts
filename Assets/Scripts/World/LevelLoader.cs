/* Level Loader
 * Author: Keirron Stach
 * Created: 14/08/2014
 * Edited: 14/08/2014
 */

using UnityEngine;
using System.Collections;

public class LevelLoader : MonoBehaviour {

	public const int LEVEL_INTRO = 0;
	public const int LEVEL_MENU = 1;
	public const int LEVEL_GAME = 2;

	public static LevelLoader Instance
	{
		get { return instance; }
	}
	private static LevelLoader instance;

	//Textures
	public Texture2D loadScreen;

	//Info
	public bool[] needsLoad;
	private int currenLevel = -1;
	public LevelAttributes[] Levels;

	// Use this for initialization
	void Start () {
		if(instance != null)
			Destroy(instance);

		instance = this;
		DontDestroyOnLoad (this);
	}

	[RPC]
	public void LoadLevel(int level)
	{
		if (needsLoad [level])
			currenLevel = level;
		else
			currenLevel = -1;

		Network.SetSendingEnabled (0, false);
		Network.isMessageQueueRunning = false;
		Network.SetLevelPrefix(level);
		Application.LoadLevel(level);
		Network.isMessageQueueRunning = true;
		Network.SetSendingEnabled (0, true);

		foreach (GameObject g in FindObjectsOfType(typeof(GameObject)))
						g.SendMessage ("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);
	}

	public class LevelAttributes
	{
		public string Name = "Level";
		public bool LoadFirst = false;
	}
}
