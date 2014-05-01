using UnityEngine;
using System.Collections;

public class NetViewWork : MonoBehaviour {

	[RPC]
	public void ResetNetworkView()
	{
		if(networkView.isMine)
		{
			NetworkViewID id = networkView.viewID;
			Component c = networkView.observed;
			NetworkStateSynchronization n = networkView.stateSynchronization;

			DestroyImmediate(networkView);

			NetworkView net = (NetworkView)gameObject.AddComponent(typeof(NetworkView));

			net.viewID = id;
			net.observed = c;
			net.stateSynchronization = n;
		}
	}
}
