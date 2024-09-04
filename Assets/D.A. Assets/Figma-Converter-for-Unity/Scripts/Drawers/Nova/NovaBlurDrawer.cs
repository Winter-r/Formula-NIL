#if NOVA_UI_EXISTS
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using Nova;
using NovaSamples.Effects;
using System;

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class NovaBlurDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject)
        {
            return;
            fobject.Data.GameObject.TryGetComponent(out UIBlock2D uIBlock2D);

            foreach (Effect effect in fobject.Effects)
            {
                if (!effect.Type.ToString().Contains("BLUR"))
                    continue;

                fobject.Data.GameObject.TryAddComponent(out BlurEffect blurEffect);

                blurEffect.BlurMode = ConvertBlurType(effect.Type);

                float radius = effect.Radius;
                blurEffect.BlurRadius = radius;
                blurEffect.InputTexture = monoBeh.Settings.NovaSettings.InputTexture;

                if (monoBeh.TryGetComponent(out BackgroundBlurGroup blurGroup))
                {
                    blurGroup.BlurEffects.Add(blurEffect);
                }
            }
        }

        private BlurMode ConvertBlurType(EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.LAYER_BLUR:
                    {
                        return BlurMode.LayerBlur;
                    }
                case EffectType.BACKGROUND_BLUR:
                    {
                        return BlurMode.BackgroundBlur;
                    }
                default:
                    {
                        return BlurMode.LayerBlur;
                    }
            }
        }
    }
}
#endif