/* Name: Test
 * Desc: Used to test other game objects without needing to go through the whole game
 * Author: Keirron Stach
 * Version: 0.1
 * Created: 20/04/2014
 * Edited: 20/04/2014
 */ 

using UnityEngine;
using System.Collections;

public class TestInit : MonoBehaviour {

	public GameObject ObjectToTest;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(ObjectToTest != null)
		{
			if(Input.GetKeyDown("j"))
			{
				GameObject.Instantiate(ObjectToTest, this.transform.position, Quaternion.identity);
			}
		}
	}
}
