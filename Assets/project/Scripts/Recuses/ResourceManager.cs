using Enums;
using UnityEngine;

public class ResourceManager : MonoBehaviour, IGiveReward
{
    public int peoples;
    public int food;
    public int electricity;
    public int science;

    public void GiveReward(RecursType rt, int amount)
    {
        switch (rt)
        {
            case RecursType.Peoples: peoples += amount; break;
            case RecursType.Food: food += amount; break;
            case RecursType.Electricity: electricity += amount; break;
            case RecursType.Science: science += amount; break;
        }

        Debug.Log($"ResourceManager: Gave {amount} {rt} (totals: P{peoples} F{food} E{electricity} S{science})");
        // «десь можно подн€ть событие на UI и т.д.
    }
}
