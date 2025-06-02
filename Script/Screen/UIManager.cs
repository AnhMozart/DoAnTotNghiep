using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Canvas Settings")]
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectTransform[] panels; // Mảng chứa các panel cần scale

    void Awake()
    {
        if (canvasScaler == null)
            canvasScaler = GetComponent<CanvasScaler>();
            
        SetupCanvasScaler();
        SetupPanels();
    }

    private void SetupCanvasScaler()
    {
        // Sử dụng chế độ Scale With Screen Size
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
        // Đặt độ phân giải tham chiếu (thường là độ phân giải thiết kế của bạn)
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        // Match Width or Height (0 = width, 1 = height, 0.5 = cả hai)
        canvasScaler.matchWidthOrHeight = 0.5f;
    }

    private void SetupPanels()
    {
        // Nếu không có panels được gán, tự động tìm tất cả các panel trong canvas
        if (panels == null || panels.Length == 0)
        {
            Debug.LogError("No panels assigned to UIManager");
        }

        foreach (RectTransform panel in panels)
        {
            if (panel != null && panel != transform)
            {
                // Thiết lập anchor và pivot để panel scale đúng
                panel.anchorMin = Vector2.zero;
                panel.anchorMax = Vector2.one;
                panel.pivot = new Vector2(0.5f, 0.5f);
                
                // Stretch để lấp đầy parent
                panel.offsetMin = Vector2.zero;
                panel.offsetMax = Vector2.zero;

                // Đặt scale về 1
                panel.localScale = Vector3.one;
            }
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        // Gọi lại setup khi kích thước canvas thay đổi
        SetupPanels();
    }
} 