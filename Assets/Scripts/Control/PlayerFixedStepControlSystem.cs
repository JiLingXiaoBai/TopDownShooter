using Unity.Entities;
using Unity.Physics;
using UnityEngine;

/// <summary>
/// Apply inputs that need to be read at a fixed rate.
/// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
public partial class PlayerFixedStepControlSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<FixedTickSingletonComponent>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<PlayerInputComponent, CharacterControlComponent>()
            .Build());
    }

    protected override void OnUpdate()
    {
        if (MainPlayerGameObject.Instance == null) return;
        uint tick = SystemAPI.GetSingleton<FixedTickSingletonComponent>().Tick;
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        foreach (var (input, control) in SystemAPI
                     .Query<RefRO<PlayerInputComponent>, RefRW<CharacterControlComponent>>().WithAll<Simulate>())
        {
            var screenPoint = input.ValueRO.lookInput;
            var camera = MainPlayerGameObject.Instance.cinemachineBrain.OutputCamera;
            var cameraRay = camera.ScreenPointToRay(new Vector3(screenPoint.x, screenPoint.y, 0));
            RaycastInput raycastInput = new RaycastInput
            {
                Start = cameraRay.GetPoint(0),
                End = cameraRay.GetPoint(999f),
                Filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = 1u << GameAsset.DEFAULT_LAYER,
                    GroupIndex = 0,
                }
            };
            if (collisionWorld.CastRay(raycastInput, out var hit))
            {
                control.ValueRW.lookVector = hit.Position;
            }
            control.ValueRW.moveVector = input.ValueRO.moveInput;
            control.ValueRW.jump = input.ValueRO.jumpPressed.IsSet(tick);
        }
    }
}