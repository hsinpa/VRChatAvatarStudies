using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hsinpa.Utility;

namespace Hsinpa.Mesh {
    public class SkinMeshBoneCombiner : MonoBehaviour
    {
        [SerializeField]
        private SkinnedMeshRenderer skinMesh;

        [SerializeField]
        private Transform referenceRootBone;

        [SerializeField]
        private bool debugFlag;

        private Dictionary<string, Transform> _cacheBoneDict;

        public void SmartSetSkinBone()
        {
            //if (_cacheBoneDict == null)
                _cacheBoneDict = CacheRootData(referenceRootBone);

            var bones = skinMesh.bones;
            int boneCount = bones.Length;

            Debug.Log("BoneCount " + boneCount);

            for (int i = 0; i < boneCount; i++) {
                if (bones[i] == null) continue;

                if (debugFlag) {
                    Debug.Log("Object Name " + bones[i].name);
                    Debug.Log("Object Path " + UtilityFunc.GetGameObjectPath(bones[i]));

                    continue;
                }

                if (_cacheBoneDict.TryGetValue(bones[i].name, out Transform refBone))
                {
                    bones[i] = refBone;
                }
                else {
                    Debug.Log("Missed : " + bones[i].name);
                    bones[i] = null;
                }
            }

            skinMesh.bones = bones;
        }

        #region Private API
        private Dictionary<string, Transform> CacheRootData(Transform referenceRootBone) {
            var boneDict = new Dictionary<string, Transform>();

            var openBone = new Queue<Transform>();
            openBone.Enqueue(referenceRootBone);

            while (openBone.Count > 0) {

                Transform currentBone = openBone.Dequeue();

                if (boneDict.ContainsKey(currentBone.name))
                    continue;

                boneDict.Add(currentBone.name, currentBone);

                if (currentBone.childCount > 0) { 
                    foreach (Transform childT in currentBone)
                    {
                        openBone.Enqueue(childT);
                    }
                }
            }

            Debug.Log("Find cache ref bones " + boneDict.Count);

            return boneDict;
        }

        #endregion

    }
}