using UnityEngine;
using Firebase.Auth;
using System;
using GameData;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseUser user;

    public event Action<bool> OnAuthStateChanged;

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
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Đã đăng xuất");
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log($"Đăng nhập thành công: {user.Email}");
            }
            OnAuthStateChanged?.Invoke(signedIn);
        }
    }

    public async void SignUp(string email, string password, Action<bool, string> callback)
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            Debug.Log($"Đăng ký thành công: {result.User.Email}");
            callback?.Invoke(true, "Đăng ký thành công!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Đăng ký thất bại: {e.Message}");
            callback?.Invoke(false, $"Đăng ký thất bại: {e.Message}");
        }
    }

    public async void SignIn(string email, string password, Action<bool, string> callback)
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            Debug.Log($"Đăng nhập thành công: {result.User.Email}");
            
            // Tải dữ liệu người chơi từ Firebase
            if (PlayerDataManager.Instance != null)
            {
                await PlayerDataManager.Instance.LoadPlayerData();
            }
            
            callback?.Invoke(true, "Đăng nhập thành công!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Đăng nhập thất bại: {e.Message}");
            callback?.Invoke(false, $"Đăng nhập thất bại: {e.Message}");
        }
    }

    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            Debug.Log("Đã đăng xuất");
        }
    }

    public async void ResetPassword(string email, Action<bool, string> callback)
    {
        try
        {
            await auth.SendPasswordResetEmailAsync(email);
            callback?.Invoke(true, "Đã gửi email đặt lại mật khẩu!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Gửi email đặt lại mật khẩu thất bại: {e.Message}");
            callback?.Invoke(false, $"Gửi email đặt lại mật khẩu thất bại: {e.Message}");
        }
    }

    public bool IsSignedIn()
    {
        return user != null;
    }

    public string GetUserId()
    {
        return user?.UserId;
    }

    public string GetUserEmail()
    {
        return user?.Email;
    }

    private void OnDestroy()
    {
        if (auth != null)
        {
            auth.StateChanged -= AuthStateChanged;
            auth = null;
        }
    }
} 