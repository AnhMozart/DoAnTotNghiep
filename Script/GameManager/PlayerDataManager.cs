using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GameData
{
    // Class để lưu backup dữ liệu
    public class PlayerDataBackup
    {
        public float Coin { get; set; }
        public int UnlockedLevel { get; set; }
        public DateTime BackupTime { get; set; }
    }

    // Class để lưu thông tin bảng xếp hạng
    public class PlayerLeaderboardEntry
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public float Score { get; set; }  // Đổi từ int sang float vì coin là float
        public string Timestamp { get; set; }
    }

    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }
        private DatabaseReference dbReference;
        private FirebaseAuth auth;

        // Cache dữ liệu người chơi
        private float cachedCoin = 0f;
        private int cachedUnlockedLevel = 1;
        private bool isInMainMenu = false;

        // Biến cho cơ chế backup và khôi phục
        private PlayerDataBackup lastBackup;
        private const string BACKUP_COIN_KEY = "BackupCoin";
        private const string BACKUP_LEVEL_KEY = "BackupLevel";
        private const string BACKUP_TIME_KEY = "BackupTime";
        private const int MAX_RETRIES = 3;
        private const float RETRY_DELAY = 1f;
        private bool isRecoveryMode = false;

        private bool isInitialized = false;
        private bool isLoadingData = false;

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

        private void Start()
        {
            // Đăng ký sự kiện khi chuyển scene
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
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
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            isInMainMenu = scene.name.Contains("MainMenu");
            Debug.Log($"Loaded scene: {scene.name}, isInMainMenu: {isInMainMenu}");

            // Nếu chuyển từ MainMenu sang Game scene, chuyển dữ liệu từ cache sang
            if (!isInMainMenu && auth != null && auth.CurrentUser != null)
            {
                // Đợi một frame để đảm bảo các Manager đã được khởi tạo
                StartCoroutine(TransferDataAfterSceneLoad());
            }
        }

        private System.Collections.IEnumerator TransferDataAfterSceneLoad()
        {
            // Đợi 2 frames để đảm bảo mọi thứ đã được khởi tạo
            yield return null;
            yield return null;

            TransferCachedDataToGame();
        }

        private async void Auth_StateChanged(object sender, System.EventArgs e)
        {
            Debug.Log("Auth_StateChanged được gọi");
            
            if (auth.CurrentUser != null)
            {
                // Người dùng vừa đăng nhập
                Debug.Log($"Người dùng đăng nhập: {auth.CurrentUser.Email}");
                
                // Đảm bảo không load data nhiều lần
                if (!isLoadingData)
                {
                    isLoadingData = true;
                    await LoadAndInitializeData();
                    isLoadingData = false;
                }
            }
            else
            {
                // Người dùng đăng xuất - chỉ reset local data
                Debug.Log("Người dùng đăng xuất - Reset local data");
                ResetLocalData();
            }
        }

        private async Task LoadAndInitializeData()
        {
            try
            {
                Debug.Log("Bắt đầu load và khởi tạo dữ liệu...");

                // Load dữ liệu từ Firebase trước
                await LoadPlayerData();

                // Đánh dấu đã khởi tạo
                isInitialized = true;

                Debug.Log($"Hoàn tất load dữ liệu - Coin: {cachedCoin}, Level: {cachedUnlockedLevel}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lỗi khi load dữ liệu: {ex.Message}");
                isInitialized = false;
            }
        }

        private void ResetLocalData()
        {
            // Chỉ reset data local, không reset PlayerPrefs
            cachedCoin = 0f;
            cachedUnlockedLevel = 1;
            isInitialized = false;

            if (PlayerManager.instance != null)
            {
                PlayerManager.instance.SetCoin(0);
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.SetUnlockedLevel(1);
            }
        }

        // Lưu dữ liệu người chơi
        public async Task SavePlayerData()
        {
            if (!IsSignedIn())
            {
                Debug.LogWarning("Không thể lưu dữ liệu: Người dùng chưa đăng nhập");
                return;
            }

            // Tạo backup trước khi lưu
            if (!isRecoveryMode)
            {
                CreateBackup();
            }

            bool success = await TryOperation(async () =>
            {
                Debug.Log("Bắt đầu lưu dữ liệu người chơi...");

                // Lấy dữ liệu hiện tại
                float currentCoin;
                int currentLevel;

                if (!isInMainMenu && PlayerManager.instance != null && LevelManager.Instance != null)
                {
                    currentCoin = PlayerManager.instance.GetCurrentCoin();
                    currentLevel = LevelManager.Instance.GetCurrentUnlockedLevel();
                    Debug.Log($"Lấy dữ liệu từ Manager: Coin={currentCoin}, Level={currentLevel}");
                }
                else
                {
                    currentCoin = cachedCoin;
                    currentLevel = cachedUnlockedLevel;
                    Debug.Log($"Lấy dữ liệu từ cache: Coin={currentCoin}, Level={currentLevel}");
                }

                string userId = auth.CurrentUser.UserId;
                string displayName = auth.CurrentUser.DisplayName ?? auth.CurrentUser.Email?.Split('@')[0] ?? "Anonymous";

                var playerData = new Dictionary<string, object>
                {
                    { "coin", currentCoin },
                    { "unlockedLevel", currentLevel },
                    { "displayName", displayName },
                    { "lastUpdated", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                Debug.Log($"Chuẩn bị lưu dữ liệu: {string.Join(", ", playerData.Select(kv => $"{kv.Key}={kv.Value}"))}");

                // Cập nhật dữ liệu người chơi
                await dbReference.Child("players").Child(userId).UpdateChildrenAsync(playerData);

                // Cập nhật bảng xếp hạng
                await UpdateLeaderboard(currentCoin);

                // Cập nhật cache
                cachedCoin = currentCoin;
                cachedUnlockedLevel = currentLevel;

                // Cập nhật PlayerPrefs
                PlayerPrefs.SetFloat("CurrentCoin", cachedCoin);
                PlayerPrefs.SetInt("UnlockedLevel", cachedUnlockedLevel);
                PlayerPrefs.Save();

                Debug.Log($"Đã lưu dữ liệu thành công: Coin={currentCoin}, Level={currentLevel}");
            }, "SavePlayerData");

            if (!success && !isRecoveryMode)
            {
                Debug.LogWarning("Không thể lưu dữ liệu sau nhiều lần thử, khôi phục từ backup");
                RestoreFromBackup();
            }
        }

        // Cập nhật bảng xếp hạng
        private async Task UpdateLeaderboard(float currentCoin)
        {
            if (!IsSignedIn()) return;

            try
            {
                string userId = auth.CurrentUser.UserId;
                var leaderboardEntry = new Dictionary<string, object>
                {
                    { "userId", userId },
                    { "displayName", auth.CurrentUser.DisplayName ?? auth.CurrentUser.Email?.Split('@')[0] ?? "Anonymous" },
                    { "coin", currentCoin },
                    { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                };

                await dbReference.Child("leaderboard").Child(userId).UpdateChildrenAsync(leaderboardEntry);
            }
            catch (Exception e)
            {
                Debug.LogError($"Lỗi khi cập nhật bảng xếp hạng: {e.Message}");
            }
        }

        // Phương thức mới để chuyển dữ liệu từ cache sang PlayerManager khi vào game
        public void TransferCachedDataToGame()
        {
            try
            {
                Debug.Log($"Bắt đầu chuyển dữ liệu cache sang game - Cache: Coin={cachedCoin}, Level={cachedUnlockedLevel}");

                if (PlayerManager.instance != null)
                {
                    PlayerManager.instance.SetCoin(cachedCoin);
                    Debug.Log($"Đã chuyển coin={cachedCoin} sang PlayerManager");
                }
                else
                {
                    Debug.LogError("Không thể chuyển dữ liệu: PlayerManager chưa được khởi tạo");
                }

                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.SetUnlockedLevel(cachedUnlockedLevel);
                    Debug.Log($"Đã chuyển level={cachedUnlockedLevel} sang LevelManager");
                }
                else
                {
                    Debug.LogError("Không thể chuyển dữ liệu: LevelManager chưa được khởi tạo");
                }

                Debug.Log("Hoàn tất chuyển dữ liệu cache sang game");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lỗi khi chuyển dữ liệu: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        public async Task LoadPlayerData()
        {
            if (!IsSignedIn())
            {
                Debug.LogWarning("Không thể tải dữ liệu: Người dùng chưa đăng nhập");
                return;
            }

            try
            {
                Debug.Log("Bắt đầu tải dữ liệu người chơi...");
                string userId = auth.CurrentUser.UserId;

                // Tạo backup trước khi load dữ liệu mới
                if (!isRecoveryMode)
                {
                    CreateBackup();
                }

                // Load dữ liệu từ Firebase
                var snapshot = await dbReference.Child("players").Child(userId).GetValueAsync();
                
                if (snapshot != null && snapshot.Exists)
                {
                    var data = snapshot.Value as Dictionary<string, object>;
                    if (data != null)
                    {
                        float firebaseCoin = GetSafeFloat(data, "coin");
                        int firebaseLevel = GetSafeInt(data, "unlockedLevel", 1);
                        Debug.Log($"Dữ liệu từ Firebase: Coin={firebaseCoin}, Level={firebaseLevel}");

                        // Kiểm tra dữ liệu có hợp lệ không
                        if (isRecoveryMode || ValidateData(firebaseCoin, firebaseLevel))
                        {
                            // Cập nhật cache
                            cachedCoin = firebaseCoin;
                            cachedUnlockedLevel = firebaseLevel;

                            // Chỉ cập nhật PlayerPrefs nếu dữ liệu hợp lệ
                            if (cachedCoin > 0 || cachedUnlockedLevel > 1)
                            {
                                PlayerPrefs.SetFloat("CurrentCoin", cachedCoin);
                                PlayerPrefs.SetInt("UnlockedLevel", cachedUnlockedLevel);
                                PlayerPrefs.Save();
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Dữ liệu từ Firebase không hợp lệ, thử khôi phục từ backup");
                            if (RestoreFromBackup())
                            {
                                isRecoveryMode = true;
                                await SavePlayerData(); // Lưu lại dữ liệu đã khôi phục
                                isRecoveryMode = false;
                            }
                        }
                    }
                }
                else
                {
                    // Nếu không có dữ liệu trên Firebase, thử khôi phục từ backup
                    if (RestoreFromBackup())
                    {
                        isRecoveryMode = true;
                        await SavePlayerData();
                        isRecoveryMode = false;
                    }
                    else
                    {
                        // Nếu không có backup, kiểm tra PlayerPrefs
                        float localCoin = PlayerPrefs.GetFloat("CurrentCoin", 0);
                        int localLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

                        if (localCoin > 0 || localLevel > 1)
                        {
                            cachedCoin = localCoin;
                            cachedUnlockedLevel = localLevel;
                            await SavePlayerData();
                        }
                        else
                        {
                            // Không làm gì cả, tránh ghi đè dữ liệu 0 lên Firebase và PlayerPrefs
                            Debug.LogWarning("Không có dữ liệu hợp lệ để lưu, giữ nguyên trạng thái.");
                        }
                    }
                }

                // Cập nhật các Manager nếu cần
                if (!isInMainMenu)
                {
                    TransferCachedDataToGame();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Lỗi khi tải dữ liệu: {e.Message}");
                
                // Trong trường hợp lỗi, thử khôi phục từ backup
                if (RestoreFromBackup())
                {
                    Debug.Log("Đã khôi phục dữ liệu từ backup sau khi gặp lỗi");
                }
                else
                {
                    // Nếu không có backup, sử dụng dữ liệu từ PlayerPrefs
                    cachedCoin = PlayerPrefs.GetFloat("CurrentCoin", 0);
                    cachedUnlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
                }
            }
        }

        private bool ValidateData(float coin, int level)
        {
            // Kiểm tra dữ liệu có hợp lệ không
            if (coin < 0 || level < 1)
            {
                return false;
            }

            // Nếu có dữ liệu backup, so sánh với backup
            if (lastBackup != null)
            {
                // Nếu dữ liệu mới thấp hơn backup quá nhiều, coi như không hợp lệ
                if (coin < lastBackup.Coin * 0.5f || level < lastBackup.UnlockedLevel)
                {
                    Debug.LogWarning($"Dữ liệu mới thấp hơn backup: Coin={coin} vs {lastBackup.Coin}, Level={level} vs {lastBackup.UnlockedLevel}");
                    return false;
                }
            }

            return true;
        }

        private float GetSafeFloat(Dictionary<string, object> data, string key, float defaultValue = 0f)
        {
            if (data != null && data.ContainsKey(key) && data[key] != null)
            {
                try
                {
                    return Convert.ToSingle(data[key]);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Lỗi khi chuyển đổi {key}: {ex.Message}");
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        private int GetSafeInt(Dictionary<string, object> data, string key, int defaultValue = 0)
        {
            if (data != null && data.ContainsKey(key) && data[key] != null)
            {
                try
                {
                    return Convert.ToInt32(data[key]);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Lỗi khi chuyển đổi {key}: {ex.Message}");
                    return defaultValue;
                }
            }
            return defaultValue;
        }

        // Lấy top 10 người chơi có số coin cao nhất
        public async Task<List<PlayerLeaderboardEntry>> GetTopPlayers(int limit = 10)
        {
            var leaderboard = new List<PlayerLeaderboardEntry>();

            try
            {
                var snapshot = await dbReference.Child("leaderboard")
                    .OrderByChild("coin")
                    .LimitToLast(limit)
                    .GetValueAsync();

                Debug.Log($"Đã nhận snapshot: Exists={snapshot?.Exists}, ChildrenCount={snapshot?.ChildrenCount}");

                if (snapshot != null && snapshot.Exists)
                {
                    // Xử lý từng child node thay vì ép kiểu trực tiếp
                    foreach (var childSnapshot in snapshot.Children)
                    {
                        try
                        {
                            var data = childSnapshot.Value as Dictionary<string, object>;
                            if (data == null)
                            {
                                Debug.LogWarning($"Data null cho child key: {childSnapshot.Key}");
                                continue;
                            }

                            Debug.Log($"Xử lý entry: {string.Join(", ", data.Select(kv => $"{kv.Key}={kv.Value}"))}");

                            string userId = GetSafeString(data, "userId");
                            string displayName = GetSafeString(data, "displayName");
                            float score = GetSafeFloat(data, "coin");
                            string timestamp = GetSafeString(data, "timestamp", DateTime.Now.ToString());

                            // Nếu displayName trống, thử lấy từ email
                            if (string.IsNullOrEmpty(displayName) && auth.CurrentUser != null && userId == auth.CurrentUser.UserId)
                            {
                                displayName = auth.CurrentUser.Email?.Split('@')[0] ?? "Anonymous";
                            }

                            var entry = new PlayerLeaderboardEntry
                            {
                                UserId = userId,
                                DisplayName = displayName,
                                Score = score,
                                Timestamp = timestamp
                            };

                            Debug.Log($"Đã xử lý entry: UserId={entry.UserId}, DisplayName={entry.DisplayName}, Score={entry.Score}");
                            leaderboard.Add(entry);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Lỗi khi xử lý entry: {ex.Message}\nStackTrace: {ex.StackTrace}");
                            continue;
                        }
                    }

                    // Sắp xếp theo điểm số giảm dần
                    leaderboard.Sort((a, b) => b.Score.CompareTo(a.Score));

                    Debug.Log($"Tổng số entry đã xử lý: {leaderboard.Count}");
                    foreach (var entry in leaderboard)
                    {
                        Debug.Log($"Entry trong bảng xếp hạng: {entry.DisplayName} - {entry.Score} điểm");
                    }
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy dữ liệu bảng xếp hạng!");
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
            if (data != null && data.ContainsKey(key) && data[key] != null)
            {
                return data[key].ToString();
            }
            return defaultValue;
        }

        // Thêm các phương thức để lấy dữ liệu đã cache
        public float GetCachedCoin() => cachedCoin;
        public int GetCachedUnlockedLevel() => cachedUnlockedLevel;

        public bool IsSignedIn()
        {
            return auth != null && auth.CurrentUser != null;
        }

        // Các phương thức backup và khôi phục
        private void CreateBackup()
        {
            try
            {
                lastBackup = new PlayerDataBackup
                {
                    Coin = cachedCoin,
                    UnlockedLevel = cachedUnlockedLevel,
                    BackupTime = DateTime.Now
                };

                // Lưu vào PlayerPrefs
                PlayerPrefs.SetFloat(BACKUP_COIN_KEY, cachedCoin);
                PlayerPrefs.SetInt(BACKUP_LEVEL_KEY, cachedUnlockedLevel);
                PlayerPrefs.SetString(BACKUP_TIME_KEY, DateTime.Now.ToString());
                PlayerPrefs.Save();

                Debug.Log($"Đã tạo backup: Coin={cachedCoin}, Level={cachedUnlockedLevel}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lỗi khi tạo backup: {ex.Message}");
            }
        }

        private bool RestoreFromBackup()
        {
            try
            {
                // Thử khôi phục từ lastBackup trước
                if (lastBackup != null)
                {
                    cachedCoin = lastBackup.Coin;
                    cachedUnlockedLevel = lastBackup.UnlockedLevel;
                    Debug.Log($"Khôi phục từ memory backup: Coin={cachedCoin}, Level={cachedUnlockedLevel}");
                    return true;
                }

                // Nếu không có lastBackup, thử từ PlayerPrefs
                if (PlayerPrefs.HasKey(BACKUP_COIN_KEY))
                {
                    cachedCoin = PlayerPrefs.GetFloat(BACKUP_COIN_KEY);
                    cachedUnlockedLevel = PlayerPrefs.GetInt(BACKUP_LEVEL_KEY);
                    Debug.Log($"Khôi phục từ PlayerPrefs backup: Coin={cachedCoin}, Level={cachedUnlockedLevel}");
                    return true;
                }

                Debug.LogWarning("Không tìm thấy bản backup nào");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Lỗi khi khôi phục backup: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> TryOperation(Func<Task> operation, string operationName)
        {
            int retryCount = 0;
            while (retryCount < MAX_RETRIES)
            {
                try
                {
                    await operation();
                    return true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    Debug.LogWarning($"Lỗi {operationName} (lần {retryCount}): {ex.Message}");
                    
                    if (retryCount < MAX_RETRIES)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(RETRY_DELAY * retryCount));
                    }
                }
            }
            return false;
        }

        // Thêm phương thức để cập nhật dữ liệu khi cần
        public async Task RefreshUserData()
        {
            if (IsSignedIn())
            {
                await LoadAndInitializeData();
            }
        }
    }
}