using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
public partial class PlayerInputSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<FixedTickSingletonComponent>();
        RequireForUpdate<PlayerInputComponent>();
    }

    protected override void OnUpdate()
    {
        if (MainPlayerGameObject.Instance == null) return;
        uint tick = SystemAPI.GetSingleton<FixedTickSingletonComponent>().Tick;
        var playerInput = SystemAPI.GetSingletonRW<PlayerInputComponent>();

        var playerActions = MainPlayerGameObject.Instance.PlayerActions;
        playerInput.ValueRW.moveInput = playerActions.Move.ReadValue<Vector2>();

        if (MainPlayerGameObject.Instance.IsKeyboardMouseInput(playerActions.Look))
        {
            playerInput.ValueRW.lookInput = playerActions.Look.ReadValue<Vector2>();
        }
        else if (MainPlayerGameObject.Instance.IsGamepadInput(playerActions.Look))
        {
            var origin = playerInput.ValueRO.lookInput;
            playerInput.ValueRW.lookInput =
                origin + new float2(GameAsset.Instance.StickSpeed * playerActions.Look.ReadValue<Vector2>());
        }

        if (playerActions.Jump.WasPressedThisFrame())
        {
            playerInput.ValueRW.jumpPressed.Set(tick);
        }
    }
}