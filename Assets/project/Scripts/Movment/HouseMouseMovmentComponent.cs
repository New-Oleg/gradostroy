
using UnityEngine;
using UnityEngine.InputSystem;

public class HouseMouseMovmentComponent : AMouseMovment
{

    protected override void OnMove(InputAction.CallbackContext ctx)
    {
        base.OnMove(ctx);
    }

    protected override void OnClick(InputAction.CallbackContext ctx)
    {
        base.OnClick(ctx);
    }
    protected override void leftButton()
    {
        base.leftButton();
        HouseManager.StartConstruction();
    }
    protected override void SetBuildState(Collision c, bool state)
    {
        base.SetBuildState(c, state);
    }

}
