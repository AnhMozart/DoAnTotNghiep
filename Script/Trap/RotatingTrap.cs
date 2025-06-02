using UnityEngine;

/// <summary>
/// Script điều khiển bẫy xoay tròn di chuyển qua lại các điểm định sẵn
/// </summary>
public class RotatingTrap : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform[] waypoints; // Các điểm di chuyển của bẫy
    [SerializeField] private float moveSpeed = 5f; // Tốc độ di chuyển của bẫy
    [SerializeField] private float rotationSpeed = 360f; // Tốc độ xoay của bẫy (độ/giây)
    [SerializeField] private float waitTime = 1f; // Thời gian chờ tại mỗi điểm
    [SerializeField] private bool loop = true; // Có lặp lại chu trình di chuyển không

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true; // Hiển thị đường đi trong Scene view
    [SerializeField] private Color gizmoColor = Color.red; // Màu của đường đi

    private int currentWaypointIndex; // Chỉ số của điểm di chuyển hiện tại
    private float waitCounter; // Đếm thời gian chờ
    private bool isWaiting; // Trạng thái đang chờ
    private Vector3 targetPosition; // Vị trí đích cần di chuyển đến
    private const float ARRIVAL_THRESHOLD = 0.1f; // Khoảng cách tối thiểu để xác nhận đã đến điểm đích

    /// <summary>
    /// Khởi tạo bẫy và kiểm tra tính hợp lệ của các điểm di chuyển
    /// </summary>
    private void Start()
    {
        if (!ValidateWaypoints()) return;
        
        // Khởi tạo vị trí ban đầu tại điểm đầu tiên
        currentWaypointIndex = 0;
        transform.position = waypoints[0].position;
        targetPosition = waypoints[0].position;
    }

    /// <summary>
    /// Kiểm tra tính hợp lệ của các điểm di chuyển
    /// </summary>
    private bool ValidateWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] Không có điểm di chuyển nào được thiết lập cho bẫy!");
            enabled = false;
            return false;
        }

        // Kiểm tra xem có điểm nào bị null không
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
            {
                enabled = false;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Cập nhật trạng thái của bẫy mỗi frame
    /// </summary>
    private void Update()
    {
        if (!enabled) return;

        if (isWaiting)
        {
            HandleWaiting();
        }
        else
        {
            HandleMovement();
        }
    }

    /// <summary>
    /// Xử lý trạng thái chờ tại mỗi điểm
    /// </summary>
    private void HandleWaiting()
    {
        waitCounter += Time.deltaTime;
        if (waitCounter >= waitTime)
        {
            ResetWaitingState();
            MoveToNextWaypoint();
        }
    }

    /// <summary>
    /// Reset trạng thái chờ
    /// </summary>
    private void ResetWaitingState()
    {
        isWaiting = false;
        waitCounter = 0f;
    }

    /// <summary>
    /// Xử lý di chuyển và xoay của bẫy
    /// </summary>
    private void HandleMovement()
    {
        // Xoay bẫy theo tốc độ đã cài đặt
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);

        // Di chuyển đến điểm tiếp theo
        targetPosition = waypoints[currentWaypointIndex].position;
        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
        transform.position = newPosition;

        // Kiểm tra xem đã đến điểm đích chưa
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget < ARRIVAL_THRESHOLD)
        { 
            isWaiting = true;
        }
    }

    /// <summary>
    /// Chuyển đến điểm di chuyển tiếp theo
    /// </summary>
    private void MoveToNextWaypoint()
    {
        if (loop)
        {
            // Nếu lặp lại, chuyển đến điểm tiếp theo hoặc quay về điểm đầu
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else
        {
            // Nếu không lặp lại, chuyển đến điểm tiếp theo hoặc dừng
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                enabled = false;
                return;
            }
        }

     }

    /// <summary>
    /// Vẽ đường đi và các điểm trong Scene view
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showGizmos || !ValidateWaypoints()) return;

        DrawWaypointLines();
        DrawWaypointPoints();
    }

    /// <summary>
    /// Vẽ các đường nối giữa các điểm
    /// </summary>
    private void DrawWaypointLines()
    {
        Gizmos.color = gizmoColor;
        
        // Vẽ đường đi giữa các điểm
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            DrawLineBetweenWaypoints(waypoints[i], waypoints[i + 1]);
        }

        // Vẽ đường từ điểm cuối về điểm đầu nếu loop
        if (loop && waypoints.Length > 0)
        {
            DrawLineBetweenWaypoints(waypoints[^1], waypoints[0]);
        }
    }

    /// <summary>
    /// Vẽ đường thẳng giữa hai điểm
    /// </summary>
    private void DrawLineBetweenWaypoints(Transform start, Transform end)
    {
        if (start != null && end != null)
        {
            Gizmos.DrawLine(start.position, end.position);
        }
    }

    /// <summary>
    /// Vẽ các điểm di chuyển
    /// </summary>
    private void DrawWaypointPoints()
    {
        Gizmos.color = Color.yellow;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawWireSphere(waypoint.position, 0.3f);
            }
        }
    }
} 