using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Blurift.UI;

[AddComponentMenu("Menu/Screen Options")]
public class MenuOptions : MonoBehaviour {

    #region variables

    public Slider MusicSlider;
    public Slider SoundSlider;

    public Text ResolutionText;
    public RectTransform ResolutionPanel;
    public GameObject DropdownPrefab;
    public Toggle FullscreenToggle;

    private Resolution[] resList;

    #endregion

	// Use this for initialization
	void Start () {
        //Volume checks
        if (PlayerPrefs.HasKey("VolumeSound"))
        {
            GameManager.SoundLevel = PlayerPrefs.GetFloat("VolumeSound");
            SoundSlider.value = GameManager.SoundLevel;
        }

        if (PlayerPrefs.HasKey("VolumeMusic"))
        {
            GameManager.MusicLevel = PlayerPrefs.GetFloat("VolumeMusic");
            MusicSlider.value = GameManager.MusicLevel;
        }

        //Set out all the resolution options.
        ResolutionText.text = Screen.width + " x " + Screen.height;
        resList = Screen.resolutions;

        for (int i = 0; i < resList.Length; i++ )
        {
            GameObject dd = Instantiate(DropdownPrefab) as GameObject;

            Dropdown dropdown = dd.GetComponent<Dropdown>();

            dropdown.Text.text = resList[i].width + " x " + resList[i].height;
            dropdown.OnPress += ResolotionChanged;
            dropdown.Index = i;

            dd.transform.SetParent(ResolutionPanel.transform);
        }

        FullscreenToggle.isOn = Screen.fullScreen;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    #region Sound Methods

    public void MusicValue(float value)
    {
        PlayerPrefs.SetFloat("VolumeMusic", value);
        GameManager.MusicLevel = value;
    }

    public void SoundValue(float value)
    {
        PlayerPrefs.SetFloat("VolumeSound", value);
        GameManager.SoundLevel = value;
    }

    #endregion

    #region Resolution

    public void ResolotionChanged(int index)
    {
        Debug.Log("UI(Resolution Changed): Index = " + index);
        ResolutionText.text = resList[index].width + " x " + resList[index].height;
        Screen.SetResolution(resList[index].width, resList[index].height, Screen.fullScreen);
    }

    public void Fullscreen(bool value)
    {
        Screen.fullScreen = value;
    }

    #endregion
}
