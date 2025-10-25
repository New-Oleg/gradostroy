using System;

[Serializable]
public class HouseData
{
    public string id;         // уникальный id экземпл€ра, напр. "house_123"
    public string buildingId; // id вида здани€ в BuildingDatabaseSO
    public string displayName; // опционально: удобное им€ дл€ UI

    public HouseData() { }

    public HouseData(string id, string buildingId, string displayName = null)
    {
        this.id = id;
        this.buildingId = buildingId;
        this.displayName = displayName;
    }
}
