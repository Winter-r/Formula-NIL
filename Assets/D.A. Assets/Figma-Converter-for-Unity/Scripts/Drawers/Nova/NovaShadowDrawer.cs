#if NOVA_UI_EXISTS
using DA_Assets.FCU.Drawers.CanvasDrawers;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using Nova;
using System;

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class NovaShadowDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject)
        {
            if (fobject.Data.FcuImageType == FcuImageType.Downloadable)
            {
                return;
            }

            if (!fobject.Data.GameObject.TryGetComponent(out UIBlock2D uIBlock2D))
            {
                return;
            }

            foreach (Effect effect in fobject.Effects)
            {
                if (effect.Type.ToString().Contains("SHADOW"))
                {
                    ShadowData shadowData = monoBeh.CanvasDrawer.ShadowDrawer.GetShadowData(effect);

                    Shadow s = new Shadow();
                    s.Enabled = true;
                    s.Direction = ShadowDirection.Out;
                    s.Color = shadowData.Color;
                    s.Blur = shadowData.Radius;
                    s.Width = shadowData.Spread;
                    s.Offset = shadowData.Offset;

                    uIBlock2D.Shadow = s;
                    break;
                }
            }
        }
    }
}
#endif