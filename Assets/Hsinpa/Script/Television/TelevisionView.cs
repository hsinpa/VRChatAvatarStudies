using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hsinpa.TV {
    public class TelevisionView : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer meshRendering;

        [SerializeField]
        private MeshFilter meshFilter;

        public Vector3 size => meshFilter.sharedMesh.bounds.size;

        public void SetTexture(Texture texture)
        {
            meshRendering.material.SetTexture(Hsinpa.StaticFlag.Shader.MainTexture, texture);
        }
    }
}