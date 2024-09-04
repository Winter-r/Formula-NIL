using DA_Assets.FCU.Model;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU.Extensions
{
    public static class ImageExtensions
    {
        private static int roundDigits = 3;

        public static bool GetBoundingSize(this FObject fobject, out Vector2 size)
        {
            size = default(Vector2);

            float? x = fobject.AbsoluteBoundingBox.Width;
            float? y = fobject.AbsoluteBoundingBox.Height;

            if (x == null || y == null)
            {
                return false;
            }

            float xR = (float)Math.Round(x.Value, roundDigits);
            float yR = (float)Math.Round(y.Value, roundDigits);

            size = new Vector2(xR, yR);
            return true;
        }

        public static bool GetBoundingPosition(this FObject fobject, out Vector2 position)
        {
            position = default(Vector2);

            float? x = fobject.AbsoluteBoundingBox.X;
            float? y = fobject.AbsoluteBoundingBox.Y;

            if (x == null || y == null)
            {
                return false;
            }

            float xR = (float)Math.Round(x.Value, roundDigits);
            float yR = (float)Math.Round(y.Value, roundDigits);

            position = new Vector2(xR, yR);
            return true;
        }

        public static bool GetRenderSize(this FObject fobject, out Vector2 size)
        {
            size = default(Vector2);

            float? x = fobject.AbsoluteRenderBounds.Width;
            float? y = fobject.AbsoluteRenderBounds.Height;

            if (x == null || y == null)
            {
                return false;
            }

            float xR = (float)Math.Round(x.Value, roundDigits);
            float yR = (float)Math.Round(y.Value, roundDigits);

            size = new Vector2(xR, yR);
            return true;
        }

        public static bool GetRenderPosition(this FObject fobject, out Vector2 position)
        {
            position = default(Vector2);

            float? x = fobject.AbsoluteRenderBounds.X;
            float? y = fobject.AbsoluteRenderBounds.Y;

            if (x == null || y == null)
            {
                return false;
            }

            float xR = (float)Math.Round(x.Value, roundDigits);
            float yR = (float)Math.Round(y.Value, roundDigits);

            position = new Vector2(xR, yR);
            return true;
        }

        public static Vector4 GetCornerRadius(this FObject fobject, ImageComponent imageComponent, bool uitk = false)
        {
            if (fobject.IsCircle())
            {
                return new Vector4
                {
                    x = 9999f,
                    y = 9999f,
                    z = 9999f,
                    w = 9999f,
                };
            }

            if (fobject.CornerRadiuses.IsEmpty())
            {
                if (imageComponent == ImageComponent.RoundedImage)
                {
                    return new Vector4
                    {
                        x = fobject.CornerRadius.ToFloat(),
                        y = fobject.CornerRadius.ToFloat(),
                        z = fobject.CornerRadius.ToFloat(),
                        w = fobject.CornerRadius.ToFloat()
                    };
                }
                else
                {
                    return new Vector4
                    {
                        x = fobject.CornerRadius.ToFloat(),
                        y = fobject.CornerRadius.ToFloat(),
                        z = fobject.CornerRadius.ToFloat(),
                        w = fobject.CornerRadius.ToFloat()
                    };
                }

            }
            else
            {
                if (uitk)
                {
                    return new Vector4
                    {
                        x = fobject.CornerRadiuses[0],
                        y = fobject.CornerRadiuses[3],
                        z = fobject.CornerRadiuses[2],
                        w = fobject.CornerRadiuses[1]
                    };
                }
                else if (imageComponent == ImageComponent.ProceduralImage)
                {
                    return new Vector4
                    {
                        x = fobject.CornerRadiuses[0],
                        y = fobject.CornerRadiuses[1],
                        z = fobject.CornerRadiuses[2],
                        w = fobject.CornerRadiuses[3]
                    };
                }
                else if (imageComponent == ImageComponent.RoundedImage)
                {
                    return new Vector4
                    {
                        x = fobject.CornerRadiuses[0],
                        y = fobject.CornerRadiuses[1],
                        z = fobject.CornerRadiuses[2],
                        w = fobject.CornerRadiuses[3],
                    };
                }
                else
                {
                    return new Vector4
                    {
                        x = fobject.CornerRadiuses[3],
                        y = fobject.CornerRadiuses[2],
                        z = fobject.CornerRadiuses[1],
                        w = fobject.CornerRadiuses[0]
                    };
                }
            }
        }

        public static bool IsZeroSize(this FObject fobject)
        {
            if (fobject.AbsoluteBoundingBox.Width == 0 || fobject.AbsoluteBoundingBox.Height == 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsVisible(this FObject fobject) => fobject.Visible.ToBoolNullTrue();

        public static bool IsVisible(this Paint paint) => paint.Visible.ToBoolNullTrue();

        public static bool IsVisible(this Effect effect) => effect.Visible.ToBoolNullTrue();

        public static bool IsSingleColor(this FObject fobject, out Color color)
        {
            Dictionary<Color, float?> values = new Dictionary<Color, float?>();
            List<bool> flags = new List<bool>();

            IsSingleColorRecursive(fobject, flags, values);

            if (flags.Count > 0)
            {
                color = default;
                return false;
            }

            if (values.Count == 1)
            {
                color = values.First().Key;
                return true;
            }
            else
            {
                color = default;
                return false;
            }
        }

        public static bool HasImageOrGifRef(this FObject fobject)
        {
            if (fobject.Fills.IsEmpty())
                return false;

            foreach (Paint item in fobject.Fills)
            {
                if (item.Visible.ToBoolNullTrue() == false)
                    continue;

                if (item.ImageRef.IsEmpty() == false || item.GifRef.IsEmpty() == false)
                    return true;
            }

            return false;
        }

        private static void IsSingleColorRecursive(FObject fobject, List<bool> flags, Dictionary<Color, float?> values)
        {
            if (fobject.Fills.IsEmpty() == false)
            {
                foreach (var item in fobject.Fills)
                {
                    if (!item.IsVisible())
                        continue;

                    if (item.ImageRef.IsEmpty() == false || item.GifRef.IsEmpty() == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    if (item.Type.ToString().Contains("SOLID") == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    values.TryAddValue<Color, float?>(item.Color, item.Opacity);
                }
            }

            if (fobject.Strokes.IsEmpty() == false)
            {
                foreach (var item in fobject.Strokes)
                {
                    if (!item.IsVisible())
                        continue;

                    if (item.ImageRef.IsEmpty() == false || item.GifRef.IsEmpty() == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    if (item.Type.ToString().Contains("SOLID") == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    values.TryAddValue<Color, float?>(item.Color, item.Opacity);
                }
            }

            if (fobject.Effects.IsEmpty() == false)
            {
                foreach (var item in fobject.Effects)
                {
                    if (!item.IsVisible())
                        continue;

                    if (item.Type.ToString().Contains("SOLID") == false)
                    {
                        flags.Add(true);
                        return;
                    }

                    values.TryAddValue<Color, float?>(item.Color, item.Opacity);
                }
            }

            if (fobject.Children.IsEmpty())
                return;

            foreach (var item in fobject.Children)
            {
                if (item.ContainsTag(FcuTag.Text))
                    continue;

                IsSingleColorRecursive(item, flags, values);
            }
        }

        public static bool ContainsRoundedCorners(this FObject fobject)
        {
            return fobject.CornerRadius > 0 || (fobject.CornerRadiuses?.Any(radius => radius > 0)).ToBoolNullFalse();
        }

        public static bool IsArcDataFilled(this FObject fobject)
        {
            if (fobject.ArcData.Equals(default(ArcData)))
            {
                return false;
            }

            return fobject.ArcData.EndingAngle < 6.28f;
        }

        public static bool IsGradient(this Paint paint)
        {
            return paint.Type.ToString().Contains("GRADIENT");
        }

        public static bool TryGetFirstGradient(this FObject fobject, out Paint gradient)
        {
            if (fobject.Fills.IsEmpty())
            {
                gradient = default;
                return false;
            }

            foreach (Paint _fill in fobject.Fills)
            {
                if (_fill.Visible == false)
                    continue;
            }

            gradient = default;
            return false;
        }

        public static FGraphic GetGraphic(this FObject fobject)
        {
            FGraphic graphic = new FGraphic();

            graphic.HaveDownloadableColors = fobject.Fills.IsColorsDownloadable() || fobject.Strokes.IsColorsDownloadable();

            if (!graphic.HaveDownloadableColors || (graphic.HaveDownloadableColors && fobject.HaveUndownloadableTags(out var reason)))
            {
                graphic.SolidFill = fobject.Fills.GetFirstSolidColor();
                graphic.GradientFill = fobject.Fills.GetFirstGradientColor();

                if (!graphic.SolidFill.IsDefault() || !graphic.GradientFill.IsDefault())
                {
                    graphic.HasFill = true;
                }

                graphic.SolidStroke = fobject.Strokes.GetFirstSolidColor();
                graphic.GradientStroke = fobject.Strokes.GetFirstGradientColor();

                if (!graphic.SolidStroke.IsDefault() || !graphic.GradientStroke.IsDefault())
                {
                    graphic.HasStroke = true;
                }
            }

            return graphic;
        }

        public static Color GradientToSolid(this Paint gradientColor) 
        {
            List<GradientColorKey> gradientColorKeys = gradientColor.ToGradientColorKeys();
            List<GradientAlphaKey> gradientAlphaKeys = gradientColor.ToGradientAlphaKeys();

            Color result = default;

            if (!gradientColorKeys.IsEmpty() && !gradientAlphaKeys.IsEmpty())
            {
                result = ImageExtensions.SetFigmaAlpha(gradientColorKeys[0].color, gradientAlphaKeys[0].alpha);
            }

            return result;  
        }

        public static Color SetFigmaAlpha(Color color, float? opacity)
        {
            return new Color(color.r, color.g, color.b, opacity == null ? 1 : opacity.ToFloat());
        }

        public static bool IsColorsDownloadable(this List<Paint> paints)
        {
            if (paints == null)
            {
                return false;
            }

            foreach (Paint paint in paints)
            {
                if (IsColorDownloadable(paint))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsColorDownloadable(this Paint paint)
        {
            if (!paint.IsVisible())
                return false;

            switch (paint.Type)
            {
                case PaintType.GRADIENT_RADIAL:
                case PaintType.GRADIENT_ANGULAR:
                case PaintType.GRADIENT_DIAMOND:
                case PaintType.IMAGE:
                case PaintType.EMOJI:
                case PaintType.VIDEO:
                    {
                        return true;
                    }
                default:
                    break;
            }

            return false;
        }

        public static Paint GetFirstSolidColor(this List<Paint> paints)
        {
            if (paints == null)
                return default;

            Paint fill = default;

            foreach (Paint paint in paints)
            {
                if (!paint.IsVisible())
                    continue;

                if (paint.Type == PaintType.SOLID)
                {
                    fill = paint;
                    fill.Color = SetFigmaAlpha(fill.Color, paint.Opacity);
                    break;
                }
            }

            return fill;
        }
        public static Paint GetFirstGradientColor(this List<Paint> paints)
        {
            if (paints == null)
                return default;

            Paint fill = default;

            foreach (Paint paint in paints)
            {
                if (!paint.IsVisible())
                    continue;

                if (paint.Type == PaintType.GRADIENT_LINEAR)
                {
                    fill = paint;
                    fill.Color = SetFigmaAlpha(fill.Color, paint.Opacity);
                    break;
                }
            }

            return fill;
        }
    }
}