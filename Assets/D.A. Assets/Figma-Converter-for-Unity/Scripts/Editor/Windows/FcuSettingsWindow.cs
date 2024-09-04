using DA_Assets.FCU.Extensions;
using DA_Assets.Shared;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable IDE0003
#pragma warning disable CS0649

namespace DA_Assets.FCU
{

    internal class FcuSettingsWindow : DAInspectorWindow<FcuSettingsWindow, FcuEditor, FigmaConverterUnity>
    {
        private List<UITab> _tabs = new List<UITab>();

        private int _selectedTab = 0;

        public override void OnShow()
        {
            CreateTabs();
        }

        public void CreateTabs()
        {         
            _tabs.Clear();

            if (monoBeh.Settings.MainSettings.WindowMode)
            {
                UITab assetTab = new UITab(FcuLocKey.label_asset.Localize(), null, this.AssetTab.Draw);
                _tabs.Add(assetTab);
            }

            UITab mainSettingTab = new UITab(FcuLocKey.label_main_settings.Localize(), null, this.MainSettingsTab.Draw, 150);
            _tabs.Add(mainSettingTab);

            if (monoBeh.IsUGUI())
            {
                UITab unityComponentsTab = new UITab(FcuLocKey.label_unity_comp.Localize(), FcuLocKey.tooltip_unity_comp.Localize(), this.UnityComponentsTab.Draw);
                _tabs.Add(unityComponentsTab);
            }

            if (monoBeh.IsNova())
            {
                UITab novaComponentsTab = new UITab(FcuLocKey.label_nova_components.Localize(), FcuLocKey.tooltip_nova_components.Localize(), this.NovaComponentsTab.Draw);
                _tabs.Add(novaComponentsTab);
            }

            UITab fontsTab = new UITab(FcuLocKey.fonts_settings.Localize(), null, this.FontsTab.Draw, 180);
            _tabs.Add(fontsTab);

            if (monoBeh.IsUGUI() || monoBeh.IsNova())
            {
                UITab prefabsTab = new UITab(FcuLocKey.label_prefabs.Localize(), null, this.PrefabSettingsTab.Draw, 180);
                _tabs.Add(prefabsTab);
            }

            //UITab scriptGeneratorTab = new UITab(FcuLocKey.label_script_generator.Localize(), FcuLocKey.tooltip_script_generator.Localize(), this.ScriptGeneratorTab.Draw);
            //_tabs.Add(scriptGeneratorTab);

            UITab importEventsTab = new UITab(FcuLocKey.label_import_events.Localize(), null, this.ImportEventsTab.Draw);
            _tabs.Add(importEventsTab);

            UITab definesTab = new UITab(FcuLocKey.label_dependencies.Localize(), null, this.DependenciesTab.Draw);
            _tabs.Add(definesTab);

            UITab debugTools = new UITab(FcuLocKey.label_debug.Localize(), FcuLocKey.tooltip_debug_tools.Localize(), this.DebugToolsTab.Draw);
            _tabs.Add(debugTools);

            _tabs[_selectedTab].Selected = true;
        }

        public override void DrawGUI()
        {
            if (_tabs.Count < 1)
            {
                return;
            }

            if (monoBeh.Settings.MainSettings.WindowMode)
            {
                titleContent = new GUIContent(FcuLocKey.label_fcu.Localize());
            }
            else
            {
                titleContent = new GUIContent(FcuLocKey.label_settings.Localize());
            }

            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Horizontal,
                Body = () =>
                {
                    DrawMenu();
                    DrawTabContent();
                }
            });
        }

        private void DrawMenu()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Style = GuiStyle.HamburgerTabsBg,
                Options = new[] { GUILayout.Width(200) },
                Scroll = true,
                InstanceId = monoBeh.GetInstanceID(),
                Body = () =>
                {
                    for (int i = 0; i < _tabs.Count; i++)
                    {
                        if (gui.TabButton(_tabs[i]))
                        {
                            _selectedTab = i;
                            _tabs[i].Selected = true;

                            for (int j = 0; j < _tabs.Count; j++)
                            {
                                if (i != j)
                                {
                                    _tabs[j].Selected = false;
                                }
                            }
                        }
                    }

                    gui.FlexibleSpace();

                    gui.DrawGroup(new Group
                    {
                        GroupType = GroupType.Horizontal,
                        Body = () =>
                        {
                            //TODO: return
                            //gui.Space10();
                            //gui.Label10px(FcuLocKey.label_beta_version.Localize(), widthType: WidthType.Expand);
                        }
                    });

                    gui.Space(7);
                }
            });
        }

        private void DrawTabContent()
        {
            gui.DrawGroup(new Group
            {
                GroupType = GroupType.Vertical,
                Style = GuiStyle.TabBg1,
                Scroll = true,
                InstanceId = monoBeh.GetInstanceID(),
                LabelWidth = _tabs[_selectedTab].LabelWidth,
                Body = () =>
                {
                    gui.DrawGroup(new Group
                    {
                        GroupType = GroupType.Vertical,
                        Style = GuiStyle.TabBg2,
                        Body = () =>
                        {
                            _tabs[_selectedTab].Content.Invoke();
                        }
                    });
                }
            });
        }


        private AssetTab assetTab;
        internal AssetTab AssetTab => monoBeh.Bind(ref assetTab, this);

        private FontsTab fontsTab;
        internal FontsTab FontsTab => monoBeh.Bind(ref fontsTab, this);

        private MainSettingsTab mainSettingsTab;
        internal MainSettingsTab MainSettingsTab => monoBeh.Bind(ref mainSettingsTab, this);

        private ScriptGeneratorTab scriptGeneratorTab;
        internal ScriptGeneratorTab ScriptGeneratorTab => monoBeh.Bind(ref scriptGeneratorTab, this);

        private UnityComponentsTab unityComponentsTab;
        internal UnityComponentsTab UnityComponentsTab => monoBeh.Bind(ref unityComponentsTab, this);

        private NovaComponentsTab novaComponentsTab;
        internal NovaComponentsTab NovaComponentsTab => monoBeh.Bind(ref novaComponentsTab, this);

        private ImportEventsTab importEventsTab;
        internal ImportEventsTab ImportEventsTab => monoBeh.Bind(ref importEventsTab, this);

        private DebugToolsTab debugToolsTab;
        internal DebugToolsTab DebugToolsTab => monoBeh.Bind(ref debugToolsTab, this);

        private DependenciesTab dependenciesTab;
        internal DependenciesTab DependenciesTab => monoBeh.Bind(ref dependenciesTab, this);

        private PrefabSettingsTab prefabSettingsTab;
        internal PrefabSettingsTab PrefabSettingsTab => monoBeh.Bind(ref prefabSettingsTab, this);
    }
}