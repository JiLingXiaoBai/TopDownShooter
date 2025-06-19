using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

public struct CharacterMovementUpdateContext
{
    // Here, you may add additional global data for your character updates, such as ComponentLookups, Singletons, NativeCollections, etc...
    // The data you add here will be accessible in your character updates and all of your character "callbacks".

    public void OnSystemCreate(ref SystemState state)
    {
        // Get lookups
    }

    public void OnSystemUpdate(ref SystemState state)
    {
        // Update lookups
    }
}

public readonly partial struct CharacterMovementAspect : IAspect,
    IKinematicCharacterProcessor<CharacterMovementUpdateContext>
{
    public readonly KinematicCharacterAspect CharacterAspect;
    public readonly RefRW<CharacterMovementComponent> MovementComponent;
    public readonly RefRW<CharacterControlComponent> ControlComponent;

    public void PhysicsUpdate(ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref CharacterMovementComponent movementComponent = ref MovementComponent.ValueRW;
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;

        // First phase of default character update
        CharacterAspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody,
            baseContext.Time.DeltaTime);
        CharacterAspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody,
            ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
        CharacterAspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody,
            ref characterPosition);

        // Update desired character velocity after grounding was detected, but before doing additional processing that depends on velocity
        HandleVelocityControl(ref context, ref baseContext);

        // Second phase of default character update
        CharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext,
            ref characterBody, in movementComponent.stepAndSlopeHandling);
        CharacterAspect.Update_GroundPushing(in this, ref context, ref baseContext, movementComponent.gravity);
        CharacterAspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody,
            ref characterPosition);
        CharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody);
        CharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
        CharacterAspect.Update_ProcessStatefulCharacterHits();
    }

    private void HandleVelocityControl(ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        float deltaTime = baseContext.Time.DeltaTime;
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref CharacterMovementComponent movementComponent = ref MovementComponent.ValueRW;
        ref CharacterControlComponent characterControl = ref ControlComponent.ValueRW;

        // Rotate move input and velocity to take into account parent rotation
        if (characterBody.ParentEntity != Entity.Null)
        {
            characterControl.moveVector = math.rotate(characterBody.RotationFromParent, characterControl.moveVector);
            characterBody.RelativeVelocity =
                math.rotate(characterBody.RotationFromParent, characterBody.RelativeVelocity);
        }

        if (characterBody.IsGrounded)
        {
            // Move on ground
            float3 targetVelocity = characterControl.moveVector * movementComponent.groundMaxSpeed;
            CharacterControlUtilities.StandardGroundMove_Interpolated(ref characterBody.RelativeVelocity,
                targetVelocity, movementComponent.groundedMovementSharpness, deltaTime, characterBody.GroundingUp,
                characterBody.GroundHit.Normal);

            // Jump
            if (characterControl.jump)
            {
                CharacterControlUtilities.StandardJump(ref characterBody,
                    characterBody.GroundingUp * movementComponent.jumpSpeed, true, characterBody.GroundingUp);
            }
        }
        else
        {
            // Move in air
            float3 airAcceleration = characterControl.moveVector * movementComponent.airAcceleration;
            if (math.lengthsq(airAcceleration) > 0f)
            {
                float3 tmpVelocity = characterBody.RelativeVelocity;
                CharacterControlUtilities.StandardAirMove(ref characterBody.RelativeVelocity, airAcceleration,
                    movementComponent.airMaxSpeed, characterBody.GroundingUp, deltaTime, false);

                // Cancel air acceleration from input if we would hit a non-grounded surface (prevents air-climbing slopes at high air accelerations)
                if (movementComponent.preventAirAccelerationAgainstUngroundedHits &&
                    CharacterAspect.MovementWouldHitNonGroundedObstruction(in this, ref context, ref baseContext,
                        characterBody.RelativeVelocity * deltaTime, out ColliderCastHit hit))
                {
                    characterBody.RelativeVelocity = tmpVelocity;
                }
            }

            // Gravity
            CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, movementComponent.gravity,
                deltaTime);

            // Drag
            CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime,
                movementComponent.airDrag);
        }
    }

    public void VariableUpdate(ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref CharacterMovementComponent movementComponent = ref MovementComponent.ValueRW;
        ref CharacterControlComponent characterControl = ref ControlComponent.ValueRW;
        ref quaternion characterRotation = ref CharacterAspect.LocalTransform.ValueRW.Rotation;

        // Add rotation from parent body to the character rotation
        // (this is for allowing a rotating moving platform to rotate your character as well, and handle interpolation properly)
        KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(ref characterRotation,
            characterBody.RotationFromParent, baseContext.Time.DeltaTime, characterBody.LastPhysicsUpdateDeltaTime);

        // Rotate towards move direction
        if (math.lengthsq(characterControl.moveVector) > 0f)
        {
            CharacterControlUtilities.SlerpRotationTowardsDirectionAroundUp(ref characterRotation,
                baseContext.Time.DeltaTime, math.normalizesafe(characterControl.moveVector),
                MathUtilities.GetUpFromRotation(characterRotation), movementComponent.rotationSharpness);
        }
    }

    #region Character Processor Callbacks

    public void UpdateGroundingUp(
        ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;

        CharacterAspect.Default_UpdateGroundingUp(ref characterBody);
    }

    public bool CanCollideWithHit(
        ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit)
    {
        return PhysicsUtilities.IsCollidable(hit.Material);
    }

    public bool IsGroundedOnHit(
        ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        in BasicHit hit, int groundingEvaluationType)
    {
        CharacterMovementComponent movementComponent = MovementComponent.ValueRO;

        return CharacterAspect.Default_IsGroundedOnHit(
            in this,
            ref context,
            ref baseContext,
            in hit,
            in movementComponent.stepAndSlopeHandling,
            groundingEvaluationType);
    }

    public void OnMovementHit(
        ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref KinematicCharacterHit hit,
        ref float3 remainingMovementDirection,
        ref float remainingMovementLength,
        float3 originalVelocityDirection,
        float hitDistance)
    {
        ref KinematicCharacterBody characterBody = ref CharacterAspect.CharacterBody.ValueRW;
        ref float3 characterPosition = ref CharacterAspect.LocalTransform.ValueRW.Position;
        CharacterMovementComponent movementComponent = MovementComponent.ValueRO;

        CharacterAspect.Default_OnMovementHit(
            in this,
            ref context,
            ref baseContext,
            ref characterBody,
            ref characterPosition,
            ref hit,
            ref remainingMovementDirection,
            ref remainingMovementLength,
            originalVelocityDirection,
            hitDistance,
            movementComponent.stepAndSlopeHandling.StepHandling,
            movementComponent.stepAndSlopeHandling.MaxStepHeight,
            movementComponent.stepAndSlopeHandling.CharacterWidthForStepGroundingCheck);
    }

    public void OverrideDynamicHitMasses(
        ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref PhysicsMass characterMass,
        ref PhysicsMass otherMass,
        BasicHit hit)
    {
        // Custom mass overrides
    }

    public void ProjectVelocityOnHits(
        ref CharacterMovementUpdateContext context,
        ref KinematicCharacterUpdateContext baseContext,
        ref float3 velocity,
        ref bool characterIsGrounded,
        ref BasicHit characterGroundHit,
        in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
        float3 originalVelocityDirection)
    {
        CharacterMovementComponent movementComponent = MovementComponent.ValueRO;

        CharacterAspect.Default_ProjectVelocityOnHits(
            ref velocity,
            ref characterIsGrounded,
            ref characterGroundHit,
            in velocityProjectionHits,
            originalVelocityDirection,
            movementComponent.stepAndSlopeHandling.ConstrainVelocityToGroundPlane);
    }

    #endregion
}