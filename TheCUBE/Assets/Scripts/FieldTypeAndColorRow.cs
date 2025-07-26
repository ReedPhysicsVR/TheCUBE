using System;
using UnityEngine;

public enum FieldType
{
    E, //0
    B, //1
    Poynting, //2
}

/// <summary>
/// Class that allows a multidimensional array of colors to be displayed in the Unity inspector.
/// For the purposes of adding multiple color palettes.
/// </summary>
[System.Serializable]
public class ColorRow
{
    public Color[] colors;
}
