#if NOVA_UI_EXISTS
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using Nova;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class NovaImageDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, Sprite sprite, GameObject target)
        {
            target.TryGetComponent(out UIBlock2D uIBlock2D);

            uIBlock2D.SetImage(sprite);

            if (fobject.IsDrawableType())
            {
                SetCorners(fobject, uIBlock2D);
            }

            SetColor(fobject, uIBlock2D);
        }

        private BorderDirection ConvertStrokeType(StrokeAlign strokeAlign)
        {
            switch (strokeAlign)
            {
                case StrokeAlign.INSIDE:
                    return BorderDirection.In;
                case StrokeAlign.OUTSIDE:
                    return BorderDirection.Out;
                case StrokeAlign.CENTER:
                    return BorderDirection.Center;
                default:
                    return BorderDirection.Out;
            }
        }

        public void SetColor(FObject fobject, UIBlock2D sr)
        {
            FGraphic graphic = fobject.GetGraphic();

            monoBeh.Log($"SetUnityImageColor | {fobject.Data.NameHierarchy} | {fobject.Data.FcuImageType} | graphic.HasFills: {graphic.HasFill} | graphic.HasStrokes: {graphic.HasStroke}", FcuLogType.ComponentDrawer);

            if (fobject.IsDrawableType() || fobject.IsGenerativeType())
            {
                if (graphic.HasFill)
                {
                    Color fillColor = default;

                    if (!graphic.SolidFill.IsDefault())
                    {
                        fillColor = graphic.SolidFill.Color;
                    }
                    else if (!graphic.GradientFill.IsDefault())
                    {
                        fillColor = graphic.GradientFill.GradientToSolid();
                    }

                    sr.Color = fillColor;
                }

                if (sr.Sprite != null && sr.Color == default)
                {
                    sr.Color = Color.white;
                }

                if (graphic.HasStroke && !fobject.IsGenerativeType())
                {
                    Color strokeColor = default;

                    if (!graphic.SolidStroke.IsDefault())
                    {
                        strokeColor = graphic.SolidStroke.Color;
                    }
                    else if (!graphic.GradientStroke.IsDefault())
                    {
                        strokeColor = graphic.GradientStroke.GradientToSolid();
                    }

                    sr.Border = new Border
                    {
                        Enabled = true,
                        Color = strokeColor,
                        Width = fobject.StrokeWeight,
                        Direction = ConvertStrokeType(fobject.StrokeAlign)
                    };
                }
                else
                {
                    sr.Border = new Border
                    {
                        Enabled = false
                    };
                }        
            }           
            else if (fobject.IsDownloadableType())
            {
                if (fobject.Data.SingleColor.IsDefault() == false)
                {
                    sr.Color = fobject.Data.SingleColor;
                }
                else
                {
                    sr.Color = Color.white;
                }
            }
        }

        private void SetCorners(FObject fobject, UIBlock2D img)
        {
            var cr = fobject.GetCornerRadius(ImageComponent.MPImage);

            img.CornerRadius = new Length
            {
                Type = LengthType.Value,
                Value = cr.x
            };
        }
    }
}
#endif