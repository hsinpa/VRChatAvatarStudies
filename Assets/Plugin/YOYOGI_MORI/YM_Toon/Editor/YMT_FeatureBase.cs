using UnityEditor;
using UnityEngine;

namespace YoyogiMori {
    public class YMT_FeatureBase {

        protected static MaterialEditor m_MaterialEditor = null;
        protected static void FindProps(YMToon2GUI ymtoon) { }
        public static void Draw(YMToon2GUI ymtoon, MaterialEditor materialEditor) { }

        protected static void DebugDraw(Material material) { }
        public static void DisableAllDebugDraw(Material material) { }
    }

}