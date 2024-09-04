using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    public class AssetTools : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator ReselectFcu()
        {
            GameObject tempGo = MonoBehExtensions.CreateEmptyGameObject();
            yield return WaitFor.Delay01();
            tempGo.MakeGameObjectSelectedInHierarchy();
            yield return WaitFor.Delay01();
            SelectFcu();
            tempGo.Destroy();
        }

        public void SelectFcu()
        {
            monoBeh.gameObject.MakeGameObjectSelectedInHierarchy();
        }

        [HideInInspector, SerializeField] bool needShowRateMe;
        public bool NeedShowRateMe
        {
            get
            {
                if (needShowRateMe)
                {
#if UNITY_EDITOR
                    if (UnityEditor.EditorPrefs.GetInt(FcuConfig.RATEME_PREFS_KEY, 0) == 1)
                        return false;
#else
                    return false;
#endif
                }

                return needShowRateMe;
            }
            set => needShowRateMe = value;
        }

        private ResolutionData resolutionData;
        public ResolutionData ResolutionData { get => resolutionData; set => resolutionData = value; }

        public void CacheResolutionData()
        {
            bool received = monoBeh.DelegateHolder.GetGameViewSize(out Vector2 gameViewSize);

            this.ResolutionData = new ResolutionData
            {
                GameViewSizeReceived = received,
                GameViewSize = gameViewSize
            };
        }

        public IEnumerator DestroyChilds()
        {
            int childCount = monoBeh.transform.childCount;

            for (int i = childCount - 1; i >= 0; i--)
            {
                GameObject go = monoBeh.transform.GetChild(i).gameObject;
                go.Destroy();
                yield return WaitFor.Delay001();
            }

            DALogger.Log(FcuLocKey.log_current_canvas_childs_destroy.Localize(monoBeh.Guid, childCount));
            yield return null;
        }

        public IEnumerator DestroyLastImportedFrames()
        {
            foreach (SyncData syncData in monoBeh.CurrentProject.LastImportedFrames)
            {
                syncData.GameObject.Destroy();
            }

            monoBeh.CurrentProject.LastImportedFrames.Clear();
            yield return null;
        }

        public static void CreateFcuOnScene()
        {
            GameObject go = MonoBehExtensions.CreateEmptyGameObject();

            go.TryAddComponent(out FigmaConverterUnity fcu);
            go.name = string.Format(FcuConfig.Instance.CanvasGameObjectName, fcu.Guid);

            fcu.CanvasDrawer.AddCanvasComponent();
        }

        public void StopImport()
        {
            monoBeh.StopDARoutines();
        }

        public void RestoreResolutionData()
        {
            if (this.ResolutionData.GameViewSizeReceived)
            {
                monoBeh.DelegateHolder.SetGameViewSize(this.ResolutionData.GameViewSize);
            }
        }

        internal void ShowRateMe()
        {
            int componentsCount = monoBeh.TagSetter.TagsCounter.Values.Sum();
            int importErrorCount = monoBeh.AssetTools.GetConsoleErrorCount();

            if (importErrorCount > 0 || componentsCount < 1)
            {
                needShowRateMe = false;
                return;
            }

            needShowRateMe = true;
        }

        public static void MakeActiveSceneDirty()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif
        }

        public int GetConsoleErrorCount()
        {
#if UNITY_EDITOR
            try
            {
                Type logEntriesType = System.Type.GetType("UnityEditor.LogEntries, UnityEditor");
                if (logEntriesType == null)
                {
                    return 0;
                }

                MethodInfo getCountsByTypeMethod = logEntriesType.GetMethod("GetCountsByType", BindingFlags.Static | BindingFlags.Public);
                if (getCountsByTypeMethod == null)
                {
                    return 0;
                }

                int errorCount = 0;
                int warningCount = 0;
                int logCount = 0;
                object[] args = new object[] { errorCount, warningCount, logCount };

                getCountsByTypeMethod.Invoke(null, args);

                errorCount = (int)args[0];
                warningCount = (int)args[1];
                logCount = (int)args[2];

                return errorCount;
            }
            catch (Exception)
            {
                return 1;
            }
#else
            return 1;
#endif
        }

        public static int GetMaxFileNumber(string folderPath, string prefix, string extension)
        {
            string[] files = Directory.GetFiles(folderPath, $"{prefix}*.{extension}", SearchOption.AllDirectories);
            int maxNumber = -1;

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                int number = ExtractFileNumber(fileName, prefix);
                if (number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return maxNumber;
        }

        private static int ExtractFileNumber(string fileName, string prefix)
        {
            if (fileName == prefix)
            {
                return 0;
            }

            char[] separators = { ' ', '-', '_' };

            foreach (char separator in separators)
            {
                if (fileName.StartsWith(prefix + separator))
                {
                    string numberPart = fileName.Substring(prefix.Length + 1);
                    if (int.TryParse(numberPart, out int number))
                    {
                        return number;
                    }
                }
            }

            return -1;
        }
    }

    public struct ResolutionData
    {
        public bool GameViewSizeReceived { get; set; }
        public Vector2 GameViewSize { get; set; }
    }
}