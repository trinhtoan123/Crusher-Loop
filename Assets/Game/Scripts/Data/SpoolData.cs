using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpoolData", menuName = "Data/SpoolData")]
public class SpoolData : ScriptableObject
{
    [SerializeField] private List<SpoolColor> spoolColors;
    [SerializeField] private List<SpoolDirection> spoolDirections;
    [SerializeField] private Material materialKnitClear;

    public List<SpoolColor> SpoolColors => spoolColors;
    public List<SpoolDirection> SpoolDirections => spoolDirections;
    public Material MaterialKnitClear => materialKnitClear;

}
[System.Serializable]
public class SpoolColor
{
    public ColorRope color;
    public Material materialSpool;
    public Material materialRoll;
    public Material materialKnit;

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
    Blue,
    Brown,
    Cyan,
    Green,
    Orange,
    Pink,
    Puple,
    Red,
    Yellow
}
[System.Serializable]
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}


