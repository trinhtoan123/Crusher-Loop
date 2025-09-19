using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpoolData", menuName = "Data/SpoolData")]
public class SpoolData : ScriptableObject
{
    public List<SpoolColor> spoolColors;
    public List<SpoolDirection> spoolDirections;

}
[System.Serializable]
public class SpoolColor
{
    public Color color;
    public Material material;
  

}
[System.Serializable]
public class SpoolDirection
{
    public Direction direction;
    public Sprite sprite;
}
[System.Serializable]
public enum Color
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Orange,
    Pink,
    Brown,
    Gray,
}
[System.Serializable]
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}


