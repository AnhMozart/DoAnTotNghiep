using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Firebase.Auth;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Leaderboard Panel")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;

    [Header("Loading")]
    [SerializeField] private GameObject loadingPanel;
    
    [Header("Error")]
    [SerializeField] private GameObject errorPanel;
    [SerializeField] private TextMeshProUGUI errorText;

    private FirebaseAuth auth;

    private void Start()
    {
        closeButton.onClick.AddListener(CloseLeaderboard);
        refreshButton.onClick.AddListener(RefreshLeaderboard);
        
        // Ẩn panel mặc định
        leaderboardPanel.SetActive(false);
        loadingPanel.SetActive(false);
        if (errorPanel != null)
            errorPanel.SetActive(false);

        // Khởi tạo Firebase Auth và đăng ký sự kiện
        auth = FirebaseAuth.DefaultInstance;
        if (auth != null)
        {
            auth.StateChanged += Auth_StateChanged;
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi destroy
        if (auth != null)
        {
            auth.StateChanged -= Auth_StateChanged;
        }
    }

    private void Auth_StateChanged(object sender, System.EventArgs e)
    {
        if (auth.CurrentUser != null && leaderboardPanel.activeSelf)
        {
            // Nếu panel đang mở và có user mới, refresh lại dữ liệu
            RefreshLeaderboard();
        }
    }

    public void ShowLeaderboard()
    {
        if (!FirebaseAuthManager.Instance.IsSignedIn())
        {
            leaderboardPanel.SetActive(true);
            ShowError("Vui lòng đăng nhập để xem bảng xếp hạng.");
            return;
        }

        leaderboardPanel.SetActive(true);
        RefreshLeaderboard();
    }

    private void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    private async void RefreshLeaderboard()
    {
        try
        {
            loadingPanel.SetActive(true);
            if (errorPanel != null)
                errorPanel.SetActive(false);

            ClearLeaderboard();

            if (FirebaseManager.Instance == null)
            {
                Debug.LogError("FirebaseManager.Instance is null!");
                ShowError("Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
                return;
            }

            if (auth.CurrentUser == null)
            {
                ShowError("Vui lòng đăng nhập để xem bảng xếp hạng.");
                return;
            }

            var topScores = await FirebaseManager.Instance.GetTopScores();
            if (topScores != null && topScores.Count > 0)
            {
                DisplayLeaderboard(topScores);
            }
            else
            {
                ShowError("Chưa có dữ liệu điểm số.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error refreshing leaderboard: {ex.Message}");
            ShowError("Đã xảy ra lỗi khi tải bảng xếp hạng.");
        }
        finally
        {
            loadingPanel.SetActive(false);
        }
    }

    private void ShowError(string message)
    {
        if (errorPanel != null && errorText != null)
        {
            errorPanel.SetActive(true);
            errorText.text = message;
        }
        else
        {
            Debug.LogWarning($"Error UI elements not assigned: {message}");
        }
    }

    private void ClearLeaderboard()
    {
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }
    }

    private void DisplayLeaderboard(List<LeaderboardEntry> entries)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var entryGO = Instantiate(leaderboardEntryPrefab, leaderboardContent);
            var entryUI = entryGO.GetComponent<LeaderboardEntryUI>();

            if (entryUI != null)
            {
                entryUI.SetData(i + 1, entry.DisplayName, entry.Score);
            }
        }
    }
} 