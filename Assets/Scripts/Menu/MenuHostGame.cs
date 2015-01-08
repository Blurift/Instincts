using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Blurift.UI;

public class MenuHostGame : MonoBehaviour {

    public ToggleList ToggleList;
    public InputField ServerName;
    public InputField Port;
    public Slider MaxPlayers;

	// Use this for initialization
	void Start () {
	    //Get all world names
        //TODO


	}
	
	// Update is called once per frame
	void Update () {

    }

    #region Buttons

    public void Create()
    {
        ToggleList.AddItem("Nothing yet " + (ToggleList.GetCurrentIndex() + 1));
    }

    public void Delete()
    {
        int index = ToggleList.GetCurrentIndex();

        ToggleList.DeleteCurrentItem();
    }

    #endregion
}
