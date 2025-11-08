using System;
using UnityEngine;

[Serializable]
public class HouseData
{
    public string id;                // уникальный id экземпляра
    public string buildingId;        // id вида здания
    public Transform transform;      // ссылка на объект
    public bool iRedy;               // готов к выдаче награды
    public bool isUnderConstruction; // идёт ли стройка
    public DateTime buildStartTime;  // когда началась стройка
    public double buildDurationHours; // время стройки в часах

    public HouseData() { }

    public HouseData(string id, string buildingId, Transform transform, double buildDurationHours)
    {
        this.id = id;
        this.buildingId = buildingId;
        this.transform = transform;
        this.buildDurationHours = buildDurationHours;
        this.isUnderConstruction = false;
        this.buildStartTime = DateTime.UtcNow;
    }
    // Проверяет, закончилась ли стройка (в часах)
    public bool IsConstructionComplete(Func<DateTime> nowProvider)
    {
        double elapsedHours = (nowProvider().ToUniversalTime() - buildStartTime).TotalHours;
        return elapsedHours >= buildDurationHours;
    }
    // Возвращает прогресс стройки от 0 до 1
    public double ConstructionProgress(Func<DateTime> nowProvider)
    {
        double elapsed = (nowProvider().ToUniversalTime() - buildStartTime).TotalHours;
        return Math.Clamp(elapsed / buildDurationHours, 0f, 1f);
    }
}
