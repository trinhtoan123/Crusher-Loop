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
    public ColorRope color;
    public Material material;
    public Material materialRoll;
  

}
[System.Serializable]
public class SpoolDirection
{
    public Direction direction;
    public Sprite sprite;
}
[System.Serializable]
public enum ColorRope
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


