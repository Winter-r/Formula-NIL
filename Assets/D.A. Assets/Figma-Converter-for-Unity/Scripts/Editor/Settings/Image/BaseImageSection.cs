using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using UnityEngine;

namespace DA_Assets.FCU
{
    internal class BaseImageSection : ScriptableObjectBinder<FcuSettingsWindow, FigmaConverterUnity>
    {
        protected void DrawBase(BaseImageSettings settings)
        {
            settings.Type = gui.EnumField(new GUIContent(FcuLocKey.label_image_type.Localize(), ""),
                settings.Type);

            settings.RaycastTarget = gui.Toggle(new GUIContent(FcuLocKey.label_raycast_target.Localize(), ""),
                settings.RaycastTarget);

            settings.PreserveAspect = gui.Toggle(new GUIContent(FcuLocKey.label_preserve_aspect.Localize(), ""),
                settings.PreserveAspect);

            settings.RaycastPadding = gui.Vector4Field(new GUIContent(FcuLocKey.label_raycast_padding.Localize(), ""),
                settings.RaycastPadding);

            settings.Maskable = gui.Toggle(new GUIContent(FcuLocKey.label_maskable.Localize(), ""),
                settings.Maskable);
        }
    }
}