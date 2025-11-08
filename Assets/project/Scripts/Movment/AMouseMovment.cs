using InputActionns;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public abstract class AMouseMovment : MonoBehaviour
{
    protected Camera mainCamera;
    protected Tilemap tilemap;
    protected InputActionnsMouse _inputActions;
    protected bool _canBuild = true;

    private void Awake()
    {
        _inputActions = new InputActionnsMouse();

        mainCamera = Camera.main;

        tilemap = FindFirstObjectByType<Tilemap>();
    }

    private void OnEnable()
    {
        _inputActions.Movment.HouseMovment.Enable();
        _inputActions.Movment.HouseMovment.performed += OnMove;
        _inputActions.Movment.HouseMovment.canceled += OnMove;


        _inputActions.Movment.KeyDetect.Enable();
        _inputActions.Movment.KeyDetect.canceled += OnClick;
    }

    private void OnDisable()
    {
        _inputActions.Movment.HouseMovment.Disable();
        _inputActions.Movment.HouseMovment.performed -= OnMove;
        _inputActions.Movment.HouseMovment.canceled -= OnMove;


        _inputActions.Movment.KeyDetect.Disable();
        _inputActions.Movment.KeyDetect.canceled -= OnClick;

    }

    protected virtual void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = ctx.ReadValue<Vector2>();

        float yDistance = Mathf.Abs(mainCamera.transform.position.y - transform.position.y);

        Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, yDistance);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPoint);

        worldPos.y = transform.position.y;

        Vector3Int cell = tilemap.WorldToCell(worldPos);
        Vector3 center = tilemap.GetCellCenterWorld(cell);

        center.y = transform.position.y;


        transform.position = center;
    }

    protected virtual void OnClick(InputAction.CallbackContext ctx)
    {
        switch (ctx.control.name)
        {
            case "leftButton":
                leftButton();
                break;

            case "rightButton":
                transform.rotation *= Quaternion.Euler(0, 90, 0);
                break;

            case "middleButton":
                Destroy(gameObject);

                break;
        }
    }

    protected virtual void leftButton()
    {
        if (_canBuild)
        {
            _inputActions.Movment.HouseMovment.performed -= OnMove;
            _inputActions.Movment.HouseMovment.canceled -= OnMove;

            Destroy(GetComponent<Rigidbody>());
            Destroy(this);
        }
        else
        {
            Debug.Log("cказать что нельзя строить");
        }
    }


    protected virtual void SetBuildState(Collision c, bool state)
    {
        if (c.transform.CompareTag("House"))
            _canBuild = state;

    }

    private void OnCollisionEnter(Collision c) => SetBuildState(c, false);
    private void OnCollisionExit(Collision c) => SetBuildState(c, true);

}
