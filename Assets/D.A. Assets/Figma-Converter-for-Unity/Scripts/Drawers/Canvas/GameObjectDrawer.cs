using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class GameObjectDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator Draw(FObject parent)
        {
            DALogger.Log(FcuLocKey.log_instantiate_game_objects.Localize());

            yield return WaitFor.Delay(WaitFor.Delay01().WaitTimeF);
            DrawFObject(parent);
        }

        public void DrawFObject(FObject parent)
        {
            for (int i = 0; i < parent.Children.Count; i++)
            {
                FObject fobject = parent.Children[i];

                if (fobject.Data.Ignore)
                    continue;

                SyncHelper syncHelper;

                if (monoBeh.SyncHelpers.IsExistsOnCurrentCanvas(fobject, out syncHelper))
                {
                    monoBeh.Log($"InstantiateGameObjects | 1 | {fobject.Data.NameHierarchy}", FcuLogType.GameObjectDrawer);
                }
                else if (monoBeh.CurrentProject.HasLocalPrefab(fobject.Data, out SyncHelper localPrefab))
                {
                    monoBeh.Log($"InstantiateGameObjects | 2 | {fobject.Data.NameHierarchy}", FcuLogType.GameObjectDrawer);
#if UNITY_EDITOR
                    syncHelper = (SyncHelper)UnityEditor.PrefabUtility.InstantiatePrefab(localPrefab);
#endif
                    int counter = 0;
                    monoBeh.SyncHelpers.SetFcuToAllChilds(syncHelper.gameObject, ref counter);

                    SetFigmaIds(fobject, syncHelper);
                    monoBeh.Events.OnObjectInstantiate?.Invoke(monoBeh, fobject.Data.GameObject);
                }
                else
                {
                    monoBeh.Log($"InstantiateGameObjects | 3 | {fobject.Data.NameHierarchy}", FcuLogType.GameObjectDrawer);
                    syncHelper = MonoBehExtensions.CreateEmptyGameObject().AddComponent<SyncHelper>();
                    monoBeh.Events.OnObjectInstantiate?.Invoke(monoBeh, fobject.Data.GameObject);
                }

                fobject.SetData(syncHelper, monoBeh);
                fobject.Data.GameObject.TryAddComponent(out RectTransform rt);

                int goLayer;

                if (fobject.ContainsTag(FcuTag.Blur))
                {
                    goLayer = LayerTools.AddLayer(FcuConfig.Instance.BlurredObjectTag);
                }
                else
                {
                    goLayer = monoBeh.Settings.MainSettings.GameObjectLayer;
                }

                fobject.Data.GameObject.layer = goLayer;

                if (!fobject.Data.GameObject.transform.parent.IsPartOfAnyPrefab())
                {
                    fobject.Data.GameObject.transform.SetParent(parent.Data.GameObject.transform);
                }

                fobject.Data.GameObject.name = fobject.Data.ObjectName;

                if (fobject.Children.IsEmpty())
                    continue;

                DrawFObject(fobject);
            }
        }

        private void SetFigmaIds(FObject rootFObject, SyncHelper rootSyncObject)
        {
            Dictionary<string, int> items = new Dictionary<string, int>();

            foreach (var childIndex in rootFObject.Data.ChildIndexes)
            {
                if (monoBeh.CurrentProject.TryGetByIndex(childIndex, out FObject childFO))
                {
                    items.Add(childFO.Id, childFO.Data.Hash);
                }
            }

            SyncHelper[] soChilds = rootSyncObject.GetComponentsInChildren<SyncHelper>(true);

            foreach (var soChild in soChilds)
            {
                string idToRemove = null;

                foreach (var item in items)
                {
                    if (item.Value == soChild.Data.Hash)
                    {
                        idToRemove = item.Key;
                        break;
                    }
                }

                if (idToRemove == null)
                    continue;

                items.Remove(idToRemove);
                soChild.Data.Id = idToRemove;

                if (monoBeh.CurrentProject.TryGetById(idToRemove, out FObject gbi))
                {
                    SetFigmaIds(gbi, soChild);
                }
            }
        }

        public IEnumerator DestroyMissing(IEnumerable<SyncData> diffCheckResult)
        {
            foreach (SyncData item in diffCheckResult)
            {
                try
                {
                    monoBeh.Log($"DestroyMissing | {item.NameHierarchy}");
                    item.GameObject.Destroy();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(ex);
                }

                yield return null;
            }
        }

        public IEnumerator DestroyMissing(List<FObject> fobjects)
        {
            SyncHelper[] syncHelpers = monoBeh.SyncHelpers.GetAllSyncHelpers();

            ConcurrentBag<SyncHelper> toDestroy = new ConcurrentBag<SyncHelper>();

            Parallel.ForEach(syncHelpers, syncHelper =>
            {
                bool find = false;

                foreach (FObject fobject in fobjects)
                {
                    if (syncHelper.Data.Id == fobject.Data.Id)
                    {
                        find = true;
                        break;
                    }
                }

                if (find == false)
                {
                    monoBeh.Log($"DestroyMissing | {syncHelper.Data.NameHierarchy}");
                    toDestroy.Add(syncHelper);
                }
            });

            foreach (SyncHelper sh in toDestroy)
            {
                try
                {
                    sh.gameObject.Destroy();
                }
                catch
                {

                }

                yield return null;
            }
        }
    }
}
