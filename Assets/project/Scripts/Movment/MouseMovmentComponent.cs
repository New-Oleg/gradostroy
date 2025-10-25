using UnityEngine;
using InputActionns;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

public class MouseMovmentComponent : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Tilemap tilemap;

    private InputActionnsMouse _inputActions; 

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

    private void OnMove(InputAction.CallbackContext ctx)
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

    private void OnClick(InputAction.CallbackContext ctx)
    {
        switch (ctx.control.name)
        {
            case "leftButton":

                _inputActions.Movment.HouseMovment.performed -= OnMove;
                _inputActions.Movment.HouseMovment.canceled -= OnMove;

                Destroy(this);
                break;

            case "rightButton":
                transform.rotation *= Quaternion.Euler(0, 90, 0);
                break;

        }
    }

}
