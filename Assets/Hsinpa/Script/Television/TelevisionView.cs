using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.TV {
    public class TelevisionView : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer meshRenderer;

        [SerializeField]
        private MeshFilter meshFilter;

        public Vector3 size => meshFilter.sharedMesh.bounds.size;

        private MaterialPropertyBlock m_PropertyBlock;

        public void SetUp() {
            m_PropertyBlock = new MaterialPropertyBlock();
        }

        public void SetTexture(Texture texture)
        {
            meshRenderer.sharedMaterial.SetTexture(Hsinpa.StaticFlag.Shader.MainTexture, texture);
        }

        public void SetUVOffset(Vector2 uvOffset, Vector2 uvScale) {
            m_PropertyBlock.SetVector(StaticFlag.Shader.UVOffset, uvOffset);
            m_PropertyBlock.SetVector(StaticFlag.Shader.UVScale, uvScale);

            meshRenderer.SetPropertyBlock(m_PropertyBlock);
        }
    }
}