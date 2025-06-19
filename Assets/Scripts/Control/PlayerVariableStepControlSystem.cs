using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

/// <summary>
/// Apply inputs that need to be read at a variable rate
/// </summary>
[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
[BurstCompile]
public partial class PlayerVariableStepControlSystem : SystemBase
{
    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<PhysicsWorldSingleton>();
        RequireForUpdate(SystemAPI.QueryBuilder().WithAll<PlayerInputComponent, CharacterControlComponent>()
            .Build());
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (MainPlayerGameObject.Instance == null) return;
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        foreach (var (input, control, trans) in SystemAPI
                     .Query<RefRO<PlayerInputComponent>, RefRW<CharacterControlComponent>, RefRO<LocalTransform>>()
                     .WithAll<Simulate>())
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
                control.ValueRW.lookVector =
                    hit.Position - (trans.ValueRO.Position + new float3(0, GameAsset.CHARACTER_HEIGHT_OFFSET, 0));
            }
        }
    }
}