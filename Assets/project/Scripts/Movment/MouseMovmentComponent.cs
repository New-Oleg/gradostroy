using UnityEngine;
using InputActionns;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class MouseMovmentComponent : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Tilemap tilemap;
    [SerializeField]
    private float cellSize;

    private InputActionnsMouse _inputActions; 

    private void Awake()
    {
        _inputActions = new InputActionnsMouse();
    }

    private void OnEnable()
    {
        _inputActions.Movment.HouseMovment.Enable();
        _inputActions.Movment.HouseMovment.performed += OnMove;
        _inputActions.Movment.HouseMovment.canceled += OnMove;

        _inputActions.Movment.KeyDetect.performed += OnClick;
        _inputActions.Movment.KeyDetect.canceled += OnClick;
    }

    private void OnDisable()
    {
        _inputActions.Movment.HouseMovment.Disable();
        _inputActions.Movment.HouseMovment.performed -= OnMove;
        _inputActions.Movment.HouseMovment.canceled -= OnMove;

        _inputActions.Movment.KeyDetect.performed -= OnClick;
        _inputActions.Movment.KeyDetect.canceled -= OnClick;


    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 screenPos = ctx.ReadValue<Vector2>();

        // Получаем расстояние от камеры до объекта по оси Y (высоте)
        float yDistance = Mathf.Abs(mainCamera.transform.position.y - transform.position.y);

        // Переводим экранные координаты в мировые
        Vector3 screenPoint = new Vector3(screenPos.x, screenPos.y, yDistance);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPoint);

        // Фиксируем Y на высоте объекта
        worldPos.y = transform.position.y;

        // Прямо конвертируем в клетку Tilemap (если Tilemap в XZ)
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        Vector3 center = tilemap.GetCellCenterWorld(cell);

        // Возвращаем в позицию с корректировкой по высоте
        center.y = transform.position.y;


        transform.position = CheckNextPoss(center);
    }

    private Vector3 CheckNextPoss(Vector3 currentPos)
    {
        Vector3 newPos = currentPos;


        // Ограничиваем по X и Z
        newPos.x = Mathf.Clamp(newPos.x, -20, 20);
        newPos.z = Mathf.Clamp(newPos.z, -20, 20);

        // Y остаётся фиксированной высотой
        return newPos;
    }

    private void OnClick(InputAction.CallbackContext ctx)
    {
        
    }

}
