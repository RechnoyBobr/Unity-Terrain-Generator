using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;
    public float rotationSpeed = 100f;
    public float mouseRotationSpeed = 2f;
    public float smoothTime = 0.1f;
    
    [Header("Height Settings")]
    public float minHeight = 5f;
    public float maxHeight = 100f;
    public float heightChangeSpeed = 20f;
    
    [Header("Zoom Settings")]
    public float minZoom = 10f;
    public float maxZoom = 100f;
    public float zoomSpeed = 5f;

    private float currentRotation = 0f;
    private float currentPitch = 0f;
    private float targetRotation = 0f;
    private float targetPitch = 0f;
    private float currentZoom;
    private float targetZoom;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;

    void Start()
    {
        targetPosition = transform.position;
        currentZoom = transform.position.y;
        targetZoom = currentZoom;
        targetRotation = transform.eulerAngles.y;
        targetPitch = transform.eulerAngles.x;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleHeight();
        HandleZoom();
        UpdateCameraPosition();
    }

    void HandleMovement()
    {
        float horizontal = -Input.GetAxis("Horizontal");
        float vertical = -Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
        if (moveDirection != Vector3.zero)
        {
            // Преобразуем направление движения относительно текущего поворота камеры
            moveDirection = Quaternion.Euler(0, currentRotation, 0) * moveDirection;
            targetPosition += moveDirection * moveSpeed * Time.deltaTime;
        }
    }

    void HandleRotation()
    {
        // Вращение с помощью клавиш Q/E
        if (Input.GetKey(KeyCode.Q))
        {
            targetRotation -= rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            targetRotation += rotationSpeed * Time.deltaTime;
        }

        // Вращение с помощью мыши (зажатая правая кнопка)
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseRotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * mouseRotationSpeed;

            targetRotation += mouseX;
            targetPitch -= mouseY; // Отрицательное значение для правильного направления
            targetPitch = Mathf.Clamp(targetPitch, -80f, 80f); // Ограничиваем угол обзора вверх/вниз
        }

        // Плавно интерполируем текущий поворот
        currentRotation = Mathf.LerpAngle(currentRotation, targetRotation, Time.deltaTime / smoothTime);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, Time.deltaTime / smoothTime);
    }

    void HandleHeight()
    {
        if (Input.GetKey(KeyCode.R))
        {
            targetZoom = Mathf.Clamp(targetZoom + heightChangeSpeed * Time.deltaTime, minZoom, maxZoom);
        }
        if (Input.GetKey(KeyCode.F))
        {
            targetZoom = Mathf.Clamp(targetZoom - heightChangeSpeed * Time.deltaTime, minZoom, maxZoom);
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom = Mathf.Clamp(targetZoom - scroll * zoomSpeed, minZoom, maxZoom);
        }
    }

    void UpdateCameraPosition()
    {
        // Вычисляем позицию камеры на основе поворота и зума
        float angle = currentRotation * Mathf.Deg2Rad;
        float pitch = currentPitch * Mathf.Deg2Rad;
        
        // Вычисляем смещение камеры с учетом наклона
        float horizontalDistance = Mathf.Cos(pitch) * targetZoom;
        float verticalDistance = Mathf.Sin(pitch) * targetZoom;
        
        Vector3 offset = new Vector3(
            Mathf.Sin(angle) * horizontalDistance,
            verticalDistance,
            Mathf.Cos(angle) * horizontalDistance
        );

        // Обновляем позицию камеры
        Vector3 newPosition = targetPosition + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref currentVelocity, smoothTime);

        // Направляем камеру на целевую точку
        transform.LookAt(targetPosition);
    }
} 