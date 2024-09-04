using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DA_Assets.FCU
{
    [Serializable]
    public class PrefabUpdater : MonoBehaviourBinder<FigmaConverterUnity>
    {

        public IEnumerator UpdatePrefabs()
        {
#if UNITY_EDITOR
            List<SyncHelper> syncHelpers = monoBeh.SyncHelpers.GetAllSyncHelpers().ToList();
            syncHelpers = syncHelpers.OrderByDescending(x => x.HierarchyLevel).ToList();

            foreach (var obj in syncHelpers)
            {
                bool need  = IsNeedUpdatePrefab(obj.gameObject, out GameObject prefabAsset);

                if (!need)
                    continue;

                PrefabUtility.ApplyPrefabInstance(obj.gameObject, InteractionMode.UserAction);
                AssetDatabase.OpenAsset(prefabAsset);

#if UNITY_2021_3_OR_NEWER
                var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
                var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
                if (prefabStage != null)
                {
#if UNITY_2021_3_OR_NEWER
                    Debug.Log("Prefab editor: " + prefabStage.assetPath);
#endif
                    var rootPrefabObject = prefabStage.prefabContentsRoot;
                    List<SyncHelper> childs = rootPrefabObject.GetComponentsInReverseOrder<SyncHelper>();
                    syncHelpers = syncHelpers.OrderByDescending(x => x.HierarchyLevel).ToList();

                    foreach (var child in childs)
                    {
                        bool isNeedUpdatePrefab2 = IsNeedUpdatePrefab(child.gameObject, out GameObject prefabAsset2);
                        if (!isNeedUpdatePrefab2)
                            continue;

                        string prefabPath = GetPrefabPath(child.gameObject);
                        PrefabUtility.ApplyPrefabInstance(child.gameObject, InteractionMode.UserAction);
                        GameObject newPrefab = PrefabUtility.SaveAsPrefabAsset(child.gameObject, prefabPath);

                        Debug.Log($"{child.gameObject.name} | {prefabPath}");
                    }
                }
                else
                {
                    Debug.Log("Prefab editor is not active.");
                }
            }
#endif
                yield return null;
        }


        private bool IsNeedUpdatePrefab(GameObject obj, out GameObject prefabAsset)
        {
            prefabAsset = null;
#if UNITY_EDITOR
            if (!PrefabUtility.IsPartOfAnyPrefab(obj))
               return false;

            PrefabAssetType prefabType = PrefabUtility.GetPrefabAssetType(obj.gameObject);
            bool isPrefab = prefabType == PrefabAssetType.Regular || prefabType == PrefabAssetType.Variant;

            if (!isPrefab)
                return false;

            bool isGameObjectInsidePrefab = PrefabUtility.GetNearestPrefabInstanceRoot(obj.gameObject) == obj.gameObject;

            if (!isGameObjectInsidePrefab)
                return false;

            List<int> instanceHashCodes = new List<int>();
            AddHashCodesRecursively(obj.gameObject, instanceHashCodes);

            string prefabPath = GetPrefabPath(obj.gameObject);
            prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            List<int> prefabHashCodes = new List<int>();
            if (prefabAsset != null)
            {
                AddHashCodesRecursively(prefabAsset, prefabHashCodes);
            }

            bool objsEqual = CompareLists(instanceHashCodes, prefabHashCodes);
           
            if (!objsEqual)
            {
                Debug.Log($"UpdatePrefabs: {obj.gameObject.name}\n{string.Join(" ", instanceHashCodes)}\n{prefabAsset.name}: {string.Join(" ", prefabHashCodes)}");

            }

            return !objsEqual;
#else
            return false;
#endif
        }

        public string GetPrefabPath(GameObject prefab)
        {
#if UNITY_EDITOR
            var obj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(prefab);
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(obj);
            return path;
#else
            return null;
#endif
        }

        private void AddHashCodesRecursively(GameObject obj, List<int> hashCodes)
        {
            foreach (Transform child in obj.transform)
            {
                hashCodes.Add(child.GetHashCode());
                AddHashCodesRecursively(child.gameObject, hashCodes);
            }
        }

        private bool CompareLists(List<int> a, List<int> b)
        {
            if (a.Count != b.Count) return false;
            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
    }
}
