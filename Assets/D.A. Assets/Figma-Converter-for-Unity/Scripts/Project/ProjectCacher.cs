using DA_Assets.FCU.Drawers;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    public class ProjectCacher : MonoBehaviourBinder<FigmaConverterUnity>
    {
        internal void Cache<T>(T @object)
        {
            try
            {
                FigmaProject figmaProject = (FigmaProject)Convert.ChangeType(@object, typeof(FigmaProject));

                string projectUrl = monoBeh.Settings.MainSettings.ProjectUrl;

                SelectableFObject doc = monoBeh.InspectorDrawer.FillSelectableFramesArray(figmaProject.Document);

                ProjectCache projectCache = new ProjectCache
                {
                    Url = projectUrl,
                    Name = figmaProject.Name,
                    DateTime = DateTime.Now,
                    Project = doc
                };

                List<ProjectCache> cachedProjects = GetCachedProjects();
                cachedProjects.RemoveAll(pc => pc.Url == projectUrl);
                cachedProjects.Insert(0, projectCache);

                if (cachedProjects.Count > FcuConfig.Instance.CachedFrameListsLimit)
                {
                    cachedProjects = cachedProjects.Take(FcuConfig.Instance.CachedFrameListsLimit).ToList();
                }


                SaveAll(cachedProjects);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public List<ProjectCache> GetCachedProjects()
        {
#if UNITY_EDITOR
            string savedData = UnityEditor.EditorPrefs.GetString(FcuConfig.Instance.CachedProjectsPrefsKey, "");

            if (string.IsNullOrWhiteSpace(savedData))
            {
                return new List<ProjectCache>();
            }

            List<ProjectCache> cachedProjects = DAJson.FromJson<List<ProjectCache>>(savedData);

            if (cachedProjects != null && cachedProjects.Count > 0)
            {
                return cachedProjects.OrderByDescending(x => x.DateTime).ToList();
            }
            else
            {
                return new List<ProjectCache>();
            }
#else
            return new List<ProjectCache>();
#endif
        }

        private void SaveAll(List<ProjectCache> cachedProjects)
        {
            if (cachedProjects == null)
            {
                cachedProjects = new List<ProjectCache>();
            }

            string json = DAJson.ToJson(cachedProjects);
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetString(FcuConfig.Instance.CachedProjectsPrefsKey, json);
#endif
        }

        public void TryRestoreFrameList()
        {
            List<ProjectCache> cachedProjects = GetCachedProjects();

            foreach (ProjectCache item in cachedProjects)
            {
                if (item.Url == monoBeh.Settings.MainSettings.ProjectUrl)
                {
                    monoBeh.InspectorDrawer.SelectableDocument = item.Project;

                    FigmaProject p = monoBeh.CurrentProject.FigmaProject;
                    p.Name = item.Name;
                    monoBeh.CurrentProject.FigmaProject = p;   

                    if (item.Project != null)
                    {
                        DALogger.Log(FcuLocKey.log_cache_restored.Localize(item.Project.Childs.Count));
                    }

                    break;
                }
            }
        }
    }


    [Serializable]
    public struct ProjectCache
    {
        public string Url { get; set; }
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
        public SelectableFObject Project { get; set; }
    }
}