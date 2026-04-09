using Unity.Entities;
using UnityEngine;

public class CameraAuthoring : MonoBehaviour
{
    public class Baker : Baker<CameraAuthoring>
    {
        public override void Bake(CameraAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent<CameraTargetTag>(entity);
        }
    }
}