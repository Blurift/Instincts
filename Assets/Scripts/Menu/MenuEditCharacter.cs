using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Blurift;
using Blurift.UI;

public class MenuEditCharacter {

    private Menu menu;

    private int hairStyle = 0;

    private ColorPicker hairColor;
    private ColorPicker topColor;
    private float hairR = 0.7f;
    private float hairG = 0.4f;
    private float hairB = 0.4f;

    List<Texture2D> hairStyles = new List<Texture2D>();
    Texture2D charHead;
    Texture2D charArms;

    private Rect hairEditor;
    private Rect topEditor;

    private Rect mainRect;
    private Rect hairStyleRect;
    private Rect hairStyleText;


    public MenuEditCharacter(Menu menu)
    {
        this.menu = menu;

        foreach(Sprite s in Menu.HairStyles)
        {
            hairStyles.Add(GetTex(s));
        }

        charHead = GetTex(menu.CharHead);
        charArms = GetTex(menu.CharArms);



        //Color Pickers
        float pickerWidth = Screen.width*0.2f;
        float pickerHeight = pickerWidth/2;

        float pickerBorder = pickerWidth * 0.05f;

        float hairX = (Screen.width*0.75f)- (pickerWidth + pickerBorder * 2) / 2;
        float hairY = Screen.height / 2 - (pickerHeight + pickerBorder * 3);
        float topY = Screen.height / 2 + pickerBorder;

        hairEditor = new Rect(hairX, hairY, pickerWidth + pickerBorder * 2, pickerHeight + pickerBorder * 2);
        topEditor = new Rect(hairX, topY, pickerWidth + pickerBorder * 2, pickerHeight + pickerBorder * 2);

        hairColor = new ColorPicker(hairX + pickerBorder, hairY+pickerBorder, pickerWidth, pickerHeight, "Hair Color");
        topColor = new ColorPicker(hairX+pickerBorder, topY + pickerBorder, pickerWidth, pickerHeight, "Top Color");

        
        hairColor.SetThreshold(0.1f, 0.9f);
        topColor.SetThreshold(0.1f, 0.9f);

        //
        mainRect = new Rect(Screen.width / 2 - (pickerWidth + pickerBorder * 2) / 2, Screen.height / 2 - pickerHeight, pickerWidth + pickerBorder * 2, pickerHeight * 2 + pickerBorder * 2);
        hairStyleText = new Rect(Screen.width / 2 - pickerWidth / 2, Screen.height/2, pickerWidth, Screen.width*0.03f);
        hairStyleRect = new Rect(hairStyleText);
        hairStyleRect.y += hairStyleText.height;

        if (PlayerPrefs.HasKey("HairStyle"))
        {
            hairR = PlayerPrefs.GetFloat("HairR");
            hairG = PlayerPrefs.GetFloat("HairG");
            hairB = PlayerPrefs.GetFloat("HairB");
            float topR = PlayerPrefs.GetFloat("TopR");
            float topG = PlayerPrefs.GetFloat("TopG");
            float topB = PlayerPrefs.GetFloat("TopB");

            hairStyle = PlayerPrefs.GetInt("HairStyle");

            topColor.Set(new Vector3(topR, topG, topB));
            hairColor.Set(new Vector3(hairR, hairG, hairB));
        }
    }

    public void Draw()
    {
        //GUI Measurements
        float buttonWidth = Screen.width * 0.18f;
        float buttonHeight = buttonWidth * 0.2f;
        float buttonMargin = Screen.width * 0.01f;
        float buttonSizeW = Screen.width * 0.2f;
        float buttonSizeH = buttonHeight + buttonMargin * 2;

        Rect backButton = new Rect(Screen.width - buttonMargin - buttonWidth, Screen.height - buttonHeight - buttonMargin, buttonWidth, buttonHeight);
        Rect hairDemo = new Rect(Screen.width / 2 - 32, 400, 64, 64);

        //Buttons
        if (GUI.Button(backButton, "Back"))
        {
            menu.SwitchScreen(ScreenType.MainMenu);

            Color t = topColor.GetColor();
            Color h = hairColor.GetColor();

            PlayerPrefs.SetInt("HairStyle", hairStyle);
            PlayerPrefs.SetFloat("HairR", h.r);
            PlayerPrefs.SetFloat("HairG", h.g);
            PlayerPrefs.SetFloat("HairB", h.b);
            PlayerPrefs.SetFloat("TopR", t.r);
            PlayerPrefs.SetFloat("TopG", t.g);
            PlayerPrefs.SetFloat("TopB", t.b);

            Menu.TopColor = topColor.GetVector3();
            Menu.HairColor = hairColor.GetVector3();
            Menu.HairStyle = hairStyle;

        }


        GUI.Box(mainRect, GUIContent.none);
        
        GUIStyle text = BluStyle.CustomStyle(Menu.LabelCenter, hairStyleText.height * 0.7f);
        GUI.Label(hairStyleText, "Hair Style", text);
        hairStyle = (int)GUI.Slider(hairStyleRect, hairStyle, 1, 0, Menu.HairStyles.Count, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb, true, 10);


        //Draw Color Pickers
        GUI.Box(hairEditor, GUIContent.none);
        GUI.Box(topEditor, GUIContent.none);
        ColorPicker.Reset();
        hairColor.Draw();
        topColor.Draw();


        //Draw Arms
        GUI.color = topColor.GetColor();
        GUI.DrawTexture(hairDemo, charArms);
        GUI.color = Color.white;

        //Draw Head
        GUI.DrawTexture(hairDemo, charHead);

        GUI.color = hairColor.GetColor();
        GUI.DrawTexture(hairDemo, hairStyles[hairStyle]);
        GUI.color = Color.white;
    }

    Texture2D GetTex(Sprite sprite)
    {
        Texture2D hairTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

        Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height);

        hairTex.SetPixels(pixels);
        hairTex.Apply();

        return hairTex;
    }
}
