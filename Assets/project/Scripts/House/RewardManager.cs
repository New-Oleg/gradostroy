using System;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class RewardManager : MonoBehaviour
{
    [Header("Houses (instances)")]
    [SerializeField] private List<HouseData> houses = new List<HouseData>();

    [Header("Data")]
    [SerializeField] private BuildingDatabaseSO buildingDatabase; 

    [Header("Settings")]
    [SerializeField] private float checkInterval = 1f; // seconds

    private Dictionary<string, RewardService> services = new Dictionary<string, RewardService>();
    private float timer;
    private IGiveReward rewardReceiver;
    private Func<DateTime> nowProvider = () => DateTime.UtcNow;

    public static event Action<HouseData> CanTackeRevard;
    private void Awake()
    {
        // Try to get IGiveReward on same GameObject (optional)
        rewardReceiver = GetComponent<IGiveReward>();
        if (rewardReceiver == null)
            Debug.LogWarning("RewardManager: IGiveReward not found on this GameObject. Call Initialize(...) with receiver.");

        if (buildingDatabase == null)
            Debug.LogError("RewardManager: BuildingDatabaseSO is not assigned!");

        InitializeServices();

        InvokeRepeating(nameof(CheckHouse), checkInterval, checkInterval);
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
                rewardReceiver,
                nowProvider
            );

            services[h.id] = svc;
        }
    }

    private void CheckHouse()
    {
        if (services.Count == 0) return;
        foreach (var kv in services)
        {
            var svc = kv.Value;
            if (svc.CanClaim())
            {
                HouseData h = houses.Find(h => h.id == svc.HouseId);
                if(!h.iRedy){
                    h.iRedy = true;
                    CanTackeRevard.Invoke(h);
                }
            }
        }
    }

    // UI / manual calls
    public void TryClaimHouse(string houseId)
    {
        if (services.TryGetValue(houseId, out var svc))
             svc.TryClaim();

        Debug.LogWarning($"TryClaimHouse: house '{houseId}' not found.");
        return;
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

        GameObject h = Instantiate(so.BuildPref);

        if(YG2.envir.isDesktop)
            h.AddComponent<MouseMovmentComponent>();

        // Создаём HouseData (сохраняется в houses)
        var houseData = new HouseData(instanceId, so.id, h.transform);
        houses.Add(houseData);

        // Создаём RewardService
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
