using UnityEngine;
using UnityEngine.VFX;

namespace henningboat.CubeMarching.Runtime.Output.GeometryFieldSFDOutputSystem
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(VisualEffect))]
    public class WorldCollisionBinder : MonoBehaviour
    {
        [SerializeField] private GeometryFieldSFDOutput _sfd;
        private VisualEffect _effect;

        private void LateUpdate()
        {
            if (_effect == null) _effect = GetComponent<VisualEffect>();

            if (_sfd == null)
                return;
            _effect.SetTexture("WorldCollisionSDF", _sfd.SDF);
            _effect.SetVector3("WorldCollisionTransformation_position", _sfd.SDFPosition);
            _effect.SetVector3("WorldCollisionTransformation_angles", Vector3.zero);
            _effect.SetVector3("WorldCollisionTransformation_scale", _sfd.SDFScale);
        }
    }
}