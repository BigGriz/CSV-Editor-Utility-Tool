using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Data/Table", order = 1)]
[System.Serializable]
public class TableSO : ScriptableObject
{
    public ArrayField[] array = new ArrayField[5];
}

[System.Serializable]
public class ArrayField
{
    [SerializeField]
    public string[] row = new string[5];
}
