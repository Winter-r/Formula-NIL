namespace DA_Assets.FCU
{
    internal class Shapes2DSection : BaseImageSection
    {
        public void Draw()
        {
            gui.SectionHeader(FcuLocKey.label_shapes2d_settings.Localize());
            gui.Space15();

            DrawBase(monoBeh.Settings.Shapes2DSettings);
        }
    }
}
