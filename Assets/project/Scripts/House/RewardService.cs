using System;
using Enums;
using UnityEngine;

public class RewardService
{
    private readonly string prefsKey;
    private readonly Func<DateTime> nowProvider;
    private readonly IGiveReward receiver;

    private readonly RecursType resourceType;
    private int rewardAmount;
    private TimeSpan cooldown;

    private bool timerStarted = false;
    private DateTime startTime;
    private bool wasReadyLastFrame = false;

    public string HouseId { get; }

    //  Событие, когда можно выдать награду
    public event Action<string> OnRewardReady;

    public RewardService(string houseId, RecursType resourceType, int rewardAmount, TimeSpan cooldown, IGiveReward receiver, Func<DateTime> nowProvider = null)
    {
        HouseId = houseId;
        this.resourceType = resourceType;
        this.rewardAmount = rewardAmount;
        this.cooldown = cooldown;
        this.receiver = receiver;
        this.nowProvider = nowProvider ?? (() => DateTime.UtcNow);
        this.prefsKey = $"House_{HouseId}_LastReward";
    }

    private DateTime GetLast()
    {
        if (!PlayerPrefs.HasKey(prefsKey)) return DateTime.MinValue;
        if (long.TryParse(PlayerPrefs.GetString(prefsKey), out long ticks))
            return new DateTime(ticks, DateTimeKind.Utc);
        return DateTime.MinValue;
    }

    private void SetLast(DateTime utc)
    {
        PlayerPrefs.SetString(prefsKey, utc.ToUniversalTime().Ticks.ToString());
        PlayerPrefs.Save();
    }

    public bool CanClaim()
    {
        if (!timerStarted) return false;

        var last = GetLast();
        if (last == DateTime.MinValue)
            last = startTime;

        return (nowProvider().ToUniversalTime() - last) >= cooldown;
    }

    public TimeSpan TimeLeft()
    {
        if (!timerStarted) return cooldown;

        var last = GetLast();
        if (last == DateTime.MinValue)
            last = startTime;

        var left = cooldown - (nowProvider().ToUniversalTime() - last);
        return left > TimeSpan.Zero ? left : TimeSpan.Zero;
    }

    public bool TryClaim()
    {
        if (!CanClaim()) return false;
        receiver.GiveReward(resourceType, rewardAmount);
        SetLast(nowProvider().ToUniversalTime());
        wasReadyLastFrame = false; // сбрасываем флаг готовности
        return true;
    }

    public void StartTimer()
    {
        startTime = nowProvider().ToUniversalTime();
        timerStarted = true;
        wasReadyLastFrame = false;
    }

    public void StapTimer()
    {
        timerStarted = false;
        wasReadyLastFrame = false;
    }

    public bool IsTimerStarted => timerStarted;

    public void UpdateParams(int newAmount, TimeSpan newCooldown)
    {
        rewardAmount = Math.Max(1, newAmount);
        cooldown = newCooldown;
    }

    public void ResetLastToNow()
    {
        SetLast(nowProvider().ToUniversalTime());
    }

    //  Метод вызывается из HouseManager
    public void Update()
    {
        if (!timerStarted) return;

        bool ready = CanClaim();

        // событие вызывается только при первом переходе в "готов"
        if (ready && !wasReadyLastFrame)
        {
            wasReadyLastFrame = true;
            OnRewardReady?.Invoke(HouseId);
        }
        else if (!ready)
        {
            wasReadyLastFrame = false;
        }
    }
}
