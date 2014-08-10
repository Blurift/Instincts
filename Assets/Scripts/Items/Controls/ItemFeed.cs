﻿using UnityEngine;
using System.Collections;
using System;


[Serializable]
public class ItemFeed : ItemBehaviour {

	[SerializeField]
	public int Hunger = 1;
	
	public override void UseItem (PlayerController controller, Item item)
	{
		HealthSystem health = (HealthSystem)controller.GetComponent (typeof(HealthSystem));
		health.networkView.RPC ("ChangeHunger", RPCMode.Server, health.Hunger + Hunger);
		item.TakeFromStack(1);
	}

	public override string DescValue ()
	{
		return "Hunger: " + Hunger.ToString();
	}
}
