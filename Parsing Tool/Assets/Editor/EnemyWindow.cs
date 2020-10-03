using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DataTypes;

public class EnemyWindow : EditorWindow
{
    Texture2D headerSectionTexture;
    Texture2D dataSectionTexture;
    Texture2D data2SectionTexture;

    Color headerSectionColor = new Color(1, 1, 1, 1);
    Color dataSectionColor = new Color(0, 0, 0, 1);
    Color data2SectionColor = new Color(0.5f, 0.5f, 0.5f, 1);

    Rect headerSection;
    Rect dataSection;
    Rect data2Section;


    [MenuItem("ToolbarName/OpenWindow")]
    static void OpenWindow()
    {
        // instance kind of
        EnemyWindow window = (EnemyWindow)GetWindow(typeof(EnemyWindow));
        window.minSize = new Vector2(600, 300);
        window.Show();
    }

    // similar to start
    private void OnEnable()
    {
        InitTextures();
        InitData();
    }

    // initialise textures
    void InitTextures()
    {
        headerSectionTexture = new Texture2D(1, 1);
        headerSectionTexture.SetPixel(0, 0, headerSectionColor);
        headerSectionTexture.Apply();

        dataSectionTexture = new Texture2D(1, 1);
        dataSectionTexture.SetPixel(0, 0, dataSectionColor);
        dataSectionTexture.Apply();

        data2SectionTexture = new Texture2D(1, 1);
        data2SectionTexture.SetPixel(0, 0, data2SectionColor);
        data2SectionTexture.Apply();
    }

    // similar to update
    private void OnGUI()
    {
        DrawLayouts();
        DrawHeader();
        DrawSettings();
    }

    // define rect values and paints textures basedon rect
    void DrawLayouts()
    {
        headerSection.x = 0;
        headerSection.y = 0;
        headerSection.width = Screen.width;
        headerSection.height = 50;

        dataSection.x = 0;
        dataSection.y = 60;
        dataSection.width = Screen.width / 3.0f;
        dataSection.height = Screen.height - 50.0f;

        data2Section.x = Screen.width / 3.0f;
        data2Section.y = 60;
        data2Section.width = Screen.width / 3.0f;
        data2Section.height = Screen.height - 50.0f;

        GUI.DrawTexture(headerSection, headerSectionTexture);
        GUI.DrawTexture(dataSection, dataSectionTexture);
        GUI.DrawTexture(data2Section, data2SectionTexture);
    }

    void DrawHeader()
    {
        // basically start working in this gui area
        GUILayout.BeginArea(headerSection);

        // text -- within chosen area
        GUILayout.Label("EnemyDesigner");

        // stop working in this gui area
        GUILayout.EndArea();
    }

    void DrawSettings()
    {
        // basically start working in this gui area
        GUILayout.BeginArea(dataSection);

        // text -- within chosen area
        GUILayout.Label("EnemyDesigner");

        // Layout - working on one line
        GUILayout.BeginHorizontal();
        // tag for enum
        GUILayout.Label("enum options");
        // stores selected enum field.         draws enum options
        mageData.dmgType = (StatType)EditorGUILayout.EnumPopup(mageData.dmgType);
        // layout - end working on one line
        GUILayout.EndHorizontal();

        // checks if button is clicked  -- text, size
        if (GUILayout.Button("Create!", GUILayout.Height(40)))
        {
            GeneralSettings.OpenWindow(GeneralSettings.SettingsType.Mage);
        }


        // stop working in this gui area
        GUILayout.EndArea();
    }

    // SO reference
    static MageData mageData;
    // getter
    public static MageData MageInfo { get { return mageData; } }

    public static void InitData()
    {
        // initialise SO's
        mageData = (MageData)ScriptableObject.CreateInstance(typeof(MageData));
    }

}


public class GeneralSettings : EditorWindow
{
    public enum SettingsType
    {
        Mage,
        Warrior,
        Rogue
    }

