using UnityEngine;

namespace DA_Assets.FCU
{
    internal class DttPuiSection : BaseImageSection
    {
        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_procedural_ui_settings.Localize());
            gui.Space15();

            DrawBase(monoBeh.Settings.DttPuiSettings);

            monoBeh.Settings.DttPuiSettings.FalloffDistance = gui.FloatField(new GUIContent(FcuLocKey.label_pui_falloff_distance.Localize(), ""),
                monoBeh.Settings.DttPuiSettings.FalloffDistance);
        }
    }
}
