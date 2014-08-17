/* File: Music Manager - KMusic
 * Author: Keirron Stach
 * Version: 0.1
 * Created 25/03/2014
 * Last Edit: 25/03/2014
*/

using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour {

	public AudioSource player;
	public AudioClip[] tracks;
	private string[] trackNames;
	private int index = 0;
	private bool randomPlay = false;

	private static MusicManager Instance;

	void Awake()
	{
		if (Instance != null)
				Destroy (gameObject);
		else
				Instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		return;
		if(tracks.Length > 0 && !Network.isServer)
		{
			player.clip = tracks[0];
			player.Play ();
		}
	}
	
	// Update is called once per frame
	void Update () {
		return;
		if (!player.isPlaying && !Network.isServer)
		{
			index ++;
			if(index >= tracks.Length)
				index = 0;

			player.clip = tracks[index];
			player.Play();
		}

		if(Network.isServer && player.isPlaying)
		{
			player.Stop();
		}
	}

	static void Pause()
	{
		if(Instance.player.isPlaying)
		{
			Instance.player.Pause();
		}
	}

	static void Play()
	{
		if(!Instance.player.isPlaying)
		{
			Instance.player.Play();
		}
	}
}

[System.Serializable]
public class TaggedMusicTrack
{
	[SerializeField]
	private string name;
	[SerializeField]
	private string[] tags;
	[SerializeField]
	private AudioClip musicClip;
}
