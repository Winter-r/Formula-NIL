using DA_Assets.Shared;
using UnityEngine;

#pragma warning disable IDE0003
#pragma warning disable CS0649

namespace DA_Assets.FCU
{
    internal class NovaComponentsTab : ScriptableObjectBinder<FcuSettingsWindow, FigmaConverterUnity>
    {
        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_nova_components.Localize(), FcuLocKey.tooltip_nova_components.Localize());
            gui.Space15();

            monoBeh.Settings.ComponentSettings.TextComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_text_component.Localize(), FcuLocKey.tooltip_text_component.Localize()),
                monoBeh.Settings.ComponentSettings.TextComponent, false);

            this.NovaSection.Draw();
            gui.Space15();

            switch (monoBeh.Settings.ComponentSettings.TextComponent)
            {
                case TextComponent.TextMeshPro:
                case TextComponent.RTLTextMeshPro:
                    this.TextMeshSettingsSection.Draw();
                    break;
            }
        }


        private TextMeshSection textMeshSettingsSection;
        internal TextMeshSection TextMeshSettingsSection => monoBeh.Bind(ref textMeshSettingsSection, scriptableObject);

        private NovaSection novaSection;
        internal NovaSection NovaSection => monoBeh.Bind(ref novaSection, scriptableObject);
    }
}
