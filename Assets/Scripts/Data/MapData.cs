using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Map", menuName = "AddData/Create MapData", order = 1)]

public class MapData : ScriptableObject
{
    public string sceneName;
    public List<DistanceWaveData> distanceWave;
    public List<BossWaveData> bossWave;
    public List<MentalWaveData> mentalWave;
    public float mapSize;
    public MapData nextMapData;

    public List<ItemData> shopItemDataList;
    public List<GameObject> randomEvent;
    public float randomEventInterval = 200;

    public float initMutifly;
    public float multiflyInterval;
    public float addMultifly;
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
    public GameObject enemyPreset;
}
[System.Serializable]
public class DistanceWaveData : WaveData
{
    public float startDistance;
    public float endDistance;
}
[System.Serializable]
public class BossWaveData : WaveData
{
    public float distance;
}

[System.Serializable]
public class MentalWaveData : WaveData
{
    public int genMentalLevelMore;
    public int genMentalLevelLess;
}