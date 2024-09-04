using DA_Assets.Shared.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DA_Assets.Shared
{
    public class ContextMenuItems : MonoBehaviour
    {
        public const string ResetToPrefabState = "Reset to prefab state";
        public const string ResetAllComponents = "Reset all components to prefab state";

        [MenuItem("GameObject/Tools/" + DAConstants.Publisher + "/" + nameof(DA_Assets.Shared) + ": " + "Simplify the hierarchy", false, 90)]
        private static void SetSelectedAsParentForAllChilds_OnClick()
        {
            GameObject selectedGameObject = Selection.activeGameObject;

            if (selectedGameObject == null)
            {
                DALogger.LogError(string.Format(nameof(GameObject), "'{0}' not selected in hierarchy."));
                return;
            }

            List<Transform> childs = new List<Transform>();
            SetSelectedAsParentForAllChild(selectedGameObject);
            foreach (Transform child in childs)
            {
                child.SetParent(selectedGameObject.transform);
            }

            void SetSelectedAsParentForAllChild(GameObject @object)
            {
                if (@object == null)
                    return;

                foreach (Transform child in @object.transform)
                {
                    if (child == null)
                        continue;

                    childs.Add(child);

                    SetSelectedAsParentForAllChild(child.gameObject);
                }
            }
        }

        [MenuItem("GameObject/Tools/" + DAConstants.Publisher + "/" + nameof(DA_Assets.Shared) + ": " + ResetToPrefabState, false, 91)]
        private static void ResetToPrefabState_OnClick()
        {
            GameObject selectedGameObject = Selection.activeGameObject;

            if (selectedGameObject == null)
            {
                DALogger.LogError(string.Format(nameof(GameObject), "'{0}' not selected in hierarchy."));
                return;
            }

            PrefabUtility.RevertPrefabInstance(Selection.activeGameObject, InteractionMode.AutomatedAction);

            DALogger.Log(string.Format(selectedGameObject.name, "'{0}' has been reset to a prefab state."));
        }

        [MenuItem("GameObject/Tools/" + DAConstants.Publisher + "/" + nameof(DA_Assets.Shared) + ": " + ResetAllComponents, false, 92)]
        private static void ResetAllComponents_OnClick()
        {
            GameObject selectedGameObject = Selection.activeGameObject;

            if (selectedGameObject == null)
            {
                DALogger.LogError(string.Format(nameof(GameObject), "'{0}' not selected in hierarchy."));
                return;
            }

            Component[] components = selectedGameObject.GetComponents<Component>();

            if (components.IsEmpty())
            {
                DALogger.LogError(string.Format(selectedGameObject.name, "No components in '{0}'."));
                return;
            }

            int count = 0;

            foreach (var item in components)
            {
                SerializedObject serializedObject = new SerializedObject(item);
                SerializedProperty propertyIterator = serializedObject.GetIterator();

                while (propertyIterator.NextVisible(true))
                {
                    PrefabUtility.RevertPropertyOverride(propertyIterator, InteractionMode.AutomatedAction);
                    count++;
                }
            }

            DALogger.Log(string.Format(count.ToString(), "{0} properties reset."));
        }
    }
}