using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DA_Assets.FCU
{
    internal class PrefabSettingsTab : ScriptableObjectBinder<FcuSettingsWindow, FigmaConverterUnity>
    {
        private List<GameObject> addedObjectsList = new List<GameObject>();
        private int currentIndex = -1;
        private Vector2 scrollPosition;

        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_prefab_settings.Localize());
            gui.Space15();

            monoBeh.Settings.PrefabSettings.PrefabsPath = gui.DrawSelectPathField(
                monoBeh.Settings.PrefabSettings.PrefabsPath,
                new GUIContent(FcuLocKey.label_prefabs_path.Localize(), FcuLocKey.tooltip_prefabs_path.Localize()),
                new GUIContent(FcuLocKey.label_change.Localize()),
                FcuLocKey.label_select_prefabs_folder.Localize());

            monoBeh.Settings.PrefabSettings.TextPrefabNameType = gui.EnumField(
                new GUIContent(FcuLocKey.label_text_prefab_naming_mode.Localize(), ""),
                monoBeh.Settings.PrefabSettings.TextPrefabNameType,
                false,
                new string[]
                {
                    FcuLocKey.label_humanized_color.Localize(),
                    FcuLocKey.label_hex_color.Localize(),
                    FcuLocKey.label_figma_color.Localize()
                });
            return;
            gui.Space30();

            gui.SectionHeader(FcuLocKey.label_find_added_objects.Localize());
            gui.Space15();

            if (GUILayout.Button("Find Added Objects"))
            {
                FindAddedObjectsInPrefabInstances();
                currentIndex = addedObjectsList.Count > 0 ? 0 : -1; // Reset current index
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("< Prev", GUILayout.Width(80), GUILayout.Height(20)))
            {
                SelectPreviousObject();
            }

            if (GUILayout.Button("Next >", GUILayout.Width(80), GUILayout.Height(20)))
            {
                SelectNextObject();
            }
            GUILayout.EndHorizontal();

            if (currentIndex != -1)
            {
                GUILayout.Label("Selected: " + addedObjectsList[currentIndex].name);
            }

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
            foreach (GameObject addedObject in addedObjectsList)
            {
                if (GUILayout.Button(addedObject.name))
                {
                    Selection.activeGameObject = addedObject;
                    EditorGUIUtility.PingObject(addedObject); // This will also highlight the object in the scene
                }
            }
            GUILayout.EndScrollView();
        }

        private void SelectPreviousObject()
        {
            if (addedObjectsList.Count > 0)
            {
                currentIndex--;
                if (currentIndex < 0)
                {
                    currentIndex = addedObjectsList.Count - 1; // Loop back to the end if we go too far back
                }
                Selection.activeGameObject = addedObjectsList[currentIndex];
            }
        }

        private void SelectNextObject()
        {
            if (addedObjectsList.Count > 0)
            {
                currentIndex++;
                if (currentIndex >= addedObjectsList.Count)
                {
                    currentIndex = 0; // Loop back to the start if we reach the end
                }
                Selection.activeGameObject = addedObjectsList[currentIndex];
            }
        }

        private void FindAddedObjectsInPrefabInstances()
        {
            List<GameObject> allObjectsInScene = monoBeh.gameObject.GetComponentsInReverseOrder<Transform>().Select(x=>x.gameObject).ToList();  
            HashSet<GameObject> addedObjects = new HashSet<GameObject>();
            addedObjectsList.Clear(); // Clear the previous list

            foreach (GameObject go in allObjectsInScene)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(go))
                {
                    GameObject prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(go);
                    if (prefabRoot != null)
                    {
                        var addedGameObjects = PrefabUtility.GetAddedGameObjects(prefabRoot);
                        foreach (var addedGameObject in addedGameObjects)
                        {
                            if (addedObjects.Add(addedGameObject.instanceGameObject)) // Add returns false if the item was already present
                            {
                                addedObjectsList.Add(addedGameObject.instanceGameObject);
                                Debug.Log($"Added GameObject: {addedGameObject.instanceGameObject.name} to Prefab: {prefabRoot.name}", addedGameObject.instanceGameObject);
                            }
                        }
                    }
                }
            }

            if (addedObjectsList.Count == 0)
            {
                Debug.Log("No added GameObjects found in prefab instances.");
            }
            else
            {
                Selection.activeGameObject = addedObjectsList[0]; // Select the first found object
            }
        }
    }
}