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
    List<string> toolbarTags = new List<string>();
    int _toolbar_sel = 0;


    // DataSheet Reference
    public UnityEngine.Object source;
    // Data Section Styling
    Texture2D dataSectionTexture;
    Rect dataSection;
    Color dataSectionColor = new Color(0.22f, 0.22f, 0.22f, 1);
    // Table Section Styling
    Texture2D tableSectionTexture;
    Rect tableSection;
    Color tableSectionColor = new Color(0.0f, 0.0f, 0.0f, 1);
    // Toolbar Section Styling
    Texture2D toolbarSectionTexture;
    Rect toolbarSection;
    Color toolbarSectionColor = new Color(0.0f, 0.0f, 0.0f, 1);

    [MenuItem("CSV Tool/Open _%#T")]
    public static void ShowWindow()
    {
        // Reference to new Window
        var window = GetWindow<GUITable>();
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

        // Safety
        if (dictionary != null)
        {
            dictionary.Clear();
        }
    }
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

    // Update
    private void OnGUI()
    {
        DrawLayouts();
        DrawHeader();
        DrawToolbar();
        if (_toolbar_sel == 0)
        {
            DrawTable();
        }
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
        tableSection.y = 52;
        tableSection.width = Screen.width - tableSection.x - 2;
        tableSection.height = 500.0f;
        GUI.DrawTexture(tableSection, tableSectionTexture);

        toolbarSection.x = 274;
        toolbarSection.y = 0;
        toolbarSection.width = Screen.width - toolbarSection.x - 2;
        toolbarSection.height = 120.0f;
        GUI.DrawTexture(toolbarSection, toolbarSectionTexture);
    }

    void DrawToolbar()
    {
        GUILayout.BeginArea(toolbarSection);

        GUILayout.BeginHorizontal();

        GUILayout.Space(6);
        _toolbar_sel = GUILayout.Toolbar(_toolbar_sel, toolbarTags.ToArray(), GUILayout.Width(250));

        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
    
    void DrawHeader()
    {
        // basically start working in this gui area
        GUILayout.BeginArea(dataSection);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("DataSheet");
        source = EditorGUILayout.ObjectField(source, typeof(TextAsset), true, GUILayout.Width(150));
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
        }

        if (!source)
        {
            EditorGUILayout.HelpBox("Please assign [DataSheet] to proceed.", MessageType.Warning);
        }
        // stop working in this gui area
        GUILayout.EndArea();
    }

    void LoadTable()
    {
        if (source)
        {
            // Get # of lines in File, setup empty array of lists
            table = new List<string>[CSVReader.GetLines(AssetDatabase.GetAssetPath(source))];
            for (int i = 0; i < CSVReader.GetLines(AssetDatabase.GetAssetPath(source)); i++)
            {
                table[i] = new List<string>();
            }

            loaded = true;
            CSVReader.LoadFromFile(AssetDatabase.GetAssetPath(source), new CSVReader.ReadLineDelegate(TableAttempt), new CSVReader.StringToEnumValue(AddEnumValue));

            if (!toolbarTags.Contains(source.name))
            {
                toolbarTags.Add(source.name);
            }
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

            float width = EditorWindow.GetWindow<GUITable>().position.width - 290 - 20;

            for (int i = 0; i < table.Length; i++)
            {
                if (i == 0)
                {                   
                    GUILayout.BeginHorizontal();

                    GUILayout.Space(26);
                    for (int j = 0; j < table[i].Count; j++)
                    {
                        if (GUILayout.Button("-", GUILayout.Width((width - 30) / table[i].Count), GUILayout.Height(20)))
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
                for (int j = 0; j < table[i].Count; j++)
                {
                    if (dictionary.ContainsKey(j) && i != 0)
                    {
                        string[] tempArray = dictionary[j].ToArray();
                        int tempIndex = Array.FindIndex(tempArray, element => element == table[i][j]);
                        // Trim % from Start - Shouldn't be too performance heavy
                        for (int k = 0; k < tempArray.Length; k++)
                        {
                            tempArray[k] = tempArray[k].Remove(0, 1);
                        }

                        tempIndex = EditorGUILayout.Popup(tempIndex, tempArray, GUILayout.Width((width - 30) / table[i].Count));

                        table[i][j] = dictionary[j][tempIndex];
                    }
                    else
                    {
                        table[i][j] = GUILayout.TextField(table[i][j], GUILayout.Width((width - 30) / table[i].Count));
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
            if (table[line_index].Count <= i)
            {
                table[line_index].Add(line[i]);
            }
            else
            {
                table[line_index][i] = line[i];
            }
        }
    }

    void AddLineToTable()
    {
        List<string>[] temp = new List<string>[table.Length + 1];
        
        for (int i = 0; i <= table.Length; i++)
        {
            if (i == table.Length)
            {
                temp[i] = new List<string>();
                for (int j = 0; j < table[0].Count; j++)
                {
                    temp[i].Add("");
                }
            }
            else
            {
                temp[i] = table[i];
            }
        }

        table = temp;
    }

    void RemoveLineFromTable(int _row)
    {
        List<string>[] temp = new List<string>[table.Length - 1];

        for (int i = 0; i < table.Length; i++)
        {
            if (i < _row)
            {
                temp[i] = table[i];
            }
            else if (i > _row)
            {
                temp[i - 1] = table[i];
            }
        }
        table = temp;
    }
        
    void AddColumnToTable()
    {
        List<string>[] temp = new List<string>[table.Length];

        for (int i = 0; i < table.Length; i++)
        {
            temp[i] = table[i];
            temp[i].Add("");
        }

        table = temp;
    }

    void RemoveColumnFromTable(int _column)
    {
        List<string>[] temp = new List<string>[table.Length];

        for (int i = 0; i < table.Length; i++)
        {
            temp[i] = table[i];
            temp[i].RemoveAt(_column);
        }
        table = temp;
    }

    // Table Container
    public List<string>[] table;

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
            tableSO.SetupTable(table, endName);

            AssetDatabase.CreateAsset(tableSO, "Assets/Tables/" + endName + ".asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void SaveText(int line_index, List<string> line)
    {
        string temp = "";

        for (int i = 0; i < line.Count; i++)
        {
            if (i != line.Count - 1)
            {
                temp += line[i] + ",";
            }
            else
            {
                temp += line[i];
            }
        }
        outStream.WriteLine(temp);
    }

    private void SaveTableAsText()
    {
        if (source)
        {
            // Save file for reading/writing
            outStream = File.CreateText(AssetDatabase.GetAssetPath(source));

            for (int i = 0; i < table.Length; i++)
            {
                string temp = "";
                for (int j = 0; j < table[i].Count; j++)
                {
                    if (j != table[i].Count - 1)
                    {
                        temp += table[i][j] + ",";
                    }
                    else
                    {
                        temp += table[i][j];
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

                // Get # of lines in File, setup empty array of lists
                table = new List<string>[CSVReader.GetLines(filePath)];
                for (int i = 0; i < CSVReader.GetLines(filePath); i++)
                {
                    table[i] = new List<string>();
                }

                // Load Text from File 
                CSVReader.LoadFromFile(filePath, new CSVReader.ReadLineDelegate(TableAttempt), new CSVReader.StringToEnumValue(AddEnumValue));
                // Save Text
                CSVReader.LoadFromFile(filePath, new CSVReader.ReadLineDelegate(SaveText), new CSVReader.StringToEnumValue(AddEnumValue));
                outStream.Close();

                // Load the Resource
                source = Resources.Load<TextAsset>(endName);

                if (!source)
                {
                    Debug.LogError("Resource did not Load!");
                    Debug.LogError("Resources path: " + endName);
                }

                loaded = true;
            }

            if (!toolbarTags.Contains(source.name))
            {
                toolbarTags.Add(source.name);
            }
        }
    }

    private void SaveFile()
    {
        if (source)
        {
            var filePath = EditorUtility.SaveFilePanel("Save Datasheet", "", source.name, ".csv");
            if (filePath.Length != 0)
            {
                // Save as CSV
                outStream = File.CreateText(filePath);

                for (int i = 0; i < table.Length; i++)
                {
                    string temp = "";
                    for (int j = 0; j < table[i].Count; j++)
                    {
                        if (j != table[i].Count - 1)
                        {
                            temp += table[i][j] + ",";
                        }
                        else
                        {
                            temp += table[i][j];
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
