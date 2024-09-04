using UnityEngine;

namespace DA_Assets.FCU
{
    internal class JoshPuiSection : BaseImageSection
    {
        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_pui_settings.Localize());
            gui.Space15();

            DrawBase(monoBeh.Settings.JoshPuiSettings);

            monoBeh.Settings.JoshPuiSettings.FalloffDistance = gui.FloatField(new GUIContent(FcuLocKey.label_pui_falloff_distance.Localize(), ""),
                monoBeh.Settings.JoshPuiSettings.FalloffDistance);
        }
    }
}
