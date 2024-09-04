using UnityEngine;

namespace DA_Assets.FCU
{
    internal class CameraTools
    {
        internal static void ExcludeBlurFromCulling(Camera camera)
        {
            int layer = LayerTools.AddLayer(FcuConfig.Instance.BlurredObjectTag);
            camera.cullingMask &= ~(1 << layer);
        }

        internal static Camera GetOrCreateMainCamera()
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("MainCamera");
                mainCamera = cameraObject.AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
            }

            return mainCamera;
        }

        internal static Camera GetOrCreateBackgroundBlurCamera()
        {
            if (!IsTagExists(FcuConfig.Instance.BlurCameraTag))
            {
                AddTag(FcuConfig.Instance.BlurCameraTag);
            }

            Camera camera;
            GameObject bgBlurCamObj = GameObject.FindGameObjectWithTag(FcuConfig.Instance.BlurCameraTag);

            if (bgBlurCamObj == null)
            {
                GameObject cameraObject = new GameObject($"{FcuConfig.Instance.BlurCameraTag}Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = FcuConfig.Instance.BlurCameraTag;
            }
            else
            {
                camera = bgBlurCamObj.GetComponent<Camera>();
            }

            return camera;
        }

        private static bool IsTagExists(string tag)
        {
#if UNITY_EDITOR
            foreach (string t in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (t.Equals(tag))
                {
                    return true;
                }
            }
#endif
            return false;
        }

        private static void AddTag(string tag)
        {
#if UNITY_EDITOR
            UnityEditor.SerializedObject tagManager = new UnityEditor.SerializedObject(UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            UnityEditor.SerializedProperty tagsProp = tagManager.FindProperty("tags");

            int tagsCount = tagsProp.arraySize;
            tagsProp.InsertArrayElementAtIndex(tagsCount);
            UnityEditor.SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(tagsCount);
            newTagProp.stringValue = tag;
            tagManager.ApplyModifiedProperties();
#endif
        }
    }
}
