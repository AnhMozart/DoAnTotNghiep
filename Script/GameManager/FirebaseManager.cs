using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    
    // Thêm các biến để lưu trữ dữ liệu người chơi hiện tại
    private int currentPlayerCoin = 0;
    private int currentPlayerUnlockedLevel = 0;

    public int CurrentPlayerCoin => currentPlayerCoin;
    public int CurrentPlayerUnlockedLevel => currentPlayerUnlockedLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeFirebase()
    {
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;

        // Đăng ký sự kiện thay đổi trạng thái đăng nhập
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

    private async void Auth_StateChanged(object sender, System.EventArgs e)
    {
        if (auth.CurrentUser != null)
        {
            // Người dùng vừa đăng nhập
            Debug.Log($"Người dùng đăng nhập: {GetUserDisplayName()}");
            await LoadPlayerData();
        }
        else
        {
            // Người dùng đăng xuất
            Debug.Log("Người dùng đăng xuất");
            ResetPlayerData();
        }
    }

    private void ResetPlayerData()
    {
        currentPlayerCoin = 0;
        currentPlayerUnlockedLevel = 0;
    }

    // Phương thức mới để tải dữ liệu người chơi
    public async Task LoadPlayerData()
    {
        if (!IsSignedIn()) return;

        try
        {
            string userId = auth.CurrentUser.UserId;
            var snapshot = await dbReference.Child("players").Child(userId).GetValueAsync();
            
            if (snapshot != null && snapshot.Exists)
            {
                var data = snapshot.Value as Dictionary<string, object>;
                if (data != null)
                {
                    currentPlayerCoin = GetSafeInt(data, "coin");
                    currentPlayerUnlockedLevel = GetSafeInt(data, "unlockedLevel");
                    Debug.Log($"Đã tải dữ liệu người chơi: Coin={currentPlayerCoin}, Level={currentPlayerUnlockedLevel}");
                }
            }
            else
            {
                // Nếu chưa có dữ liệu, tạo dữ liệu mới
                ResetPlayerData();
                await SavePlayerData(0, 1); // Mặc định có 1 màn đã mở
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi tải dữ liệu người chơi: {e.Message}");
            ResetPlayerData();
        }
    }

    // Cập nhật phương thức SavePlayerData
    public async Task SavePlayerData(int score, int unlockedMaps)
    {
        if (!IsSignedIn()) return;

        try
        {
            string userId = auth.CurrentUser.UserId;
            string displayName = GetUserDisplayName();

            // Cập nhật biến local
            currentPlayerCoin = score;
            currentPlayerUnlockedLevel = unlockedMaps;

            var playerData = new Dictionary<string, object>
            {
                { "coin", score },
                { "unlockedLevel", unlockedMaps },
                { "displayName", displayName },
                { "lastUpdated", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
            };

            await dbReference.Child("players").Child(userId).UpdateChildrenAsync(playerData);
            
            // Cập nhật bảng xếp hạng
            await UpdateLeaderboard(score);
            
            Debug.Log($"Đã lưu dữ liệu người chơi: Coin={score}, Level={unlockedMaps}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi lưu dữ liệu: {e.Message}");
        }
    }

    // Cập nhật bảng xếp hạng
    private async Task UpdateLeaderboard(int newScore)
    {
        if (!IsSignedIn()) return;

        try
        {
            string userId = auth.CurrentUser.UserId;
            string displayName = GetUserDisplayName();
            
            // Kiểm tra điểm số hiện tại
            var currentScoreSnapshot = await dbReference.Child("leaderboard").Child(userId).Child("coin").GetValueAsync();
            int currentScore = 0;
            
            if (currentScoreSnapshot != null && currentScoreSnapshot.Exists)
            {
                currentScore = Convert.ToInt32(currentScoreSnapshot.Value);
            }

            // Chỉ cập nhật nếu điểm mới cao hơn điểm cũ
            if (newScore > currentScore)
            {
                var leaderboardEntry = new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "displayName", displayName },
                    { "coin", newScore },
                    { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                await dbReference.Child("leaderboard").Child(userId).UpdateChildrenAsync(leaderboardEntry);
                Debug.Log($"Đã cập nhật điểm cao mới: {newScore}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi cập nhật bảng xếp hạng: {e.Message}");
        }
    }

    private string GetUserDisplayName()
    {
        if (auth == null || auth.CurrentUser == null)
            return "Anonymous";

        // Ưu tiên sử dụng DisplayName nếu có
        if (!string.IsNullOrEmpty(auth.CurrentUser.DisplayName))
            return auth.CurrentUser.DisplayName;

        // Nếu không có DisplayName, sử dụng email
        if (!string.IsNullOrEmpty(auth.CurrentUser.Email))
        {
            // Lấy phần trước @ của email
            string emailName = auth.CurrentUser.Email.Split('@')[0];
            return emailName;
        }

        return "Anonymous";
    }

    // Lấy dữ liệu người chơi
    public async Task<(int Score, int UnlockedMaps)> GetPlayerData()
    {
        if (!IsSignedIn()) return (0, 0);

        try
        {
            string userId = auth.CurrentUser.UserId;
            var snapshot = await dbReference.Child("players").Child(userId).GetValueAsync();
            
            if (snapshot.Exists)
            {
                var data = snapshot.Value as Dictionary<string, object>;
                int score = Convert.ToInt32(data["coin"]);
                int unlockedMaps = Convert.ToInt32(data["unlockedLevel"]);
                return (score, unlockedMaps);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi lấy dữ liệu: {e.Message}");
        }

        return (0, 0);
    }

    // Lấy top 10 điểm cao nhất
    public async Task<List<LeaderboardEntry>> GetTopScores()
    {
        var leaderboard = new List<LeaderboardEntry>();
        try
        {
            if (!IsSignedIn())
            {
                Debug.LogWarning("Người dùng chưa đăng nhập!");
                return leaderboard;
            }

            Debug.Log("Bắt đầu lấy dữ liệu bảng xếp hạng...");
            DatabaseReference reference = FirebaseDatabase.DefaultInstance.GetReference("leaderboard");
            var snapshot = await reference.OrderByChild("coin").LimitToLast(10).GetValueAsync();

            Debug.Log($"Đã nhận snapshot: Exists={snapshot?.Exists}, ChildrenCount={snapshot?.ChildrenCount}");

            if (snapshot != null && snapshot.Exists)
            {
                foreach (var entry in snapshot.Children)
                {
                    try
                    {
                        var data = entry.Value as Dictionary<string, object>;
                        Debug.Log($"Entry data: {(data != null ? string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}")) : "null")}");
                        
                        if (data == null) continue;

                        string userId = GetSafeString(data, "userId");
                        string displayName = GetSafeString(data, "displayName");
                        
                        // Nếu displayName trống, thử lấy email từ auth
                        if (string.IsNullOrEmpty(displayName) && auth.CurrentUser != null && userId == auth.CurrentUser.UserId)
                        {
                            displayName = GetUserDisplayName();
                        }
                        
                        // Nếu vẫn trống, sử dụng giá trị mặc định
                        if (string.IsNullOrEmpty(displayName))
                        {
                            displayName = "Người chơi ẩn danh";
                        }

                        int score = GetSafeInt(data, "coin");
                        string timestamp = GetSafeString(data, "timestamp", System.DateTime.Now.ToString());

                        Debug.Log($"Đã xử lý entry: UserId={userId}, DisplayName={displayName}, Score={score}");

                        leaderboard.Add(new LeaderboardEntry
                        {
                            UserId = userId,
                            DisplayName = displayName,
                            Score = score,
                            Timestamp = timestamp
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Lỗi khi xử lý entry: {ex.Message}\nStackTrace: {ex.StackTrace}");
                        continue;
                    }
                }

                Debug.Log($"Tổng số entry đã xử lý: {leaderboard.Count}");
            }
            else
            {
                Debug.LogWarning("Không tìm thấy dữ liệu bảng xếp hạng!");
            }
            
            // Sắp xếp theo điểm số giảm dần
            leaderboard.Sort((a, b) => b.Score.CompareTo(a.Score));
            
            Debug.Log("Danh sách sau khi sắp xếp:");
            foreach (var entry in leaderboard)
            {
                Debug.Log($"- {entry.DisplayName}: {entry.Score} điểm");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Lỗi khi lấy bảng xếp hạng: {e.Message}\nStackTrace: {e.StackTrace}");
        }

        return leaderboard;
    }

    private string GetSafeString(Dictionary<string, object> data, string key, string defaultValue = "")
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            return data[key].ToString();
        }
        return defaultValue;
    }

    private int GetSafeInt(Dictionary<string, object> data, string key, int defaultValue = 0)
    {
        if (data.ContainsKey(key) && data[key] != null)
        {
            try
            {
                return Convert.ToInt32(data[key]);
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    private bool IsSignedIn()
    {
        return auth != null && auth.CurrentUser != null;
    }
}

// Class để lưu thông tin bảng xếp hạng
public class LeaderboardEntry
{
    public string UserId { get; set; }
    public string DisplayName { get; set; }
    public int Score { get; set; }
    public string Timestamp { get; set; }
} 