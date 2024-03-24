using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "Create Map", menuName = "AddData/Create MapData", order = 1)]

public class MapData : ScriptableObject
{
    public string sceneName;
    public float mentalDownTime;
    public List<Wave> timeWave;
    public List<Wave> distanceWave;
    public float mapSize;
    public MapData nextMapData;

    public List<ShopItemData> shopItemDataList;
    public List<GameObject> randomEvent;
    public float randomEventInterval = 200;
}

[System.Serializable]
public class Wave
{
    public int startTime;
    public int endTime;
    public float genTime;
    public float elapsedTime;

    public float distace;
    public Vector3 genLocalPosition;

    public List<Define.EnemyName> enemyList;
    public float hpMultiply;
}