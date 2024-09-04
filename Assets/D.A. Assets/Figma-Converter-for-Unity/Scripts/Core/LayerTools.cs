using UnityEngine;

namespace DA_Assets.FCU
{
    internal class LayerTools
    {
        internal static int AddLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);

            if (layer != -1)
                return layer;

#if UNITY_EDITOR
            UnityEditor.SerializedObject tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            UnityEditor.SerializedProperty layersProp = tagManager.FindProperty("layers");

            for (int i = 8; i < layersProp.arraySize; i++)
            {
                UnityEditor.SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (sp != null && sp.stringValue == "")
                {
                    sp.stringValue = layerName;
                    tagManager.ApplyModifiedProperties();
                    break;
                }
            }
#endif

            layer = LayerMask.NameToLayer(layerName);

            return layer;
        }
    }
}
