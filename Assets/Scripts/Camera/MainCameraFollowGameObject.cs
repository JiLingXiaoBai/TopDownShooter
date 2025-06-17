using UnityEngine;

public class MainCameraFollowGameObject : MonoBehaviour
{
    private static MainCameraFollowGameObject _instance;

    public static MainCameraFollowGameObject Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = FindFirstObjectByType<MainCameraFollowGameObject>();
            if (_instance == null)
            {
                GameObject gameObject = new GameObject("MainCameraFollowGameObject");
                _instance = gameObject.AddComponent<MainCameraFollowGameObject>();
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }

    private MainCameraFollowGameObject()
    {
    }
}