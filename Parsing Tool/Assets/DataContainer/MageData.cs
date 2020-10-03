using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// our types namespace
using DataTypes;

[CreateAssetMenuAttribute(fileName = "New Mage Data", menuName = "Data/Mage")]
public class MageData : CharacterData
{
    public StatType dmgType;
    public StatType2 wpnType;
}
