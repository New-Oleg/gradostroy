
using UnityEngine;
using UnityEngine.InputSystem;

public class RoadMouseMovmentComponent : AMouseMovment
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
        if (_canBuild)
        {
            _inputActions.Movment.HouseMovment.performed -= OnMove;
            _inputActions.Movment.HouseMovment.canceled -= OnMove;

            gameObject.tag = "Road";
            Instantiate(gameObject);
            Destroy(GetComponent<Rigidbody>());
            Destroy(this);
        }
        else
        {
            Debug.Log("cказать что нельзя строить");
        }
    }

    protected override void SetBuildState(Collision c, bool state)
    {
        if (c.transform.CompareTag("Road") || c.transform.CompareTag("House"))
            _canBuild = state;
    }
}
