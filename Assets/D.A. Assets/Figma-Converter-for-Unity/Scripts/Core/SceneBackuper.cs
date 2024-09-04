using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DA_Assets.FCU
{
    public class SceneBackuper
    {
        public static void BackupActiveScene()
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                bool sceneFileExists = File.Exists(activeScene.path);
                string newName = $"{DateTime.Now.ToString(FcuConfig.Instance.DateTimeFormat2)}_{activeScene.name}.unity";
                string filePath;

                if (sceneFileExists)
                {
                    string backupsPath = GetBackupsPath();
                    backupsPath.CreateFolderIfNotExists();
                    filePath = Path.Combine(backupsPath, newName);
                }
                else
                {
                    string assetsScenesPath = Path.Combine(Application.dataPath, "Scenes");
                    assetsScenesPath = assetsScenesPath.Replace("\\", "/"); 
                    assetsScenesPath.CreateFolderIfNotExists();
                    filePath = Path.Combine(assetsScenesPath, newName);
                }

                if (sceneFileExists)
                {
                    File.Copy(activeScene.path, filePath);
                }
                else
                {
#if UNITY_EDITOR
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene, filePath);
#endif
                }

                DALogger.Log(FcuLocKey.log_scene_backup_created.Localize(filePath));
            }
            catch (Exception ex)
            {
                DALogger.LogError(FcuLocKey.log_scene_backup_creation_error.Localize(ex.ToString()));
            }
        }

        private static string GetProjectAbsolutePath()
        {
            string[] parts = Application.dataPath.Split('/');
            string path = "";

            for (int i = 0; i < parts.Length - 1; i++) //Iterate through all parts except the last one.
            {
                if (i > 0) path += "\\"; //Add a path separator for all but the first element.
                path += parts[i];
            }

            return path;
        }

        public static string GetBackupsPath()
        {
            string @base = GetProjectAbsolutePath();
            string path = Path.Combine(@base, "Library", "Backup", "Scenes"); 
            return path;
        }
    }
}