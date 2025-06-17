using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class MainCameraSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (MainCameraGameObject.Instance != null &&
            SystemAPI.TryGetSingletonEntity<MainCameraFollowComponent>(out Entity mainCameraFollowEntity))
        {
            LocalToWorld localToWorld = SystemAPI.GetComponent<LocalToWorld>(mainCameraFollowEntity);
            MainCameraGameObject.Instance.follow.SetPositionAndRotation(localToWorld.Position,
                localToWorld.Rotation);
        }
    }
}