using UnityEngine;
using System.Collections;

public class ItemDrop : MonoBehaviour {

	public static Item ItemToPickUp = null;

	public float DespawnTime = 0;
	public Item item;

	private bool ready = false;

	private float unitWidth = 0;
	private float unitHeight = 0;

	private bool showHover = false;

	private string hoverText = "";

	// Use this for initialization
	void Start () {

		//item = new Item (); //(Item)GetComponent(typeof(Item));

		if (item == null)
			return;

		Texture2D tex = item.Icon;

		if(tex != null)
		{
			float dispWidth = tex.width;
			if(dispWidth < 32)
				dispWidth = 32;
			((SpriteRenderer)GetComponent(typeof(SpriteRenderer))).sprite =
				Sprite.Create(item.Icon,new Rect(0,0,item.Icon.width,item.Icon.height),new Vector2(0.5f, 0.5f),dispWidth);

			unitWidth = tex.width/32f;
			unitHeight = tex.height/32f;

			unitWidth = 1;
			unitHeight = 1;

			hoverText = item.Name;

			ready = true;


		}
	}
	
	// Update is called once per frame
	void Update () {
		if ((!Network.isServer && !Network.isClient) || GameManager.ControllingInventory == false)
			return;


		if(ready && !HUD.Instance.HUDFocus)
		{
			//Check distance to player;
			if(Helper.DistanceFloatFromTarget(gameObject.transform.position, Camera.main.transform.position) < 2)
			{
				ItemToPickUp = item;
			}
		}

		if (Time.time > DespawnTime && Network.isServer && !item.IsOwned)
		{
			RemoveFromWorld();
			Network.Destroy(gameObject);
		}
	}

	public void RemoveFromWorld()
	{
		RemoveNetworkBufferedRPC(networkView.viewID);

		Destroy (this);
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
}
