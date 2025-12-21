using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class HouseManager : MonoBehaviour
{
    [Header("Houses (instances)")]
    [SerializeField] private static List<HouseData> houses = new List<HouseData>();

    [Header("Data")]
    [SerializeField] private BuildingDatabaseSO buildingDatabase;

    [Header("Settings")]
    [SerializeField] private float checkIntervalSeconds = 1f;

    [Header("Road Check")]
    [SerializeField] private float roadCheckRadius = 2f;

    private Dictionary<string, RewardService> services = new Dictionary<string, RewardService>();
    private IGiveReward rewardReceiver;
    private static Func<DateTime> nowProvider = () => DateTime.UtcNow;

    public static event Action<HouseData> CanTakeReward;
    public static event Action<HouseData> OnConstructionStarted;
    public static event Action<HouseData> OnConstructionCompleted;

    private void Awake()
    {
        rewardReceiver = GetComponent<IGiveReward>();

        InitializeServices();

        RunHouseLoops().Forget();
    }

    private void InitializeServices()
    {
        services.Clear();

        buildingDatabase.Refresh();

        foreach (var h in houses)
        {
            if (h == null || h.isUnderConstruction) continue;

            var so = buildingDatabase.GetById(h.buildingId);
            if (so == null)
            {
                Debug.LogWarning($"HouseManager: BuildingSO not found for '{h.buildingId}'");
                continue;
            }

            if (services.ContainsKey(h.id)) continue;

            var svc = new RewardService(
                h.id,
                so.resourceType,
                so.rewardAmount,
                so.Cooldown,
                rewardReceiver,
                nowProvider
            );

            svc.OnRewardReady += HandleRewardReady;
            services[h.id] = svc;

            if (IsHouseConnected(h))
            {
                svc.StartTimer();
                Debug.Log($"[HouseManager] House '{h.id}' connected to road -> timer started");
            }
        }
    }

    private async UniTaskVoid RunHouseLoops()
    {
        while (true)
        {
            try
            {
                // 1. Проверка стройки
                foreach (var house in houses)
                {
                    if (house == null) continue;
                    if (house.isUnderConstruction && house.IsConstructionComplete(nowProvider))
                        FinishConstruction(house);
                }

                // 2. Проверка соединения с дорогой и таймера
                foreach (var house in houses)
                {
                    if (house == null || house.isUnderConstruction) continue;
                    if (services.TryGetValue(house.id, out var svc))
                    {
                        if (!svc.IsTimerStarted && IsHouseConnected(house))
                        {
                            svc.StartTimer();
                            Debug.Log($"[HouseManager] House '{house.id}' connected now -> timer started");
                        }
                        else if (svc.IsTimerStarted && !IsHouseConnected(house))
                        {
                            svc.StapTimer();
                            Debug.Log($"[HouseManager] Timer stopped for house '{house.id}' (road disconnected)");
                        }
                    }
                }

                // 3. Проверка готовности наград
                foreach (var svc in services.Values)
                    svc.Update();
            }
            catch (Exception ex)
            {
                Debug.LogError($"HouseManager Loop Error: {ex}");
            }

            await UniTask.Delay(TimeSpan.FromSeconds(checkIntervalSeconds));
        }
    }

    private void HandleRewardReady(string houseId)
    {
        var house = houses.Find(h => h.id == houseId);
        if (house == null) return;

        house.iRedy = true;
        CanTakeReward.Invoke(house);
    }

    private void FinishConstruction(HouseData house)
    {
        var so = buildingDatabase.GetById(house.buildingId);
        Vector3 pos = house.transform.position;
        Quaternion rot = house.transform.rotation;
        Destroy(house.transform.gameObject);
        house.transform = Instantiate(so.BuildPref, pos, rot).transform;

        house.isUnderConstruction = false;

        var svc = new RewardService(
            house.id,
            so.resourceType,
            so.rewardAmount,
            so.Cooldown,
            rewardReceiver,
            nowProvider
        );
        svc.OnRewardReady += HandleRewardReady;
        services[house.id] = svc;

        if (IsHouseConnected(house))
        {
            svc.StartTimer();
            Debug.Log($"[HouseManager] House '{house.id}' connected to road -> timer started");
        }

        OnConstructionCompleted?.Invoke(house);
    }

    private bool IsHouseConnected(HouseData house)
    {
        try
        {
            Collider[] hits = Physics.OverlapSphere(house.transform.position, roadCheckRadius);
            foreach (var hit in hits)
                if (hit.CompareTag("Road"))
                    return true;
        }
        catch
        {
            houses.Remove(house);
            return false;
        }

        return false;
    }

    public void TryClaimHouse(string houseId)
    {
        if (services.TryGetValue(houseId, out var svc))
        {
            if (svc.TryClaim())
            {
                var house = houses.Find(h => h.id == houseId);
                if (house != null)
                    house.iRedy = false;
            }
        }
    }

    public TimeSpan GetTimeLeft(string houseId)
    {
        if (services.TryGetValue(houseId, out var svc))
            return svc.TimeLeft();
        return TimeSpan.Zero;
    }

    public void AddHouse(BuildingSO so)
    {
        if (so == null) return;

        string instanceId = Guid.NewGuid().ToString();
        GameObject building = Instantiate(so.BuildingPref);

        if (YG2.envir.isDesktop)
            building.AddComponent<HouseMouseMovmentComponent>();

        var houseData = new HouseData(instanceId, so.id, building.transform, so.BuildingTime)
        {
            isUnderConstruction = false
        };

        houses.Add(houseData);
        Debug.Log($"[HouseManager] Added house '{so.displayName}' ({instanceId}) - construction not started");
    }

    public static void StartConstruction()
    {
        if (houses.Count == 0) return;

        var house = houses[houses.Count - 1];
        if (house.isUnderConstruction)
        {
            Debug.LogWarning($"StartConstruction: House '{house.id}' already under construction");
            return;
        }

        house.isUnderConstruction = true;
        house.buildStartTime = nowProvider();

        Debug.Log($"[HouseManager] Construction started for house '{house.id}'");
        OnConstructionStarted?.Invoke(house);
    }

    public void BuildRoad(GameObject roadPrefab)
    {
        GameObject r = Instantiate(roadPrefab);
        if (YG2.envir.isDesktop)
            r.AddComponent<RoadMouseMovmentComponent>();
    }

    public IReadOnlyDictionary<string, RewardService> Services => services;
}
