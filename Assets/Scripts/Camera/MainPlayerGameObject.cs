using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class MainPlayerGameObject : MonoBehaviour
{
    public static MainPlayerGameObject Instance;

    public CinemachineBrain cinemachineBrain;
    public CinemachinePositionComposer cinemachinePositionComposer;
    public Transform cameraFollow;

    private InputSystemActions _actions;
    private InputSystemActions.PlayerActions _playerActions;

    public InputSystemActions.PlayerActions PlayerActions => _playerActions;

    public bool IsKeyboardMouseInput(InputAction inputAction)
    {
        return inputAction.activeControl != null &&
               _actions.KeyboardMouseScheme.SupportsDevice(inputAction.activeControl.device);
    }

    public bool IsGamepadInput(InputAction inputAction)
    {
        return inputAction.activeControl != null &&
               _actions.GamepadScheme.SupportsDevice(inputAction.activeControl.device);
    }

    private void Awake()
    {
        _actions = new InputSystemActions();
        _playerActions = _actions.Player;
        Instance = this;
    }

    private void OnDestroy()
    {
        _actions.Dispose();
    }

    private void OnEnable()
    {
        _playerActions.Enable();
    }

    private void OnDisable()
    {
        _playerActions.Disable();
    }
}