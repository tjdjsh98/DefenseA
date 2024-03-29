using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "Create Map", menuName = "AddData/Create MapData", order = 1)]

public class MapData : ScriptableObject
{
    public string sceneName;
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
    public float hpMultiply;

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