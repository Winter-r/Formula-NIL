using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using UnityEngine;

#pragma warning disable IDE0003
#pragma warning disable CS0649

namespace DA_Assets.FCU
{
    internal class ScriptGeneratorTab : ScriptableObjectBinder<FcuSettingsWindow, FigmaConverterUnity>
    {
        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_script_generator.Localize());
            gui.Space15();

            monoBeh.Settings.ScriptGeneratorSettings.IsEnabled = gui.Toggle(
                new GUIContent(FcuLocKey.label_enabled.Localize()),
                monoBeh.Settings.ScriptGeneratorSettings.IsEnabled);

            monoBeh.Settings.ScriptGeneratorSettings.Namespace = gui.TextField(
                new GUIContent(FcuLocKey.label_namespace.Localize()),
                monoBeh.Settings.ScriptGeneratorSettings.Namespace);

            monoBeh.Settings.ScriptGeneratorSettings.OutputPath = gui.DrawSelectPathField(
                monoBeh.Settings.ScriptGeneratorSettings.OutputPath,
                new GUIContent(FcuLocKey.label_scripts_output_path.Localize()),
                new GUIContent(FcuLocKey.label_change.Localize()),
                FcuLocKey.label_select_folder.Localize());
        }
    }
}
