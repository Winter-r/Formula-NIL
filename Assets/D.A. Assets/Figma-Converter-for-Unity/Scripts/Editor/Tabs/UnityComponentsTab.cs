using DA_Assets.Shared;
using UnityEngine;

#pragma warning disable IDE0003
#pragma warning disable CS0649

namespace DA_Assets.FCU
{
    internal class UnityComponentsTab : ScriptableObjectBinder<FcuSettingsWindow, FigmaConverterUnity>
    {
        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_import_components.Localize(), FcuLocKey.tooltip_import_components.Localize());
            gui.Space15();

            monoBeh.Settings.ComponentSettings.ImageComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_image_component.Localize(), FcuLocKey.tooltip_image_component.Localize()),
                monoBeh.Settings.ComponentSettings.ImageComponent, false);

            monoBeh.Settings.ComponentSettings.TextComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_text_component.Localize(), FcuLocKey.tooltip_text_component.Localize()),
                monoBeh.Settings.ComponentSettings.TextComponent, false);

            monoBeh.Settings.ComponentSettings.ShadowComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_shadow_type.Localize(), FcuLocKey.tooltip_shadow_type.Localize()),
                monoBeh.Settings.ComponentSettings.ShadowComponent, false);

            monoBeh.Settings.ComponentSettings.ButtonComponent = gui.EnumField(
                new GUIContent(FcuLocKey.label_button_type.Localize(), FcuLocKey.tooltip_button_type.Localize()),
                monoBeh.Settings.ComponentSettings.ButtonComponent, false, null);

            monoBeh.Settings.ComponentSettings.UseI2Localization = gui.Toggle(
                new GUIContent(FcuLocKey.label_use_i2localization.Localize(), FcuLocKey.tooltip_use_i2localization.Localize()),
                monoBeh.Settings.ComponentSettings.UseI2Localization);

            gui.Space15();

            switch (monoBeh.Settings.ComponentSettings.ImageComponent)
            {
                case ImageComponent.UnityImage:
                case ImageComponent.RawImage:
                    this.UnityImageSettingsSection.Draw();
                    break;
                case ImageComponent.SubcShape:
                    this.Shapes2DSettingsSection.Draw();
                    break;
                case ImageComponent.ProceduralImage:
                    this.JoshPuiSettingsSection.Draw();
                    break;
                case ImageComponent.RoundedImage:
                    this.DttPuiSettingsSection.Draw();
                    break;
                case ImageComponent.MPImage:
                    this.MPImageSettingsSection.Draw();
                    break;
                case ImageComponent.SpriteRenderer:
                    this.SpriteRendererSettingsSection.Draw();
                    break;
            }

            gui.Space15();

            switch (monoBeh.Settings.ComponentSettings.TextComponent)
            {
                case TextComponent.UnityText:
                    this.DefaultTextSettingsSection.Draw();
                    break;
                case TextComponent.TextMeshPro:
                case TextComponent.RTLTextMeshPro:
                    this.TextMeshSettingsSection.Draw();
                    break;
            }

            gui.Space15();
            this.ButtonSettingsSection.Draw();
#if DABUTTON_EXISTS
            gui.Space15();
            this.DabSettingsSection.Draw();
#endif

            gui.Space30();
        }

        private UnityImageSection unityImageSettingsSection;
        internal UnityImageSection UnityImageSettingsSection => monoBeh.Bind(ref unityImageSettingsSection, scriptableObject);

        private Shapes2DSection shapesSettingsSection;
        internal Shapes2DSection Shapes2DSettingsSection => monoBeh.Bind(ref shapesSettingsSection, scriptableObject);

        private JoshPuiSection joshPuiSettingsSection;
        internal JoshPuiSection JoshPuiSettingsSection => monoBeh.Bind(ref joshPuiSettingsSection, scriptableObject);

        private DttPuiSection dttPuiSettingsSection;
        internal DttPuiSection DttPuiSettingsSection => monoBeh.Bind(ref dttPuiSettingsSection, scriptableObject);

        private MPImageSection mpImageSettingsSection;
        internal MPImageSection MPImageSettingsSection => monoBeh.Bind(ref mpImageSettingsSection, scriptableObject);

        private SrSection srSettingsSection;
        internal SrSection SpriteRendererSettingsSection => monoBeh.Bind(ref srSettingsSection, scriptableObject);

        private TextMeshSection textMeshSettingsSection;
        internal TextMeshSection TextMeshSettingsSection => monoBeh.Bind(ref textMeshSettingsSection, scriptableObject);

        private UnityTextSection defaultTextSettingsSection;
        internal UnityTextSection DefaultTextSettingsSection => monoBeh.Bind(ref defaultTextSettingsSection, scriptableObject);

        private ButtonSection buttonSettingsSection;
        internal ButtonSection ButtonSettingsSection => monoBeh.Bind(ref buttonSettingsSection, scriptableObject);

        private DabSection dabSettingsSection;
        internal DabSection DabSettingsSection => monoBeh.Bind(ref dabSettingsSection, scriptableObject);
    }
}