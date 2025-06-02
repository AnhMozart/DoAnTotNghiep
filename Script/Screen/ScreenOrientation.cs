using UnityEngine;

public class ScreenOrientation : MonoBehaviour
{
    void Start()
    {
        // Cài đặt hướng màn hình ngang
        Screen.orientation = UnityEngine.ScreenOrientation.LandscapeLeft;
        
        // Khóa hướng màn hình để không thể xoay
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }
} 