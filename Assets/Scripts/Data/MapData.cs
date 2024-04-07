using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Map", menuName = "AddData/Create MapData", order = 1)]

public class MapData : ScriptableObject
{
    public string sceneName;
    public List<TimeWaveData> timeWave;
    public List<DistanceWaveData> distanceWave;
    public List<MentalWaveData> mentalWave;
    public List<PresetWaveData> presetWave;
    public float mapSize;
    public MapData nextMapData;

    public List<ItemData> shopItemDataList;
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
    public float multiply;

    public Vector3 genLocalPosition;
    public Define.EnemyName enemyName;
}
[System.Serializable]
public class TimeWaveData : WaveData
{
    public int startTime;
    public int endTime;
}
[System.Serializable]
public class DistanceWaveData : WaveData
{
    public float distance;
}

[System.Serializable]
public class MentalWaveData : WaveData
{
    public int genMentalLevelOrMore;
}

[System.Serializable]
public class PresetWaveData : WaveData
{
    public GameObject enemyPreset;
}