using Unity.Cinemachine;
using UnityEngine;

[DisallowMultipleComponent]
public class MainCameraGameObject : MonoBehaviour
{
    public CinemachinePositionComposer positionComposer;
    public Transform follow;
    public static MainCameraGameObject Instance;

    private void Awake()
    {
        Instance = this;
    }
}