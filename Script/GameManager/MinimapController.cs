using UnityEngine;
using UnityEngine.UI;

public class MinimapController : MonoBehaviour
{
    [Header("Minimap Settings")]
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private RawImage minimapDisplay;
    [SerializeField] private float zoomLevel = 10f;
    [SerializeField] private int minimapSize = 256; // Kích thước cố định của minimap
    
    [Header("Player Settings")]
    [SerializeField] private Transform player;
    [SerializeField] private float heightOffset = 30f;
    [SerializeField] private float maxMoveDistance = 100f; // Giới hạn khoảng cách di chuyển
    
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask minimapLayer;
    
    private RenderTexture minimapTexture;
    private bool isRotating = false;
    private Vector3 lastPlayerPosition;

    private void Start()
    {
        // Kiểm tra reference cơ bản
        if (minimapCamera == null || minimapDisplay == null || player == null)
        {
            Debug.LogError("Thiếu reference cần thiết!");
            return;
        }

        // Tìm Main Camera nếu chưa được gán
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // Thiết lập minimap với kích thước cố định
        minimapTexture = new RenderTexture(minimapSize, minimapSize, 16);
        minimapCamera.targetTexture = minimapTexture;
        minimapDisplay.texture = minimapTexture;
        
        // Thiết lập camera
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = zoomLevel;
        minimapCamera.enabled = true;
        minimapCamera.cullingMask = minimapLayer;
        
        // Thiết lập depth
        mainCamera.depth = 0;
        minimapCamera.depth = 1;
        
        // Lưu vị trí player ban đầu
        lastPlayerPosition = player.position;
        
        // Thiết lập vị trí camera
        Vector3 initialPosition = player.position;
        initialPosition.y = heightOffset;
        minimapCamera.transform.position = initialPosition;
        minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void LateUpdate()
    {
        if (player == null) return;
        
        // Cập nhật vị trí camera theo player
        Vector3 newPosition = player.position;
        newPosition.y = heightOffset;
        
        // Kiểm tra xem vị trí mới có hợp lệ không
        if (float.IsNaN(newPosition.x) || float.IsNaN(newPosition.y) || float.IsNaN(newPosition.z))
        {
            Debug.LogWarning("Vị trí player không hợp lệ!");
            return;
        }
        
        // Kiểm tra khoảng cách di chuyển
        float moveDistance = Vector3.Distance(lastPlayerPosition, newPosition);
        if (moveDistance > maxMoveDistance)
        {
            Debug.LogWarning($"Player di chuyển quá nhanh: {moveDistance} units");
            return;
        }
        
        // Cập nhật vị trí camera
        minimapCamera.transform.position = newPosition;
        lastPlayerPosition = newPosition;
        
        // Xoay camera theo player nếu được bật
        if (isRotating)
        {
            minimapCamera.transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
        
        // Kiểm tra và cập nhật RenderTexture nếu cần
        if (minimapTexture == null || !minimapTexture.IsCreated())
        {
            Debug.LogWarning("RenderTexture không hợp lệ, tạo lại...");
            minimapTexture = new RenderTexture(minimapSize, minimapSize, 16);
            minimapCamera.targetTexture = minimapTexture;
            minimapDisplay.texture = minimapTexture;
        }
    }

    public void ToggleRotation() => isRotating = !isRotating;

    public void SetZoom(float zoom)
    {
        zoomLevel = zoom;
        minimapCamera.orthographicSize = zoomLevel;
    }

    private void OnDestroy()
    {
        if (minimapTexture != null)
        {
            minimapTexture.Release();
            Destroy(minimapTexture);
        }
    }
}
