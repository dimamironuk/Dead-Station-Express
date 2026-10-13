using UnityEngine;
using UnityEngine.EventSystems;

public class TrainCameraController : MonoBehaviour
{
    [Header("Train References")]
    [SerializeField] private Transform _engineTransform;
    [SerializeField] private float _wagonOffset = 3.2f;

    [Header("Scroll Settings")]
    [SerializeField] private float _dragSensitivity = 0.03f;
    [SerializeField] private float _smoothSpeed = 15f;

    [Header("Camera Limits (Padding)")]
    [SerializeField] private float _frontPadding = 5f;
    [SerializeField] private float _backPadding = 15f;

    private float _targetZ;
    private int _wagonCount = 0;
    private Vector2 _lastInputPosition;

    private void Start()
    {
        _targetZ = transform.position.z;
    }

    public void SetWagonCount(int count)
    {
        _wagonCount = count;
    }

    private void Update()
    {
        HandleInput();
        ApplySmoothMovement();
    }

    private void HandleInput()
    {
        float deltaY = 0f;
        bool hasInput = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);


            if (touch.phase == TouchPhase.Began)
            {
                _lastInputPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                deltaY = touch.position.y - _lastInputPosition.y;
                _lastInputPosition = touch.position;
                hasInput = true;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                _lastInputPosition = Input.mousePosition;
            }
            else
            {
                Vector2 currentMouse = Input.mousePosition;
                deltaY = currentMouse.y - _lastInputPosition.y;
                _lastInputPosition = currentMouse;
                hasInput = true;
            }
        }

        if (hasInput)
        {
            _targetZ -= deltaY * _dragSensitivity;
        }

        float engineZ = _engineTransform != null ? _engineTransform.position.z : transform.position.z;

        float maxAllowedZ = engineZ + _frontPadding;
        float minAllowedZ = engineZ - (_wagonCount * Mathf.Abs(_wagonOffset)) - _backPadding;

        if (minAllowedZ > maxAllowedZ)
        {
            float temp = minAllowedZ;
            minAllowedZ = maxAllowedZ;
            maxAllowedZ = temp;
        }

        _targetZ = Mathf.Clamp(_targetZ, minAllowedZ, maxAllowedZ);
    }

    private void ApplySmoothMovement()
    {
        Vector3 position = transform.position;
        position.z = Mathf.Lerp(position.z, _targetZ, Time.deltaTime * _smoothSpeed);
        transform.position = position;
    }
}