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

    [MenuItem("CSV Tool/Open _%#T")]
    public static void ShowWindow()
    {
        // Reference to new Window
        var window = GetWindow<GUITable>();
        // Set Title & Size
        window.titleContent = new GUIContent("CSV Tool");
        window.minSize = new Vector2(274, 50);
    }
    // Setup
    private void OnEnable()
    {      
        SetupHeader();
        SetupTable();
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
        // Header
        tableSectionTexture = new Texture2D(1, 1);
        tableSectionTexture.SetPixel(0, 0, tableSectionColor);
        tableSectionTexture.Apply();
    }

    // Update
    private void OnGUI()
    {
        DrawLayouts();
        DrawHeader();

        DrawTable();
    }

    // Define Rect Values
    void DrawLayouts()
    {
        dataSection.x = 2;
        dataSection.y = 0;
        dataSection.width = 270.0f;
        dataSection.height = 50.0f;
        GUI.DrawTexture(dataSection, dataSectionTexture);

        tableSection.x = 272;
        tableSection.y = 52;
        tableSection.width = Screen.width - tableSection.x - 2;
        tableSection.height = 500.0f;
        GUI.DrawTexture(tableSection, tableSectionTexture);
    }

    public bool doOnce = false;

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

        // stop working in this gui area
        GUILayout.EndArea();
    }

    void LoadTable()
    {
        // Get # of lines in File, setup empty array of lists
        table = new List<string>[CSVReader.GetLines(AssetDatabase.GetAssetPath(source))];
        for (int i = 0; i < CSVReader.GetLines(AssetDatabase.GetAssetPath(source)); i++)
        {
            table[i] = new List<string>();
        }

        loaded = true;
        CSVReader.LoadFromFile(AssetDatabase.GetAssetPath(source), new CSVReader.ReadLineDelegate(TableAttempt));
    }

    void DrawTable()
    {
        // Check CSV file is loaded
        if (source && loaded)
        {
            GUILayout.BeginArea(tableSection);

            float width = 150;

            for (int i = 0; i < table.Length; i++)
            {
                GUILayout.BeginHorizontal();

                for (int j = 0; j < table[i].Count; j++)
                {
                    table[i][j] = GUILayout.TextField(table[i][j], GUILayout.Width(width / table[i].Count));
                }

                GUILayout.EndHorizontal();
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

    // Table Container
    public List<string>[] table;

    #region OpenFile & Save
    public StreamWriter outStream;
    public string path;

    public bool loaded = false;

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
        // Save file for reading/writing
        outStream = File.CreateText(path);

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

    private void OpenFile()
    {
        var filePath = EditorUtility.OpenFilePanel("level", Application.streamingAssetsPath, "csv");
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
                CSVReader.LoadFromFile(filePath, new CSVReader.ReadLineDelegate(TableAttempt));
                // Save Text
                CSVReader.LoadFromFile(filePath, new CSVReader.ReadLineDelegate(SaveText));
                outStream.Close();
                // Load the Resource
                source = Resources.Load<TextAsset>(endName);
                loaded = true;
            }
        }
    }
    #endregion OpenFile & Save
}
