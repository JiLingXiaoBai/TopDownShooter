using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary>
/// Apply inputs that need to be read at a fixed rate.
/// It is necessary to handle this as part of the fixed step group, in case your framerate is lower than the fixed step rate.
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderFirst = true)]
public partial struct PlayerFixedStepControlSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FixedTickSingletonComponent>();
        state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<PlayerInputComponent, CharacterControlComponent>()
            .Build());
    }

    public void OnUpdate(ref SystemState state)
    {
        uint tick = SystemAPI.GetSingleton<FixedTickSingletonComponent>().Tick;
        foreach (var (input, control, trans) in SystemAPI
                     .Query<RefRO<PlayerInputComponent>, RefRW<CharacterControlComponent>, RefRO<LocalTransform>>()
                     .WithAll<Simulate>())
        {
            var moveVector = new float3(input.ValueRO.moveInput.x, 0,  input.ValueRO.moveInput.y);
            control.ValueRW.moveVector = MathUtilities.ClampToMaxLength(moveVector, 1f);
            control.ValueRW.jump = input.ValueRO.jumpPressed.IsSet(tick);
        }
    }
}