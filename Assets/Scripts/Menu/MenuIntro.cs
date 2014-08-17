using UnityEngine;
using System.Collections;

public class MenuIntro : MonoBehaviour {

	public GUISkin MenuSkin;
	public Texture2D Filler;

	public Texture2D BluriftLogo;

	private float startTime = 0;
	public float fadeTime = 2;

	private Vector2 termsScroll = Vector2.zero;

	private string title = "<b>INSTINCTS</b>\n" +
				"VERSION: " + Menu.VERSION;

	private string terms = 
		"<b>INSTINCTS TERMS AND CONDITIONS</b>\n\n\n" +
			"By clicking <b>Accept</b> below you acknowledge that you have read and agree to the following conditions. \n\n\n" +
			"(1) Instincts is in a PRE-ALPHA state.\n" +
			"It is not a complete game and should not be considered so.\n\n\n" +
			"(2) This means while playing you will encounter performance issues, bugs, incompatibility issues and many other unforeseen problems.\n\n\n" +
			"(3)If you come accross you can report it too support@blurift.com, we also have a forum at www.blutift.com/forum that you can post bugs and support issues too.\n\n\n" +
			"Thank you for playing!"
			;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI()
	{
		GUI.skin = MenuSkin;
		Menu.SetFontSize (GUI.skin);

		/****DRAW Black BACKGROUND */
		GUI.color = Color.black;
		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), Filler);
		GUI.color = Color.white;

		GUIStyle termsStyle = GUI.skin.customStyles [1];
		termsStyle.fontSize = 16;
		GUIStyle titleStyle = new GUIStyle (termsStyle);
		titleStyle.fontSize = 35;
		titleStyle.alignment = TextAnchor.UpperCenter;


		float termsWidth = Screen.width * 0.4f;
		float termsHeight = Screen.height * 0.6f;

		float termsButton = termsWidth * 0.35f;

		float textHeight = termsStyle.CalcHeight (new GUIContent (terms), termsWidth - 12);
		float titleHeight = titleStyle.CalcHeight (new GUIContent(title), termsWidth - 12);


		
		Rect innerTitle = new Rect(0,0, termsWidth -12, titleHeight);
		Rect innerTerms = new Rect (0, titleHeight+20, termsWidth - 12, textHeight);

		float innerHeight = textHeight + titleHeight + 20;
		if (innerHeight < termsHeight)
			innerHeight = termsHeight;
		Rect inner = new Rect (0, 0, termsWidth - 12, innerHeight);

		termsScroll = GUI.BeginScrollView (new Rect (Screen.width / 2 - termsWidth / 2, Screen.height * 0.1f, termsWidth, termsHeight), termsScroll, inner,false,true);

		GUI.Label (innerTitle, title, titleStyle);
		GUI.Label (innerTerms, terms, termsStyle);

		GUI.EndScrollView (true);

		if(GUI.Button(new Rect(Screen.width/2 - termsWidth/2, Screen.height-Screen.height*0.24f, termsButton, termsButton*0.35f), "I DISAGREE"))
		{
			Application.Quit();
		}

		if(GUI.Button(new Rect(Screen.width/2 + termsWidth/2 - termsButton, Screen.height-Screen.height*0.24f, termsButton, termsButton*0.35f), "I AGREE"))
		{
			Application.LoadLevel(LevelLoader.LEVEL_MENU);
		}
	}
}
