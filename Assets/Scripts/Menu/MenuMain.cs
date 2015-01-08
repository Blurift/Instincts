using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("Menu/Main")]
public class MenuMain : MonoBehaviour
{

    

    #region Screens

    public RectTransform ScreenHost;
    public RectTransform ScreenJoin;
    public RectTransform ScreenCreateProfile;
    public RectTransform ScreenSelectProfile;
    public RectTransform ScreenOptions;
    public RectTransform ScreenEdit;

    #endregion

    // Use this for initialization
	void Start () {
	



        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    #region Options

    private void SwitchReset()
    {
        ScreenHost.gameObject.SetActive(false);
        ScreenJoin.gameObject.SetActive(false);
        ScreenOptions.gameObject.SetActive(false);
        ScreenCreateProfile.gameObject.SetActive(false);
        ScreenSelectProfile.gameObject.SetActive(false);
        ScreenEdit.gameObject.SetActive(false);
    }

    public void SwitchOptions()
    {
        SwitchReset();
        ScreenOptions.gameObject.SetActive(true);
    }

    public void SwitchHost()
    {
        SwitchReset();
        ScreenHost.gameObject.SetActive(true);
    }
    public void SwitchJoin()
    {
        SwitchReset();
        ScreenJoin.gameObject.SetActive(true);
    }
    public void SwitchSelect()
    {
        SwitchReset();
        ScreenSelectProfile.gameObject.SetActive(true);
    }
    public void SwitchCreate()
    {
        SwitchReset();
        ScreenCreateProfile.gameObject.SetActive(true);
    }
    public void SwitchEdit()
    {
        SwitchReset();
        ScreenEdit.gameObject.SetActive(true);
    }
    public void Exit()
    {
        Application.Quit();
    }
    
    #endregion
}
