using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using UnityEditor;
using UnityEngine;

namespace DA_Assets.FCU
{
    [CustomEditor(typeof(SyncHelper)), CanEditMultipleObjects]
    internal class SyncHelperEditor : Editor
    {
        private DAInspector gui => DAInspector.Instance;
        private FigmaConverterUnity monoBeh;
        private SyncHelper syncObject;

        private void OnEnable()
        {
            syncObject = (SyncHelper)target;
            monoBeh = syncObject.Data.FigmaConverterUnity;
        }

        public override void OnInspectorGUI()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Style = GuiStyle.Background,
                DarkBg = true,
                Body = () =>
                {
                    if (monoBeh == null)
                    {
                        gui.Label12px(FcuLocKey.label_dont_remove_fcu_meta.Localize(), widthType: WidthType.Expand);
                        gui.Label10px(FcuLocKey.label_more_about_layout_updating.Localize(), widthType: WidthType.Expand);

                        gui.Space10();

                        gui.Label10px(FcuLocKey.label_fcu_is_null.Localize(nameof(FigmaConverterUnity), FcuConfig.CreatePrefabs, FcuConfig.SetFcuToSyncHelpers), widthType: WidthType.Expand);
                        return;
                    }

                    if (monoBeh.IsUITK())
                    {
                        GUILayout.TextArea(syncObject.Data.NameHierarchy);
                    }

                    if (monoBeh.IsDebug())
                    {
                        gui.Space10();
                        EditorGUILayout.Vector3Field("World Position", syncObject.transform.position);
                        EditorGUILayout.Vector3Field("Local Position", syncObject.transform.localPosition);
                        gui.Space10();
                        base.OnInspectorGUI();
                    }

                    if (monoBeh.IsUITK() || monoBeh.IsDebug())
                        gui.Space10();

                    gui.Label12px(FcuLocKey.label_dont_remove_fcu_meta.Localize(), widthType: WidthType.Expand);
                    gui.Label10px(FcuLocKey.label_more_about_layout_updating.Localize(), widthType: WidthType.Expand);
                }
            });
        }
    }
}