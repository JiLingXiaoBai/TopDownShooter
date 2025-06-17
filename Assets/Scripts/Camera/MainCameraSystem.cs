using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class MainCameraSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (SystemAPI.TryGetSingletonEntity<MainCameraFollowComponent>(out Entity mainCameraFollowEntity))
        {
            LocalToWorld localToWorld = SystemAPI.GetComponent<LocalToWorld>(mainCameraFollowEntity);
            MainCameraFollowGameObject.Instance.transform.SetPositionAndRotation(localToWorld.Position,
                localToWorld.Rotation);
        }
    }
}