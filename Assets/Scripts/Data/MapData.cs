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
    public List<TimeWaveData> timeWave;
    public List<DistanceWaveData> distanceWave;
    public List<MentalWaveData> mentalWave;
    public float mapSize;
    public MapData nextMapData;

    public List<ShopItemData> shopItemDataList;
    public List<GameObject> randomEvent;
    public float randomEventInterval = 200;
}

public class Wave
{
    public WaveData waveData;

    public float elapsedTime;
}

[System.Serializable]
public class WaveData
{
    public float genTime;
}

[System.Serializable]
public class TimeWaveData : WaveData
{
    public int startTime;
    public int endTime;

    public Vector3 genLocalPosition;

    public List<Define.EnemyName> enemyList;
    public float hpMultiply;
}
[System.Serializable]
public class DistanceWaveData : WaveData
{
    public float distace;

    public Vector3 genLocalPosition;

    public List<Define.EnemyName> enemyList;
    public float hpMultiply;
}

[System.Serializable]
public class MentalWaveData : WaveData
{
    public int genMentalLevelOrMore;

    public Vector3 genLocalPosition;

    public List<Define.EnemyName> enemyList;
    public float hpMultiply;
}