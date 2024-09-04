#if NOVA_UI_EXISTS
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using Nova;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class NovaMaskDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject)
        {
            bool get = monoBeh.CurrentProject.TryGetByIndex(fobject.Data.ParentIndex, out FObject target);

            if (get == false && fobject.ContainsTag(FcuTag.Frame) == false)
            {
                return;
            }

            GameObject targetGo;

            if (fobject.IsObjectMask())
            {
                targetGo = target.Data.GameObject;
            }
            else
            {
                targetGo = fobject.Data.GameObject;
            }

            if (fobject.IsFrameMask() || fobject.IsClipMask())
            {
                targetGo.TryAddComponent(out ClipMask unityMask);
            }
            else if (fobject.IsObjectMask())
            {
               // monoBeh.CanvasDrawer.ImageDrawer.Draw(fobject, targetGo);
                targetGo.TryAddComponent(out ClipMask unityMask);

                Sprite sprite = monoBeh.SpriteWorker.GetSprite(fobject);
                unityMask.Mask = sprite.texture;

                fobject.Data.GameObject.SetActive(false);
            }
        }
    }
}
#endif