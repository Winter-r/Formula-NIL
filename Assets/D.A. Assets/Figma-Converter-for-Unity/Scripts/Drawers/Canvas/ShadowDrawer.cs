using DA_Assets.FCU.Model;
using System;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using DA_Assets.FCU.Extensions;
using UnityEngine;
using UnityEngine.UI;

#if TRUESHADOW_EXISTS
using LeTai.TrueShadow;
#endif

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class ShadowDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject)
        {
            switch (monoBeh.Settings.ComponentSettings.ShadowComponent)
            {
                case ShadowComponent.TrueShadow:
                    DrawTrueShadow(fobject);
                    break;
            }
        }

        private void DrawTrueShadow(FObject fobject)
        {
#if TRUESHADOW_EXISTS
            TrueShadow[] oldShadows = fobject.Data.GameObject.GetComponents<TrueShadow>();

            foreach (TrueShadow oldShadow in oldShadows)
                oldShadow.Destroy();

            foreach (Effect effect in fobject.Effects)
            {
                if (effect.Type.ToString().Contains("SHADOW"))
                {
                    monoBeh.Log($"DrawTrueShadow | {fobject.Data.Hierarchy}", FcuLogType.ComponentDrawer);

                    Image img = null;

                    if (fobject.ContainsTag(FcuTag.Image) == false)
                    {
                        fobject.Data.GameObject.TryAddGraphic(out img);
                    }

                    fobject.Data.GameObject.TryAddComponent(out TrueShadow trueShadow);

                    if (fobject.ContainsTag(FcuTag.Image) == false)
                    {
                        img.enabled = false;
                    }

                    ShadowData shadowData = GetShadowData(effect);

                    trueShadow.OffsetAngle = shadowData.Angle;
                    trueShadow.OffsetDistance = shadowData.Distance;
                    trueShadow.Spread = shadowData.Spread;
                    trueShadow.Color = shadowData.Color;
                    trueShadow.Size = shadowData.Radius;

                    trueShadow.BlendMode = BlendMode.Multiply;

                    if (effect.Type.ToString().Contains("DROP"))
                        trueShadow.Inset = false;
                    else
                        trueShadow.Inset = true;

                    trueShadow.enabled = true;
                }
            }
#endif
        }

        public ShadowData GetShadowData(Effect effect)
        {
            ShadowData shadowData = new ShadowData();
            shadowData.Offset = effect.Offset;  

            float x = effect.Offset.x;
            float y = effect.Offset.y;

            float angle = Mathf.Atan2(y, x) * (180.0f / Mathf.PI);
            float distance = Mathf.Sqrt(x * x + y * y);

            shadowData.Angle = angle;
            shadowData.Distance = distance;
            shadowData.Spread = effect.Spread.ToFloat();

            shadowData.Color = effect.Color;
            shadowData.Radius = effect.Radius;

            return shadowData;
        }
    }

    public struct ShadowData
    {
        public Vector2 Offset { get; set; }
        public float Angle { get; set; }
        public float Distance { get; set; }
        public float Spread { get; set; }
        public Color Color { get; set; }
        public float Radius { get; set; }
    }
}