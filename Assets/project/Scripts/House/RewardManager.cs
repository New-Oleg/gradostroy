using System;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class RewardManager : MonoBehaviour
{
    [Header("Houses (instances)")]
    [SerializeField] private List<HouseData> houses = new List<HouseData>();

    [Header("Data")]
    [SerializeField] private BuildingDatabaseSO buildingDatabase; // assign in inspector

    [Header("Settings")]
    [SerializeField] private float checkInterval = 1f; // seconds

    private Dictionary<string, RewardService> services = new Dictionary<string, RewardService>();
    private float timer;
    private IGiveReward rewardReceiver;
    private Func<DateTime> nowProvider = () => DateTime.UtcNow;

    private void Awake()
    {
        // Try to get IGiveReward on same GameObject (optional)
        rewardReceiver = GetComponent<IGiveReward>();
        if (rewardReceiver == null)
            Debug.LogWarning("RewardManager: IGiveReward not found on this GameObject. Call Initialize(...) with receiver.");

        if (buildingDatabase == null)
            Debug.LogError("RewardManager: BuildingDatabaseSO is not assigned!");

        InitializeServices();
    }

    // Call manually to provide your IGiveReward and optional time provider (e.g. server time)
    public void Initialize(IGiveReward receiver, Func<DateTime> nowProvider = null)
    {
        rewardReceiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
        if (nowProvider != null) this.nowProvider = nowProvider;
        InitializeServices();
    }

    private void InitializeServices()
    {
        services.Clear();
        if (buildingDatabase == null) return;

        // ensure DB map is refreshed
        buildingDatabase.Refresh();

        foreach (var h in houses)
        {
            if (h == null)
            {
                Debug.LogWarning("RewardManager: null HouseData in list.");
                continue;
            }

            var so = buildingDatabase.GetById(h.buildingId);
            if (so == null)
            {
                Debug.LogWarning($"RewardManager: BuildingSO not found for id '{h.buildingId}' (house '{h.id}'). Skipping.");
                continue;
            }

            if (services.ContainsKey(h.id))
            {
                Debug.LogWarning($"RewardManager: duplicate house id '{h.id}'. Skipping.");
                continue;
            }

            var svc = new RewardService(
                h.id,
                so.resourceType,
                so.rewardAmount,
                so.Cooldown,
                rewardReceiver ?? throw new InvalidOperationException("IGiveReward not provided to RewardManager"),
                nowProvider
            );

            services[h.id] = svc;
        }
    }

    private void Update()
    {
        if (services.Count == 0) return;
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        // Простой обход всех сервисов. При большом N можно оптимизировать (пакетами / очередь).
        foreach (var kv in services)
        {
            var svc = kv.Value;
            if (svc.CanClaim())
            {
                svc.TryClaim();
                Debug.Log($"[RewardManager] House {svc.HouseId} auto-claimed.");
            }
        }
    }

    // UI / manual calls
    public bool TryClaimHouse(string houseId)
    {
        if (services.TryGetValue(houseId, out var svc))
            return svc.TryClaim();

        Debug.LogWarning($"TryClaimHouse: house '{houseId}' not found.");
        return false;
    }

    public TimeSpan GetTimeLeft(string houseId)
    {
        if (services.TryGetValue(houseId, out var svc))
            return svc.TimeLeft();
        return TimeSpan.Zero;
    }

    public void AddHouse(BuildingSO so)
    {

        // Генерируем уникальный id для конкретного экземпляра
        string instanceId = Guid.NewGuid().ToString();

        Instantiate(so.BuildPref);

        // Создаём HouseData для сериализуемого списка (сохраняется в houses)
        var houseData = new HouseData(instanceId, so.id, so.displayName);
        houses.Add(houseData);

        // Создаём RewardService на основе данных из ScriptableObject
        var service = new RewardService(
            instanceId,
            so.resourceType,
            so.rewardAmount,
            so.Cooldown,
            rewardReceiver,
            nowProvider
        ); 

        // Добавляем в словарь активных сервисов
        services[instanceId] = service;

        // Опционально сбрасываем таймер (чтобы дом не выдал награду сразу)
        service.ResetLastToNow();

        Debug.Log($"AddHouse: добавлен дом '{so.displayName}' (id: {instanceId})");
    }

    public IReadOnlyDictionary<string, RewardService> Services => services;
}
