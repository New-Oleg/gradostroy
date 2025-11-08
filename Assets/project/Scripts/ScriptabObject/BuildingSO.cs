using Enums;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingSO", menuName = "Scriptable Objects/BuildingSO")]
public class BuildingSO : ScriptableObject
{
    [Header("Identity")]
    public GameObject BuildPref;      // префаб здания
    public string id;                 // уникальный id, например "house_small", "factory_1"
    public string displayName;        // читаемое имя для UI

    [Header("Building")]
    public double BuildingTime; // время строительства 
    public GameObject BuildingPref; // префаб строительства сдания

    [Header("Reward")]
    public RecursType resourceType;   // какой ресурс выдаёт
    public int rewardAmount;      // фиксированное количество при выдаче
    public double cooldownHours;  // фиксированный интервал в часах

    public TimeSpan Cooldown => TimeSpan.FromHours(Math.Max(0.0, cooldownHours));
}
