using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DA_Assets.Shared
{
    internal class SpriteRemoverWindow : EditorWindow
    {
        [SerializeField] string spritesPath = "Assets\\Sprites";
        private static Vector2 windowSize = new Vector2(500, 150);
        private DAInspector gui => DAInspector.Instance;

        [MenuItem("Tools/" + DAConstants.Publisher + "/" + nameof(DA_Assets.Shared) + ": Remove unused sprites", false, 90)]
        public static void ShowWindow()
        {
            SpriteRemoverWindow win = GetWindow<SpriteRemoverWindow>("Remove unused sprites");
            win.maxSize = windowSize;
            win.minSize = windowSize;

            win.position = new Rect(
                (Screen.currentResolution.width - windowSize.x * 2) / 2,
                (Screen.currentResolution.height - windowSize.y * 2) / 2,
                windowSize.x,
                windowSize.y);
        }

        private void OnGUI()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Style = GuiStyle.TabBg2,
                Body = () =>
                {
                    gui.Label12px($@"Remove sprites from the selected folder that are not used by
Image components in the current open scene.", widthType: WidthType.Expand);

                    gui.Space15();

                    spritesPath = gui.DrawSelectPathField(
                        spritesPath,
                        new GUIContent($"Sprites Path"),
                        new GUIContent($"…"),
                       $"Select folder");

                    gui.Space15();

                    if (gui.OutlineButton($"Remove"))
                    {
                        RemoveCurrentSceneUnusedSprites().StartDARoutine(null);
                    }
                }
            });
        }

        public IEnumerator RemoveCurrentSceneUnusedSprites()
        {
#if UNITY_EDITOR
            Image[] images;

#if UNITY_2023_3_OR_NEWER
            images = MonoBehaviour.FindObjectsByType<Image>(FindObjectsSortMode.None);
#else
            images = MonoBehaviour.FindObjectsOfType<Image>();
#endif

            var sceneSpritePathes = images
                .Where(x => x.sprite != null)
                .Select(x => AssetDatabase.GetAssetPath(x.sprite));

            var assetSpritePathes = AssetDatabase.FindAssets($"t:{typeof(Sprite).Name}", new string[]
            {
                spritesPath
            }).Select(x => AssetDatabase.GUIDToAssetPath(x));

            var result = assetSpritePathes.Where(x1 => sceneSpritePathes.All(x2 => x2 != x1));

            foreach (var filePath in result)
            {
                File.Delete(filePath.GetFullAssetPath());
            }

            DALogger.Log($"{result.Count()} sprites removed.");

            AssetDatabase.Refresh();
#endif
            yield return null;
        }
    }
}