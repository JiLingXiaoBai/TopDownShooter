using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Physics;

[UpdateInGroup(typeof(KinematicCharacterPhysicsUpdateGroup))]
[BurstCompile]
public partial struct CharacterMovementPhysicsUpdateSystem : ISystem
{
    private EntityQuery _characterQuery;
    private CharacterMovementUpdateContext _context;
    private KinematicCharacterUpdateContext _baseContext;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        _characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
            .WithAll<CharacterMovementComponent, CharacterControlComponent>().Build(ref state);
        _context = new CharacterMovementUpdateContext();
        _context.OnSystemCreate(ref state);
        _baseContext = new KinematicCharacterUpdateContext();
        _baseContext.OnSystemCreate(ref state);

        state.RequireForUpdate(_characterQuery);
        state.RequireForUpdate<PhysicsWorldSingleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _context.OnSystemUpdate(ref state);
        _baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());

        CharacterMovementPhysicsUpdateJob job = new CharacterMovementPhysicsUpdateJob
        {
            context = _context,
            baseContext = _baseContext,
        };
        job.ScheduleParallel();
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    public partial struct CharacterMovementPhysicsUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public CharacterMovementUpdateContext context;
        public KinematicCharacterUpdateContext baseContext;

        void Execute(CharacterMovementAspect movementAspect)
        {
            movementAspect.PhysicsUpdate(ref context, ref baseContext);
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            baseContext.EnsureCreationOfTmpCollections();
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask,
            bool chunkWasExecuted)
        {
        }
    }
}