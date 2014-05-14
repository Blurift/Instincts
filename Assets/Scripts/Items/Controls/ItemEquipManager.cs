using UnityEngine;
using System.Collections;

public class ItemEquipManager : MonoBehaviour {

	public Transform EquipPosition;

	void Start()
	{
		if(!networkView.isMine)
		{

			GameObject[] go = GameObject.FindGameObjectsWithTag("Player");

			for (int i = 0; i < go.Length; i++)
			{
				if(networkView.owner == go[i].networkView.owner)
				{
					EquipPosition = go[i].GetComponent<PlayerController>().EquipPosition;
				}
			}
		}
	}

	void Update()
	{
		if(EquipPosition != null)
		{
			transform.position = new Vector3(EquipPosition.position.x,EquipPosition.position.y, transform.position.z);
			transform.rotation = EquipPosition.rotation;
		}
	}

	public void Destroy()
	{
		RemoveNetworkBufferedRPC(networkView.viewID);
		Network.Destroy (gameObject);
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
