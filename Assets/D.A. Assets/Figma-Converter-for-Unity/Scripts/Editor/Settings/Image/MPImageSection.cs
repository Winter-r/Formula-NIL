using UnityEngine;

namespace DA_Assets.FCU
{
    internal class MPImageSection : BaseImageSection
    {
        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_mpuikit_settings.Localize());
            gui.Space15();

            DrawBase(monoBeh.Settings.MPUIKitSettings);

            monoBeh.Settings.MPUIKitSettings.FalloffDistance = gui.FloatField(new GUIContent(FcuLocKey.label_pui_falloff_distance.Localize(), ""),
                monoBeh.Settings.MPUIKitSettings.FalloffDistance);
        }
    }
}
