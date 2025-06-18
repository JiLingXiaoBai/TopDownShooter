using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterControlAuthoring : MonoBehaviour
{
    private class CharacterControlBaker : Baker<CharacterControlAuthoring>
    {
        public override void Bake(CharacterControlAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CharacterControlComponent());
        }
    }
}
[Serializable]
public struct CharacterControlComponent : IComponentData
{
    public float2 moveVector;
    public float3 lookVector;
    public bool jump;
}