using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hsinpa.TV
{
    [ExecuteInEditMode]
    public class TexProjectionSource : MonoBehaviour
    {
        [SerializeField]
        private Texture texture;

        [SerializeField]
        private float textureHeight;

        [SerializeField]
        private float textureWidth;


        [SerializeField]
        private MeshRenderer meshRenderer;

        private Vector3 lookAtTarget = new Vector3();
        private Vector3 lookUpVector = new Vector3(0,1,0);

        Material projectMat;

        void Start()
        {
            projectMat = meshRenderer.sharedMaterial;
            projectMat.SetTexture("_ProjectTex", texture);
        }

        private void Update()
        {
            lookAtTarget = transform.position + (transform.forward * 5);

            var projMatrix = Matrix4x4.LookAt(transform.position, lookAtTarget, lookUpVector);

            Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(textureWidth, textureHeight, 1));

            projMatrix = projMatrix * scaleMatrix;
            projMatrix = Matrix4x4.Inverse(projMatrix);
            if (projectMat == null)
                Start();

            projectMat.SetMatrix("_TextureProjectMatrix", projMatrix);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, lookAtTarget);
        }
    }
}