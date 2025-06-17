using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerInputAuthoring : MonoBehaviour
{
    private class PlayerInputBaker : Baker<PlayerInputAuthoring>
    {
        public override void Bake(PlayerInputAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent<PlayerInputComponent>(entity);
        }
    }
}

[Serializable]
public struct PlayerInputComponent : IComponentData
{
    public float2 moveInput;
    public float2 lookInput;
    public FixedInputEvent jumpPressed;
}