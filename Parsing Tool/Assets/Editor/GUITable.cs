using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class GUITable : EditorWindow
{
    // Multiple Tabs
    List<string> toolbarTags;
    int _toolbar_sel = 0;
    int temp = 0;
    bool firstTable = true;

    public static GUITable window;

    // DataSheet Reference
    public UnityEngine.Object source;
    public UnityEngine.Object sourceToLoad;
    public List<UnityEngine.Object> sources;
    // Data Section Styling
    Texture2D dataSectionTexture;
    Rect dataSection;
    Color dataSectionColor = new Color(0.3f, 0.3f, 0.3f, 1);
    // Table Section Styling
    Texture2D tableSectionTexture;
    Rect tableSection;
    Color tableSectionColor = new Color(0.2f, 0.2f, 0.2f, 1);
    // Toolbar Section Styling
    Texture2D toolbarSectionTexture;
    Rect toolbarSection;
    Color toolbarSectionColor = new Color(0.25f, 0.25f, 0.25f, 1);

    [MenuItem("CSV Tool/Open _%#T")]
    public static void ShowWindow()
    {
        // Reference to new Window
        window = GetWindow<GUITable>();
        // Set Title & Size
        window.titleContent = new GUIContent("CSV Tool");
        window.minSize = new Vector2(744, 120);
    }
    // Setup
    private void OnEnable()
    {
        loaded = false;
        source = null;
        SetupHeader();
        SetupTable();
        SetupToolbar();

        // Toolbars
        if (sources == null)
        {
            sources = new List<UnityEngine.Object>();
        }
        if (toolbarTags == null)
        {
            toolbarTags = new List<string>();
        }

        // Safety
        if (dictionary != null)
        {
            dictionary.Clear();
        }

        // Tables
        table = new List<List<string>[]>();
        tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
        tex.Apply();
    }
    #region StylingSections
    void SetupHeader()
    {
        // Header
        dataSectionTexture = new Texture2D(1, 1);
        dataSectionTexture.SetPixel(0, 0, dataSectionColor);
        dataSectionTexture.Apply();
    }
    void SetupTable()
    {
        // Table
        tableSectionTexture = new Texture2D(1, 1);
        tableSectionTexture.SetPixel(0, 0, tableSectionColor);
        tableSectionTexture.Apply();
    }
    void SetupToolbar()
    {
        // Toolbar
        toolbarSectionTexture = new Texture2D(1, 1);
        toolbarSectionTexture.SetPixel(0, 0, toolbarSectionColor);
        toolbarSectionTexture.Apply();
    }
    #endregion StylingSections

    // Update
    private void OnGUI()
    {
        DrawBackGround();
        DrawLayouts();
        DrawToolbar();
        DrawHeader(_toolbar_sel);
        if (_toolbar_sel >= 0)
        {
            DrawTable();
        }
    }

    #region DrawCalls
    Texture2D tex;
    void DrawBackGround()
    {
        GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), tex, ScaleMode.StretchToFill);
    }
    // Define Rect Values
    void DrawLayouts()
    {
        dataSection.x = 2;
        dataSection.y = 0;
        dataSection.width = 270.0f;
        dataSection.height = 120.0f;
        GUI.DrawTexture(dataSection, dataSectionTexture);

        tableSection.x = 274;
        tableSection.y = 22;
        tableSection.width = Screen.width - tableSection.x - 2;
        tableSection.height = Screen.height - toolbarSection.y - 2;
        GUI.DrawTexture(tableSection, tableSectionTexture);

        toolbarSection.x = 274;
        toolbarSection.y = 0;
        toolbarSection.width = Screen.width - toolbarSection.x - 2;
        toolbarSection.height = 22.0f;
        GUI.DrawTexture(toolbarSection, toolbarSectionTexture);
    }
    void DrawToolbar()
    {
        GUILayout.BeginArea(toolbarSection);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
                GUILayout.Space(28);
                _toolbar_sel = GUILayout.Toolbar(_toolbar_sel, toolbarTags.ToArray(), GUILayout.Width(58 * toolbarTags.Count));
                if (sources.Count != 0)
                {
                    source = sources[_toolbar_sel];
                }
            GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    void DrawHeader(int _sel)
    {
        // basically start working in this gui area
        GUILayout.BeginArea(dataSection);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("DataSheet");
        sourceToLoad = EditorGUILayout.ObjectField(sourceToLoad, typeof(TextAsset), false, GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        // OpenFile Button
        if (GUILayout.Button("Open"))
        {
            OpenFile();
        }
        if (GUILayout.Button("Load"))
        {
            LoadTable();
        }
        if (GUILayout.Button("Save"))
        {
            SaveTableAsText();
        }
        EditorGUILayout.EndHorizontal();
        if (source && loaded)
        {
            if (GUILayout.Button("Save As..."))
            {
                SaveFile();
            }

            // Don't need to show this one
            /*EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Current DataSheet");
            source = EditorGUILayout.ObjectField(source, typeof(TextAsset), true, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();*/
        }

        if (!source)
        {
            EditorGUILayout.HelpBox("Please assign [DataSheet] to proceed.", MessageType.Warning);
        }
        // stop working in this gui area
        GUILayout.EndArea();
    }
    #endregion DrawCalls

    void LoadTable()
    {
        if (sourceToLoad)
        {
            temp = _toolbar_sel;

            if (!toolbarTags.Contains(sourceToLoad.name))
            {
                toolbarTags.Add(sourceToLoad.name);

                // Move pointer forward -- need to move this down but temp
                if (!firstTable)
                {
                    temp++;
                }
                else
                {
                    firstTable = false;
                }
            }

            string tempPath = AssetDatabase.GetAssetPath(sourceToLoad);

            // Get # of lines in File, setup empty array of lists
            table.Add(new List<string>[CSVReader.GetLines(tempPath)]);
            for (int i = 0; i < CSVReader.GetLines(tempPath); i++)
            {
                table[temp][i] = new List<string>();
            }

            loaded = true;
            CSVReader.LoadFromFile(tempPath, new CSVReader.ReadLineDelegate(TableAttempt), new CSVReader.StringToEnumValue(AddEnumValue));

            sources.Add(sourceToLoad);
            source = sourceToLoad;
            sourceToLoad = null;
        }
    }

    // Dictionary for Enum Values
    Dictionary<int, List<string>> dictionary;
    void AddEnumValue(int line_index, List<string> line)
    {
        // Check dictionary exists
        if (dictionary == null)
        {
            // If not, make one
            dictionary = new Dictionary<int, List<string>>();
        }

        // Check if '%' prefix is present or a dictionary for column already exists
        if (line[line.Count - 1].ToString().StartsWith("%") || dictionary.ContainsKey(line.Count - 1))
        {
            // Don't count headers
            if (line_index != 0)
            {
                // Check if entry is empty
                if (!dictionary.ContainsKey(line.Count - 1))
                {
                    // If so, create new list
                    dictionary.Add((line.Count - 1), new List<string>());
                }

                // Add '%' sign if doesn't exist
                if (!line[line.Count - 1].ToString().StartsWith("%"))
                {
                    line[line.Count - 1] = "%" + line[line.Count - 1];
                }

                // Check dictionary does not already contain the value
                if (!dictionary[(line.Count - 1)].Contains(line[line.Count - 1]))
                {
                    // If so add the value to the list
                    dictionary[(line.Count - 1)].Add(line[line.Count - 1]);
                }
            }
        }
    }
    
    void DrawTable()
    {
        // Check CSV file is loaded
        if (source && loaded)
        {
            GUILayout.BeginArea(tableSection);
            GUILayout.Space(2);

            float width = window.position.width - 290 - 24;

            for (int i = 0; i < table[_toolbar_sel].Length; i++)
            {
                if (i == 0)
                {                   
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(26);
                    for (int j = 0; j < table[_toolbar_sel][i].Count; j++)
                    {
                        if (GUILayout.Button("-", GUILayout.Width((width - 30) / table[_toolbar_sel][i].Count), GUILayout.Height(20)))
                        {
                            RemoveColumnFromTable(j);
                        }
                    }
                    if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        AddColumnToTable();
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.Width(20), GUILayout.Height(20)))
                {                   
                    RemoveLineFromTable(i);
                }
                for (int j = 0; j < table[_toolbar_sel][i].Count; j++)
                {
                    if (dictionary.ContainsKey(j) && i != 0)
                    {
                        // Check if new Row Added
                        if (!dictionary[j].Contains(table[_toolbar_sel][i][j]))
                        {
                            // If so add the value to the list
                            dictionary[j].Add(table[_toolbar_sel][i][j]);
                        }

                        string[] tempArray = dictionary[j].ToArray();
                        int tempIndex = Array.FindIndex(tempArray, element => element == table[_toolbar_sel][i][j]);
                        // Trim % from Start - Shouldn't be too performance heavy
                        for (int k = 0; k < tempArray.Length; k++)
                        {
                            if (tempArray[k].Length > 0)
                            {
                                tempArray[k] = tempArray[k].Remove(0, 1);
                            }
                        }

                        tempIndex = EditorGUILayout.Popup(tempIndex, tempArray, GUILayout.Width((width - 30) / table[_toolbar_sel][i].Count));

                        table[_toolbar_sel][i][j] = dictionary[j][tempIndex];
                    }
                    else
                    {
                        table[_toolbar_sel][i][j] = GUILayout.TextField(table[_toolbar_sel][i][j], GUILayout.Width((width - 30) / table[_toolbar_sel][i].Count));
                    }
                }
                GUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+", GUILayout.Width(20), GUILayout.Height(20)))
            {
                AddLineToTable();
            }
            GUILayout.EndArea();
        }
        else
        {
            loaded = false;
        }
    }
    
    void TableAttempt(int line_index, List<string> line)
    {
        for (int i = 0; i < line.Count; i++)
        {
            if (table[temp][line_index].Count <= i)
            {
                table[temp][line_index].Add(line[i]);
            }
            else
            {
                table[temp][line_index][i] = line[i];
            }
        }
    }

    #region Add/Remove Rows & Columns
    void AddLineToTable()
    {
        List<string>[] temp = new List<string>[table[_toolbar_sel].Length + 1];
        
        for (int i = 0; i <= table[_toolbar_sel].Length; i++)
        {
            if (i == table[_toolbar_sel].Length)
            {
                temp[i] = new List<string>();
                for (int j = 0; j < table[_toolbar_sel][0].Count; j++)
                {
                    temp[i].Add("");
                }
            }
            else
            {
                temp[i] = table[_toolbar_sel][i];
            }
        }

        table[_toolbar_sel] = temp;
    }
    void RemoveLineFromTable(int _row)
    {
        List<string>[] temp = new List<string>[table[_toolbar_sel].Length - 1];

        for (int i = 0; i < table[_toolbar_sel].Length; i++)
        {
            if (i < _row)
            {
                temp[i] = table[_toolbar_sel][i];
            }
            else if (i > _row)
            {
                temp[i - 1] = table[_toolbar_sel][i];
            }
        }
        table[_toolbar_sel] = temp;
    }      
    void AddColumnToTable()
    {
        List<string>[] temp = new List<string>[table[_toolbar_sel].Length];

        for (int i = 0; i < table[_toolbar_sel].Length; i++)
        {
            temp[i] = table[_toolbar_sel][i];
            temp[i].Add("");
        }

        table[_toolbar_sel] = temp;
    }
    void RemoveColumnFromTable(int _column)
    {
        List<string>[] temp = new List<string>[table[_toolbar_sel].Length];

        for (int i = 0; i < table[_toolbar_sel].Length; i++)
        {
            temp[i] = table[_toolbar_sel][i];
            temp[i].RemoveAt(_column);
        }
        table[_toolbar_sel] = temp;
    }
    #endregion Add/Remove Rows & Columns

    // Table Container
    public List<List<string>[]> table;

    #region OpenFile & Save
    public StreamWriter outStream;
    public string path;

    public bool loaded = false;

    void SaveSO()
    {
        if (source)
        {
            string endName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(source));

            DataContainer tableSO = ScriptableObject.CreateInstance<DataContainer>();
            tableSO.SetupTable(table[_toolbar_sel], endName);

            AssetDatabase.CreateAsset(tableSO, "Assets/Tables/" + endName + ".asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void SaveText(int line_index, List<string> line)
    {
        string tempText = "";

        for (int i = 0; i < line.Count; i++)
        {
            if (i != line.Count - 1)
            {
                tempText += line[i] + ",";
            }
            else
            {
                tempText += line[i];
            }
        }
        outStream.WriteLine(tempText);
    }

    private void SaveTableAsText()
    {
        if (source)
        {
            // Save file for reading/writing
            outStream = File.CreateText(AssetDatabase.GetAssetPath(source));

            for (int i = 0; i < table[_toolbar_sel].Length; i++)
            {
                string temp = "";
                for (int j = 0; j < table[_toolbar_sel][i].Count; j++)
                {
                    if (j != table[_toolbar_sel][i].Count - 1)
                    {
                        temp += table[_toolbar_sel][i][j] + ",";
                    }
                    else
                    {
                        temp += table[_toolbar_sel][i][j];
                    }

                }
                outStream.WriteLine(temp);
            }
            outStream.Close();

            SaveSO();
        }
    }

    private void OpenFile()
    {
        var filePath = EditorUtility.OpenFilePanel("Select Datasheet", Application.streamingAssetsPath, "csv");
        if (filePath.Length != 0)
        {
            if (filePath.EndsWith(".csv"))
            {
                // Get FilePath & Save Location
                string fullPath = Path.GetFullPath(filePath).TrimEnd(Path.DirectorySeparatorChar);
                string fileName = fullPath.Split(Path.DirectorySeparatorChar).Last();
                string endName = Path.GetFileNameWithoutExtension(filePath);
                string savePath = Application.dataPath + "/Resources/";


                // Save file for reading/writing
                path = savePath + endName + ".txt";
                outStream = File.CreateText(path);

                temp = _toolbar_sel;

                // If new Source add Toolbar
                if (!toolbarTags.Contains(endName))
                {
                    toolbarTags.Add(endName);
                    // Check if this is the first table loaded
                    if (!firstTable)
                    {
                        // If not, move the selection num forward
                        temp++;
                    }
                    else
                    {
                        // else mark as first loaded
                        firstTable = false;
                    }
                }

                // Get # of lines in File, setup empty array of lists
                table.Add(new List<string>[CSVReader.GetLines(filePath)]);
                for (int i = 0; i < CSVReader.GetLines(filePath); i++)
                {
                    table[temp][i] = new List<string>();
                }

                // Load Text from File 
                CSVReader.LoadFromFile(filePath, new CSVReader.ReadLineDelegate(TableAttempt), new CSVReader.StringToEnumValue(AddEnumValue));
                // Save Text
                CSVReader.LoadFromFile(filePath, new CSVReader.ReadLineDelegate(SaveText), new CSVReader.StringToEnumValue(AddEnumValue));
                outStream.Close();

                // Load the Resource
                source = Resources.Load<TextAsset>(endName);
                sources.Add(source);

                if (!source)
                {
                    Debug.LogError("Resource did not Load!");
                    Debug.LogError("Resources path: " + endName);
                }

                loaded = true;
            }
        }
    }

    private void SaveFile()
    {
        if (source)
        {
            var filePath = EditorUtility.SaveFilePanel("Save Datasheet", "", source.name, "csv");
            if (filePath.Length != 0)
            {
                // Save as CSV
                outStream = File.CreateText(filePath);

                for (int i = 0; i < table[_toolbar_sel].Length; i++)
                {
                    string temp = "";
                    for (int j = 0; j < table[_toolbar_sel][i].Count; j++)
                    {
                        if (j != table[_toolbar_sel][i].Count - 1)
                        {
                            temp += table[_toolbar_sel][i][j] + ",";
                        }
                        else
                        {
                            temp += table[_toolbar_sel][i][j];
                        }

                    }
                    outStream.WriteLine(temp);
                }
                outStream.Close();
            }
        }
    }
    #endregion OpenFile & Save
}
