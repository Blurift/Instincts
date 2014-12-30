using UnityEngine;
using System;
using System.Collections;

[Serializable]
public class AIAction{

	public string ActionName = "AIACTION";
	public GameObject ParentAIGameObject;
	public AI ParentAI;
	public bool transparent = true;

	public virtual void Initialize(AI parent)
	{
		ParentAI = parent;
		ParentAIGameObject = parent.gameObject;

		if(ParentAIGameObject == null)
		{
			Debug.Log("Error no gameObject");
			End ();
		}
	}

	public virtual void Update()
	{
	}

	public void End()
	{

		ParentAI.RemoveAction (this); 
	}
}
