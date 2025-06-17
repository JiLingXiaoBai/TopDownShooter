using System;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterMovementAuthoring : MonoBehaviour
{
    public AuthoringKinematicCharacterProperties characterProperties = AuthoringKinematicCharacterProperties.GetDefault();

    public float rotationSharpness = 25f;
    public float groundMaxSpeed = 10f;
    public float groundedMovementSharpness = 15f;
    public float airAcceleration = 50f;
    public float airMaxSpeed = 10f;
    public float airDrag = 0f;
    public float jumpSpeed = 10f;
    public float3 gravity = math.down() * 30f;
    public bool preventAirAccelerationAgainstUngroundedHits = true;
    public BasicStepAndSlopeHandlingParameters stepAndSlopeHandling = BasicStepAndSlopeHandlingParameters.GetDefault();
    private class CharacterMovementBaker : Baker<CharacterMovementAuthoring>
    {
        public override void Bake(CharacterMovementAuthoring authoring)
        {
            KinematicCharacterUtilities.BakeCharacter(this, authoring.gameObject, authoring.characterProperties);
            Entity entity = GetEntity(TransformUsageFlags.Dynamic | TransformUsageFlags.WorldSpace);
            AddComponent(entity, new CharacterMovementComponent
            {
                rotationSharpness = authoring.rotationSharpness,
                groundMaxSpeed = authoring.groundMaxSpeed,
                groundedMovementSharpness = authoring.groundedMovementSharpness,
                airAcceleration = authoring.airAcceleration,
                airMaxSpeed = authoring.airMaxSpeed,
                airDrag = authoring.airDrag,
                jumpSpeed = authoring.jumpSpeed,
                gravity = authoring.gravity,
                preventAirAccelerationAgainstUngroundedHits = authoring.preventAirAccelerationAgainstUngroundedHits,
                stepAndSlopeHandling = authoring.stepAndSlopeHandling,
            });
        }
    }
}

[Serializable]
public struct CharacterMovementComponent : IComponentData
{
    public float rotationSharpness;
    public float groundMaxSpeed;
    public float groundedMovementSharpness;
    public float airAcceleration;
    public float airMaxSpeed;
    public float airDrag;
    public float jumpSpeed;
    public float3 gravity;
    public bool preventAirAccelerationAgainstUngroundedHits;
    public BasicStepAndSlopeHandlingParameters stepAndSlopeHandling;
}





 