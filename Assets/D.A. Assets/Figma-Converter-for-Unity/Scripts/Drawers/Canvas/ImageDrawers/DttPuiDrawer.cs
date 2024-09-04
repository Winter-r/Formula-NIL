using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;



#if PROCEDURAL_UI_ASSET_STORE_RELEASE
using DTT.UI.ProceduralUI;
#endif

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class DttPuiDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, Sprite sprite, GameObject target)
        {
#if PROCEDURAL_UI_ASSET_STORE_RELEASE
            target.TryAddGraphic(out RoundedImage img);

            img.sprite = sprite;
            img.type = monoBeh.Settings.DttPuiSettings.Type;
            img.raycastTarget = monoBeh.Settings.DttPuiSettings.RaycastTarget;
            img.preserveAspect = monoBeh.Settings.DttPuiSettings.PreserveAspect;
            img.DistanceFalloff = monoBeh.Settings.DttPuiSettings.FalloffDistance;
#if UNITY_2020_1_OR_NEWER
            img.raycastPadding = monoBeh.Settings.DttPuiSettings.RaycastPadding;
#endif
            /* if (fobject.Type == NodeType.ELLIPSE)
             {
                 target.TryAddComponent(out RoundModifier roundModifier);
             }
             else
             {
                 if (fobject.CornerRadiuses != null)
                 {
                     target.TryAddComponent(out FreeModifier freeModifier);
                     freeModifier.Radius = fobject.GetCornerRadius(ImageComponent.ProceduralImage);
                 }
                 else
                 {
                     target.TryAddComponent(out UniformModifier uniformModifier);
                     uniformModifier.Radius = fobject.CornerRadius.ToFloat();
                 }
             }*/
            SetCorners(fobject, img);
            SetColor(fobject, img);
#endif
        }

        public void SetColor(FObject fobject, Image img)
        {
            FGraphic graphic = fobject.GetGraphic();

            monoBeh.Log($"SetUnityImageColor | {fobject.Data.NameHierarchy} | {fobject.Data.FcuImageType} | hasFills: {graphic.HasFill} | hasStroke: {graphic.HasStroke}", FcuLogType.ComponentDrawer);

            if (fobject.IsDrawableType())
            {
                if (graphic.HasFill)
                {
                    if (graphic.SolidFill.IsDefault() == false)
                    {
                        img.color = graphic.SolidFill.Color;
                    }
                    else
                    {
                        img.color = Color.white;
                    }

                    if (graphic.GradientFill.IsDefault() == false)
                    {
                        monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.AddGradient(graphic.GradientFill, img.gameObject);
                    }
                }
                else
                {
                    Color c = Color.white;
                    c.a = 0;
                    img.color = c;
                }

                if (graphic.HasStroke)
                {
                    if (fobject.StrokeAlign == StrokeAlign.OUTSIDE)
                    {
                        monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.AddUnityOutline(fobject, img.gameObject, graphic.SolidStroke, graphic.GradientStroke);
                    }
                }
            }
            else
            {
                monoBeh.CanvasDrawer.ImageDrawer.UnityImageDrawer.SetColor(fobject, img);
            }
        }

#if PROCEDURAL_UI_ASSET_STORE_RELEASE
        private void SetCorners(FObject fobject, RoundedImage img)
        {
            if (fobject.Type == NodeType.ELLIPSE)
            {
                /*img.DrawShape = DrawShape.Circle;
                img.Circle = new Circle
                {
                    FitToRect = true
                };*/
            }
            else
            {
                img.RoundingUnit = RoundingUnit.WORLD;
                Vector4 radius = fobject.GetCornerRadius(ImageComponent.MPImage);
                if (!radius.IsDefault())
                {
                    Type objectType = img.GetType();
                    FieldInfo fieldInfo = objectType.GetField("_cornerMode", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (fieldInfo != null)
                    {
#if UNITY_EDITOR
                        fieldInfo.SetValue(img, (int)RoundingCornerMode.INDIVIDUAL);
#endif
                    }

                    SetByIndex(0, img, objectType, "_roundingAmount", radius[0]);
                    SetByIndex(1, img, objectType, "_roundingAmount", radius[1]);
                    SetByIndex(2, img, objectType, "_roundingAmount", radius[2]);
                    SetByIndex(3, img, objectType, "_roundingAmount", radius[3]);
                }
               
            }
        }
#endif


        public void SetByIndex(int index, object classInstance, Type type, string arrayFieldName, float value)
        {
            FieldInfo fieldInfo = type.GetField(arrayFieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null)
            {
                DALogger.LogError($"Field '{arrayFieldName}' not found in type '{type}'.");
                return;
            }

            float[] array = fieldInfo.GetValue(classInstance) as float[];
            if (array == null)
            {
                DALogger.LogError($"Field '{arrayFieldName}' is not an array or is null.");
                return;
            }

            if (index < 0 || index >= array.Length)
            {
                DALogger.LogError($"Index '{index}' is out of bounds for array '{arrayFieldName}'.");
                return;
            }

            array[index] = value;

            fieldInfo.SetValue(classInstance, array);

            monoBeh.Log($"Value {value} was set at index {index} of array '{arrayFieldName}'.");
        }
    }
}
