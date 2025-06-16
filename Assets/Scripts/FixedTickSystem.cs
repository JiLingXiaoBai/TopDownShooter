using Unity.Entities;
using Unity.Burst;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup), OrderLast = true)]
[BurstCompile]
public partial struct FixedTickSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        if (!SystemAPI.HasSingleton<FixedTickSingletonComponent>())
        {
            state.EntityManager.CreateEntity(typeof(FixedTickSingletonComponent));
        }
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ref FixedTickSingletonComponent singleton = ref SystemAPI.GetSingletonRW<FixedTickSingletonComponent>().ValueRW;
        singleton.Tick++;
    }
}

public struct FixedTickSingletonComponent : IComponentData
{
    public uint Tick;
}