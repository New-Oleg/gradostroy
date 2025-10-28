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

    public string HouseId { get; }

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
        return (nowProvider().ToUniversalTime() - GetLast()) >= cooldown;
    }

    public TimeSpan TimeLeft()
    {
        var left = cooldown - (nowProvider().ToUniversalTime() - GetLast());
        return left > TimeSpan.Zero ? left : TimeSpan.Zero;
    }

    public bool TryClaim()
    {
        if (!CanClaim()) return false;
        receiver.GiveReward(resourceType, rewardAmount);
        SetLast(nowProvider().ToUniversalTime());
        return true;
    }

    // В случае, если нужно поменять параметры (редко нужно, но полезно)
    public void UpdateParams(int newAmount, TimeSpan newCooldown)
    {
        rewardAmount = Math.Max(1, newAmount);
        cooldown = newCooldown;
    }

    // Опция: принудительно сбросить время последнего получения (если нужно)
    public void ResetLastToNow()
    {
        SetLast(nowProvider().ToUniversalTime());
    }
}
