using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer : ScriptableObject
{
    public List<string>[] table;
    new public string name;

    public void SetupTable(List<string>[] _table, string _name)
    {
        table = new List<string>[_table.Length];
        System.Array.Copy(_table, table, _table.Length);
        name = _name;
    }
}
