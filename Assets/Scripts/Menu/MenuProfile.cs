using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Blurift.BluRMF;

public class MenuProfile {

	private List<Profile> profiles;

	private Rect createBounds;
	private Rect bounds;
	private Rect createNameInput;
	private Rect createNew;
	private Rect createBack;
	private Rect createTitle;

	private Rect selectBox;

	private string createName = "";

	private int selectedProfile = -1;
	private Vector2 selectPosition = Vector2.zero;

	public MenuProfile()
	{
		string[] files = Directory.GetFiles (GameManager.ProfilePath ());

		List<Profile> profiles = new List<Profile> ();

		for(int i = 0; i < files.Length; i++)
		{
			profiles.Add(Profile.LoadProfile(files[i]));
		}

		this.profiles = profiles;


		SetScreen ();

	}

	/// <summary>
	/// Sets the create screen bounds.
	/// </summary>
	private void SetScreen()
	{
		float width = Screen.width / 2;
		float height = width * 0.6f;


		createBounds = new Rect (width/2, Screen.height/2 - height/2, width, height);
		bounds = createBounds;

		float inputWidth = createBounds.width * 0.8f;
		float inputHeight = height * 0.2f;

		createNameInput = new Rect (width - inputWidth / 2, createBounds.position.y + inputHeight*1.5f, inputWidth, inputHeight);
		createTitle = new Rect (createNameInput.x, createNameInput.y - inputHeight * 1.2f, inputWidth, inputHeight);

		float buttonWidth = inputWidth * 0.3f;

		createNew = new Rect (width - buttonWidth*1.2f, createBounds.position.y + inputHeight * 3.5f, buttonWidth, inputHeight);
		createBack = new Rect (width + buttonWidth*0.2f, createBounds.position.y + inputHeight * 3.5f, buttonWidth, inputHeight);

		selectBox = new Rect (width - inputWidth/2, bounds.position.y + inputHeight/2, inputWidth, inputHeight * 2.5f);
	
	}

	public void DrawCreateProfileScreen()
	{
		GUI.Box (createBounds, "");

		GUIStyle customFont = Menu.GetCustomStyleFont(Menu.LabelCenter, 0.08f);
		Menu.GetCustomStyleFont(customFont, 0.4f);

		GUI.Label (createTitle, "Create a profile", customFont);
		createName = GUI.TextField (createNameInput, createName);

		//Create new profile
		if(GUI.Button(createNew, "Create"))
		{
			if(createName != "")
			{
				Profile p = Profile.Create(createName);
				Menu.MainProfile = p;
				Profile.SaveProfile(GetProfilePath(p.UID), p);
				PlayerPrefs.SetString("Profile", p.UID);
				Menu.CurrentScreen = ScreenType.MainMenu;
			}
		}

		bool oldEnabled = GUI.enabled;

		GUI.enabled = GUI.enabled && profiles.Count != 0;
		//Back button
		if(GUI.Button(createBack, "Back"))
		{
			Menu.CurrentScreen = ScreenType.MainMenu;
		}

		GUI.enabled = oldEnabled;
	}

	public void DrawSelectProfileScreen()
	{
		GUI.Box (bounds, "");

		float selectButtonHeight = selectBox.height * 0.25f;
		float selectInnerHeight = profiles.Count * selectButtonHeight;
		if (selectInnerHeight < selectBox.height)
			selectInnerHeight = selectBox.height;

		Rect selectInner = new Rect (0, 0, selectBox.width - 11, selectInnerHeight);
		selectPosition = GUI.BeginScrollView (selectBox, selectPosition, selectInner);
		{
			for (int i = 0; i < profiles.Count; i++) {
				bool oldEnabled = GUI.enabled;

				GUI.enabled = oldEnabled && selectedProfile != i;
				if(GUI.Button(new Rect(0,selectButtonHeight*i,selectInner.width,selectButtonHeight-2), profiles[i].Name))
					selectedProfile = i;

				GUI.enabled = oldEnabled;
			}
		}
		GUI.EndScrollView (true);

		//Select profile button
		if(GUI.Button(createNew, "Select"))
		{
			if(selectedProfile > -1)
			{
				Menu.MainProfile = profiles[selectedProfile];
				Menu.CurrentScreen = ScreenType.MainMenu;
				PlayerPrefs.SetString("Profile", profiles[selectedProfile].UID);
			}
		}

		if(GUI.Button(createBack, "Back"))
		{
			Menu.CurrentScreen = ScreenType.MainMenu;
		}
	}

	public bool ProfileAvailable()
	{
		return profiles.Count != 0;
	}

	public static string GetProfilePath(string uid)
	{
		return GameManager.ProfilePath () + uid + ".pro";
	}
}
