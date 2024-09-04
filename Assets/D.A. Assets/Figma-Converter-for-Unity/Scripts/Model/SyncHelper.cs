using System;
using UnityEngine;

namespace DA_Assets.FCU.Model
{
    //TODO: set current GameObject to SyncHelper
    [Serializable]
    public class SyncHelper : MonoBehaviour
    {
        void OnValidate()
        {
            try
            {
                data.DisplayNameHierarchyInField();
            }
            catch
            {

            }
        }

        [SerializeField] SyncData data;
        public SyncData Data { get => data; set => data = value; }

        public int HierarchyLevel
        {
            get
            {
                int level = 0;
                Transform current = transform;

                while (current.parent != null)
                {
                    level++;
                    current = current.parent;
                }

                return level;
            }
        }
    }

    public static class TempExtensions
    {

        public static void SetData(this FObject fobject, SyncHelper syncHelper, FigmaConverterUnity fcu)
        {
            fobject.Data.Id = fobject.Id;
            fobject.Data.FigmaConverterUnity = fcu;
            fobject.Data.GameObject = syncHelper.gameObject;
            //fobject.Data.GameObject.name = fobject.Data.NewName;

            syncHelper.Data = fobject.Data;

            if (fobject.Type == NodeType.TEXT)
            {
                fobject.Data.HumanizedTextPrefabName = fcu.NameHumanizer.GetHumanizedTextPrefabName(fobject);
            }
        }

        public static bool TryGetChild<T>(this Transform parent, int index, out T child) where T : MonoBehaviour
        {
            try
            {
                Transform childTransform = parent.GetChild(index);
                return childTransform.TryGetComponent(out child);
            }
            catch
            {
                child = default;
                return false;
            }
        }
    }
}
