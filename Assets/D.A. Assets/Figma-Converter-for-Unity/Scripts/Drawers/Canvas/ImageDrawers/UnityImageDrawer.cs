using DA_Assets.DAG;
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;

#if PROCEDURAL_UI_ASSET_STORE_RELEASE
using System.Reflection;
using DTT.UI.ProceduralUI;
#endif

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class UnityImageDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, Sprite sprite, GameObject target)
        {
            MaskableGraphic graphic;

            if (monoBeh.UsingRawImage())
            {
                target.TryAddGraphic(out RawImage img);
                graphic = img;

                if (sprite != null)
                {
                    img.texture = sprite.texture;
                }
            }
            else
            {
                target.TryAddGraphic(out Image img);
                graphic = img;

                img.sprite = sprite;
                img.type = monoBeh.Settings.UnityImageSettings.Type;
                img.preserveAspect = monoBeh.Settings.UnityImageSettings.PreserveAspect;
            }

            graphic.raycastTarget = monoBeh.Settings.UnityImageSettings.RaycastTarget;
            graphic.maskable = monoBeh.Settings.UnityImageSettings.Maskable;
#if UNITY_2020_1_OR_NEWER
            graphic.raycastPadding = monoBeh.Settings.UnityImageSettings.RaycastPadding;
#endif

            SetColor(fobject, graphic);
            TryAddCornerRounder(fobject, target);
        }

        public void SetColor(FObject fobject, MaskableGraphic gr)
        {
            FGraphic graphic = fobject.GetGraphic();

            monoBeh.Log($"SetUnityImageColor | {fobject.Data.NameHierarchy} | {fobject.Data.FcuImageType} | graphic.HasFills: {graphic.HasFill} | graphic.HasStrokes: {graphic.HasStroke}", FcuLogType.ComponentDrawer);

            if (fobject.IsDrawableType())
            {
                if (graphic.HasFill && graphic.HasStroke)
                {
                    AddUnityOutline(fobject, gr.gameObject, graphic.SolidStroke, graphic.GradientStroke);
                }

                if (graphic.HasFill || fobject.Data.IsOverlappedByStroke)
                {
                    if (graphic.SolidFill.IsDefault() == false)
                    {
                        gr.color = graphic.SolidFill.Color;
                    }
                    else
                    {
                        gr.color = Color.white;
                    }

                    if (graphic.GradientFill.IsDefault() == false)
                    {
                        AddGradient(graphic.GradientFill, gr.gameObject);
                    }
                }
                else if (graphic.HasStroke)
                {
                    if (graphic.SolidStroke.IsDefault() == false)
                    {
                        gr.color = graphic.SolidStroke.Color;
                    }
                    else
                    {
                        gr.color = Color.white;
                    }

                    if (graphic.GradientStroke.IsDefault() == false)
                    {
                        AddGradient(graphic.GradientStroke, gr.gameObject);
                    }
                }
                else
                {
                    fobject.Data.GameObject.TryDestroyComponent<Outline>();
                }
            }
            else if (fobject.IsGenerativeType())
            {
                if (graphic.HasFill && graphic.HasStroke)//no need colorize
                {
                    if (fobject.StrokeAlign == StrokeAlign.OUTSIDE)
                    {
                        AddUnityOutline(fobject, gr.gameObject, graphic.SolidStroke, graphic.GradientStroke);
                    }
                }
                else if (graphic.HasFill)
                {
                    if (graphic.SolidFill.IsDefault() == false)
                    {
                        gr.color = graphic.SolidFill.Color;
                    }
                    else
                    {
                        gr.color = Color.white;
                    }

                    if (graphic.GradientFill.IsDefault() == false)
                    {
                        AddGradient(graphic.GradientFill, gr.gameObject);
                    }
                }
                else if (graphic.HasStroke)
                {
                    if (graphic.SolidStroke.IsDefault() == false)
                    {
                        gr.color = graphic.SolidStroke.Color;
                    }
                    else
                    {
                        gr.color = Color.white;
                    }

                    if (graphic.GradientStroke.IsDefault() == false)
                    {
                        AddGradient(graphic.GradientStroke, gr.gameObject);
                    }
                }
            }
            else if (fobject.IsDownloadableType())
            {
                if (fobject.Data.SingleColor.IsDefault() == false)
                {
                    gr.color = fobject.Data.SingleColor;
                }
                else
                {
                    gr.color = Color.white;
                }
            }
        }

        public void AddUnityOutline(FObject fobject, GameObject target, Paint solidStroke, Paint gradientStroke)
        {
            if (fobject.StrokeAlign != StrokeAlign.OUTSIDE)
            {
                return;
            }

            target.TryAddComponent(out UnityEngine.UI.Outline outline);
            outline.effectDistance = new Vector2(fobject.StrokeWeight, -fobject.StrokeWeight);

            if (solidStroke.IsDefault() == false)
            {
                outline.effectColor = solidStroke.Color;
            }
            else if (gradientStroke.IsDefault() == false)
            {
                List<GradientColorKey> gradientColorKeys = gradientStroke.ToGradientColorKeys();

                if (!gradientColorKeys.IsEmpty())
                {
                    outline.effectColor = gradientColorKeys.First().color;
                }
            }
            else
            {
                outline.effectColor = default;
            }
        }

        public void AddGradient(Paint gradientColor, GameObject go)
        {
            if (monoBeh.UsingMPUIKit())
                return;

            List<GradientColorKey> gradientColorKeys = gradientColor.ToGradientColorKeys();
            List<GradientAlphaKey> gradientAlphaKeys = gradientColor.ToGradientAlphaKeys();

            float angle = gradientColor.GradientHandlePositions.ToAngle();

            if (monoBeh.UsingDttPui())
            {
#if PROCEDURAL_UI_ASSET_STORE_RELEASE
                go.TryAddComponent(out GradientEffect gradient);

                Gradient newGradient = new Gradient();
                newGradient.colorKeys = gradientColorKeys.ToArray();
                newGradient.alphaKeys = gradientAlphaKeys.ToArray();

                Type objectType = gradient.GetType();
                FieldInfo fieldInfo = objectType.GetField("_gradient", BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo != null)
                {
                    fieldInfo.SetValue(gradient, newGradient);
                }

                gradient.Rotation = angle;

                FieldInfo fieldInfo1 = objectType.GetField("_type", BindingFlags.NonPublic | BindingFlags.Instance);

                if (fieldInfo1 != null)
                {
                    fieldInfo1.SetValue(gradient, GradientEffect.GradientType.LINEAR);
                }

#endif
            }
            else
            {
                go.TryAddComponent(out DAGradient gradient);

                gradient.Angle = angle;
                gradient.BlendMode = DAColorBlendMode.Multiply;

                gradient.Gradient.colorKeys = gradientColorKeys.ToArray();
                gradient.Gradient.alphaKeys = gradientAlphaKeys.ToArray();
            }
        }

        private bool TryAddCornerRounder(FObject fobject, GameObject target)
        {
            if (fobject.IsSprite())
            {
                return false;
            }
            Vector4 cr = fobject.GetCornerRadius(ImageComponent.UnityImage);

            monoBeh.Log($"TryAddCornerRounder | {fobject.Data.NameHierarchy} | {cr}");

            if (cr.IsDefault() == false)
            {
                target.TryAddComponent(out CornerRounder cornerRounder);
                cornerRounder.SetRadii(cr);
            }

            return false;
        }
    }
}
