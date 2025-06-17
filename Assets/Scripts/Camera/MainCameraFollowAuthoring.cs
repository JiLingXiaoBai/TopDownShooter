using Unity.Entities;
using UnityEngine;

[DisallowMultipleComponent]
public class MainCameraFollowAuthoring : MonoBehaviour
{
    private class MainCameraFollowBaker : Baker<MainCameraFollowAuthoring>
    {
        public override void Bake(MainCameraFollowAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<MainCameraFollowComponent>(entity);
        }
    }
}

public struct MainCameraFollowComponent : IComponentData
{
}