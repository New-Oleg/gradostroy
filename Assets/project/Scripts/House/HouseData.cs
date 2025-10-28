using System;
using UnityEngine;

[Serializable]
public class HouseData
{
    public string id;         // уникальный id экземпляра
    public Transform transform; // позиция объектта на сцене 
    public string buildingId; // id вида здания в BuildingDatabaseSO
    public bool iRedy;

    public HouseData() { }

    public HouseData(string id, string buildingId, Transform transform)
    {
        this.id = id;
        this.transform = transform;
        this.buildingId = buildingId;
    }
}
