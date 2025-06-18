using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class MainCameraSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (MainPlayerGameObject.Instance != null &&
            SystemAPI.TryGetSingletonEntity<MainCameraFollowComponent>(out Entity mainCameraFollowEntity))
        {
            LocalToWorld localToWorld = SystemAPI.GetComponent<LocalToWorld>(mainCameraFollowEntity);
            MainPlayerGameObject.Instance.cameraFollow.SetPositionAndRotation(localToWorld.Position,
                localToWorld.Rotation);
        }
    }
}