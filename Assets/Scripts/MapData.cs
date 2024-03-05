using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "Create Map", menuName = "AddData/Create MapData", order = 1)]

public class MapData : ScriptableObject
{
    public string sceneName;
    public float mentalDownTime;
    public List<Wave> timeWave;
    public List<Wave> distanceWave;
}
