using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class Map
{
    public int MapSize = 100;
    public int mechineCount = 3;
    int currentIndex = -10002;
    int moreBackBuildingIndex = -1000;
    int groundCount = 5;

    GameObject leftBuilding;
    GameObject centerBuilding;
    GameObject rightBuilding;

    GameObject moreBackLeftBuilding;
    GameObject moreBackCenterBuilding;
    GameObject moreBackRightBuilding;

    List<string> buildingPresetPathList = new List<string>();
    List<string> moreBackBuildingPresetPathList = new List<string>();

    float groundTerm;
    float yPosision = -8.91f;
    public float YPosition => yPosision;

    GameObject groundFolder;
    public Map(float groundTenm)
    {
        this.groundTerm = groundTenm;

        for(int i =0; i< mechineCount; i++)
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/VendingMechine");
            go.transform.position = new Vector3(MapSize / mechineCount * i, YPosition, 0);
        }
    }

    public void Update(Player player)
    {
        if (player == null) return;

        Vector3 pos = player.transform.position;
        //SetGround(GetIndex(pos.x));

        int mul = 4;

        int index = Mathf.RoundToInt(pos.x / (groundTerm * mul));
        if (index != moreBackBuildingIndex)
        {
            if (moreBackBuildingPresetPathList.Count > 0)
            {
                // 중간
                Random.InitState(Mathf.RoundToInt(index / moreBackBuildingPresetPathList.Count));
                int random = (int)(Random.value * 1000);

                if (moreBackCenterBuilding)
                    Managers.GetManager<ResourceManager>().Destroy(moreBackCenterBuilding);
                Vector3 position = new Vector3(index * groundTerm * mul, 0, 0);
                position.y = -2f;
                moreBackCenterBuilding = Managers.GetManager<ResourceManager>().Instantiate(moreBackBuildingPresetPathList[(random + index) % moreBackBuildingPresetPathList.Count]);
                // 왼쪽
                Random.InitState(Mathf.RoundToInt((index - 1) / moreBackBuildingPresetPathList.Count));
                random = (int)(Random.value * 1000);

                if (moreBackLeftBuilding)
                    Managers.GetManager<ResourceManager>().Destroy(moreBackLeftBuilding);
                position = new Vector3((index - 1) * groundTerm * mul, 0, 0);
                position.y = -2f;
                moreBackLeftBuilding = Managers.GetManager<ResourceManager>().Instantiate(moreBackBuildingPresetPathList[(random + index - 1) % moreBackBuildingPresetPathList.Count]);
                //오른쪽
                Random.InitState(Mathf.RoundToInt((index + 1) / moreBackBuildingPresetPathList.Count));
                random = (int)(Random.value * 1000);

                if (moreBackRightBuilding)
                    Managers.GetManager<ResourceManager>().Destroy(moreBackRightBuilding);
                position = new Vector3((index + 1) * groundTerm * mul, 0, 0);
                position.y = -2f;
                moreBackRightBuilding = Managers.GetManager<ResourceManager>().Instantiate(moreBackBuildingPresetPathList[(random + index + 1) % moreBackBuildingPresetPathList.Count]);
            }

            moreBackBuildingIndex = index;
        }

        float distacne = Camera.main.transform.position.x - moreBackBuildingIndex * groundTerm * mul;

        moreBackCenterBuilding.transform.position = new Vector3(moreBackBuildingIndex * groundTerm * mul + distacne / 2, 0, 0);
        moreBackRightBuilding.transform.position = new Vector3(moreBackCenterBuilding.transform.position.x + groundTerm * 2, 0, 0);
        moreBackLeftBuilding.transform.position = new Vector3(moreBackCenterBuilding.transform.position.x - groundTerm * 2, 0, 0);

    }


    public void SetGround(int index)
    {
        if (index == currentIndex) return;

        Random.InitState(Mathf.RoundToInt(index / groundCount));
        int groundRandom = (int)(Random.value * 1000);

        //List<int> indexList = grounds.Keys.ToList();

        //for (int i = 0; i < groundCount; i++)
        //{
        //    int groundIndex = index + ((i % 2) == 0 ? -1 : 1) * (Mathf.FloorToInt((i - 1) / 2) + 1) * (i == 0 ? 0 : 1);
        //    // int randomGroundTile = (groundRandom + groundIndex) % groundCount;

        //    if (!grounds.ContainsKey(groundIndex))
        //    {
        //        grounds.Add(groundIndex, Managers.GetManager<ResourceManager>().Instantiate("Ground"));
        //        grounds[groundIndex]?.transform.SetParent(groundFolder.transform);
        //        grounds[groundIndex].transform.position = new Vector3(groundTerm * groundIndex, YPosition, 0);
        //    }

        //    indexList.Remove(groundIndex);
        //}

        //for (int i = indexList.Count - 1; i >= 0; i--)
        //{
        //    Managers.GetManager<ResourceManager>().Destroy(grounds[indexList[i]]);
        //    grounds.Remove(indexList[i]);
        //}


        currentIndex = index;


        //if (buildingPresetPathList.Count > 0)
        //{

        //    // 중간
        //    Random.InitState(Mathf.RoundToInt(index / buildingPresetPathList.Count));
        //    int random = (int)(Random.value * 1000);

        //    if (centerBuilding)
        //        Managers.GetManager<ResourceManager>().Destroy(centerBuilding);
        //    Vector3 position = grounds[currentIndex].transform.position;
        //    position.y = Random.Range(-1f, -4f);
        //    centerBuilding = Managers.GetManager<ResourceManager>().Instantiate(buildingPresetPathList[(random + index) % buildingPresetPathList.Count]);
        //    centerBuilding.transform.parent = groundFolder.transform;
        //    centerBuilding.transform.position = position;

        //    // 왼쪽
        //    Random.InitState(Mathf.RoundToInt((index - 1) / buildingPresetPathList.Count));
        //    random = (int)(Random.value * 1000);

        //    if (leftBuilding)
        //        Managers.GetManager<ResourceManager>().Destroy(leftBuilding);
        //    position = grounds[currentIndex - 1].transform.position;
        //    position.y = Random.Range(-1f, -4f);
        //    leftBuilding = Managers.GetManager<ResourceManager>().Instantiate(buildingPresetPathList[(random + index - 1) % buildingPresetPathList.Count]);
        //    leftBuilding.transform.parent = groundFolder.transform;
        //    leftBuilding.transform.position = position;

        //    //오른쪽
        //    Random.InitState(Mathf.RoundToInt((index + 1) / buildingPresetPathList.Count));
        //    random = (int)(Random.value * 1000);

        //    if (rightBuilding)
        //        Managers.GetManager<ResourceManager>().Destroy(rightBuilding);
        //    position = grounds[currentIndex + 1].transform.position;
        //    position.y = Random.Range(-1f, -4f);
        //    rightBuilding = Managers.GetManager<ResourceManager>().Instantiate(buildingPresetPathList[(random + index + 1) % buildingPresetPathList.Count]);
        //    rightBuilding.transform.parent = groundFolder.transform;
        //    rightBuilding.transform.position = position;
        //}
    }

    //public void SetCenterGround(GameObject ground)
    //{
    //    grounds.Add(0, ground);

    //    for (int i = 1; i < groundCount; i++)
    //    {
    //        int groundIndex = ((i % 2) == 0 ? -1 : 1) * (Mathf.FloorToInt((i - 1) / 2) + 1);
    //        grounds.Add(groundIndex, Managers.GetManager<ResourceManager>().Instantiate("Ground"));
    //        grounds[groundIndex]?.transform.SetParent(groundFolder.transform);
    //        grounds[groundIndex].transform.position = new Vector3(groundTerm * groundIndex, YPosition, 0);
    //    }
    //}

    public void AddBuildingPreset(string path)
    {
        buildingPresetPathList.Add(path);
    }
    public void AddMoreBackBuildingPreset(string path)
    {
        moreBackBuildingPresetPathList.Add(path);
    }

    public int GetIndex(float x)
    {
        int index = Mathf.RoundToInt(x / (groundTerm));
        return index;
    }
}