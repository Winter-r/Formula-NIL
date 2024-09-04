namespace DA_Assets.FCU
{
    public enum FcuNameType
    {
        Object,
        Field,
        Method,
        File,
        UitkGuid,
        Class,
        UssClass
    }
    public enum PreserveRatioMode
    {
        None,
        WidthControlsHeight,
        HeightControlsWidth,
    }

    public enum FcuLogType
    {
        Default,
        SetTag,
        IsDownloadable,
        Transform,
        Error,
        GameObjectDrawer,
        ComponentDrawer,
        HashGenerator
    }

    public enum FcuImageType
    {
        None,
        Downloadable,
        Drawable,
        Generative,
        Mask
    }

    public enum PositioningMode
    {
        Absolute = 0,
        GameView = 1
    }

    public enum UIFramework
    {
        UGUI = 0,
        UITK = 1,
        NOVA = 2
    }

    public enum ImageFormat
    {
        PNG = 0,
        JPG = 1
    }

    public enum ImageComponent
    {
        UnityImage = 0,
        SubcShape = 1,
        MPImage = 2,
        ProceduralImage = 3,
        RawImage = 4,
        SpriteRenderer = 5,
        RoundedImage = 6,
        ShapesAsset = 7
    }

    public enum TextComponent
    {
        UnityText = 0,
        TextMeshPro = 1,
        RTLTextMeshPro = 2
    }

    public enum ShadowComponent
    {
        Figma = 0,
        TrueShadow = 1
    }

    public enum ButtonComponent
    {
        UnityButton = 0,
        DAButton = 1,
        FcuButton = 2
    }
}