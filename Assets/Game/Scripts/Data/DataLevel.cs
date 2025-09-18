using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataLevel", menuName = "Data/DataLevel")]
public class DataLevel : ScriptableObject
{
    [SerializeField] private List<Level> levels;
    public List<Level> Levels => levels;

}
[System.Serializable]
public class Level
{
    public GameObject levelPrefab;
    public int cointEarn;
}