    static SettingsType dataSetting;
    static GeneralSettings window;

    public static void OpenWindow(SettingsType _settings)
    {
        dataSetting = _settings;
        window = (GeneralSettings)GetWindow(typeof(GeneralSettings));
        window.minSize = new Vector2(250, 200);
        window.Show();
    }

    private void OnGUI()
    {
        switch(dataSetting)
        {
            case SettingsType.Mage:
            {
                    // passing through character data to adjust
                DrawSettings((CharacterData)EnemyWindow.MageInfo);
                break;
            }
            // other options here
        }
    }

    void DrawSettings(CharacterData _charData)
    {
        // **********************Object FIELDS**********************
        // store the float                     -- float to modify?
        // Layout - working on one line
        GUILayout.BeginHorizontal();
        // tag for objectfield
        GUILayout.Label("prefab");
        //                  cast                                                                        scene?
        _charData.prefab = (GameObject)EditorGUILayout.ObjectField(_charData.prefab, typeof(GameObject), false);
        GUILayout.EndHorizontal();


        // **********************FLOAT FIELDS**********************
        // store the float                     -- float to modify?
        // Layout - working on one line
        GUILayout.BeginHorizontal();
        // tag for float val
        GUILayout.Label("max health");
        _charData.maxHealth = EditorGUILayout.FloatField(_charData.maxHealth);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("max mana");
        _charData.maxMana = EditorGUILayout.FloatField(_charData.maxMana);
        GUILayout.EndHorizontal();


        // **********************SLIDER FIELDS**********************
        // store the float                     -- float to modify?
        // Layout - working on one line
        GUILayout.BeginHorizontal();
        // tag for slider val
        GUILayout.Label("max health");
        _charData.maxHealth = EditorGUILayout.Slider(_charData.maxHealth, 0.0f, 10.0f);
        GUILayout.EndHorizontal();


        // **********************Text FIELDS**********************
        // store the float                     -- float to modify?
        // Layout - working on one line
        GUILayout.BeginHorizontal();
        // tag for text val
        GUILayout.Label("name");
        _charData.name = EditorGUILayout.TextField(_charData.name);
        GUILayout.EndHorizontal();



        //*******************HELP********************
        
        // check if no prefab
        if (_charData.prefab == null)
        {
            // lil warning box thing
            EditorGUILayout.HelpBox("needs prefab", MessageType.Warning);
        }
        else if (_charData.name == null || _charData.name.Length < 1)
        {
            // lil warning box thing
            EditorGUILayout.HelpBox("needs a name", MessageType.Warning);
        }
        // check if button is pressed && prefab exists -- only shows button if prefab
        else if (GUILayout.Button("Finish and Save", GUILayout.Height(30)))
        {
            // call func
            SaveCharacterData();
            // finish up
            window.Close();
        }


    }


    void SaveCharacterData()
    {
        string prefabPath; // path to base prefab
        string newPrefabPath = "Assets/prefabs/characters";
        string dataPath = "Assets/resources/characterData/data/";

        switch (dataSetting)
        {
            case SettingsType.Mage:
            {
                // create asset file at location
                dataPath += "mage/" + EnemyWindow.MageInfo.name + ".asset";
                AssetDatabase.CreateAsset(EnemyWindow.MageInfo, dataPath);

                    newPrefabPath += "mage/" + EnemyWindow.MageInfo.name + ".prefab";
                    // get prefabpath
                    prefabPath = AssetDatabase.GetAssetPath(EnemyWindow.MageInfo.prefab);
                    // make a copy
                    AssetDatabase.CopyAsset(prefabPath, newPrefabPath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    // makes prefab with a reference to the data created
                    GameObject mageprefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                    // if it doesnt have the component, add one
                    if (!mageprefab.GetComponent<Mage>())
                    {
                        mageprefab.AddComponent(typeof(Mage));
                    }
                    mageprefab.GetComponent<Mage>().mageData = EnemyWindow.MageInfo;

                    break;
            }
        }
    }

}