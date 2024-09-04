using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class ImageDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject, GameObject customGameObject = null)
        {
            GameObject target = customGameObject == null ? fobject.Data.GameObject : customGameObject;

            if (fobject.Data.GameObject.IsPartOfAnyPrefab() == false)
            {
                if (target.TryGetComponent(out Graphic oldGraphic))
                {
                    Type curType = monoBeh.GetCurrentImageType();

                    if (oldGraphic.GetType().Equals(curType) == false)
                    {
                        oldGraphic.RemoveComponentsDependingOn();
                        oldGraphic.Destroy();
                    }
                }
            }

            Sprite sprite = monoBeh.SpriteWorker.GetSprite(fobject);

            if (monoBeh.IsNova())
            {
#if NOVA_UI_EXISTS
                this.NovaImageDrawer.Draw(fobject, sprite, target);
#endif
            }
            else if (monoBeh.UsingSpriteRenderer())
            {
                this.SpriteRendererDrawer.Draw(fobject, sprite, target);
            }
            else if (fobject.IsObjectMask() || monoBeh.UsingUnityImage() || monoBeh.UsingRawImage())
            {
                this.UnityImageDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingShapes2D())
            {
                this.Shapes2DDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingShapes())
            {
                this.ShapesDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingJoshPui())
            {
                this.JoshPuiDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingDttPui())
            {
                this.DttPuiDrawer.Draw(fobject, sprite, target);
            }
            else if (monoBeh.UsingMPUIKit())
            {
                this.MPUIKitDrawer.Draw(fobject, sprite, target);
            }
        }

        [SerializeField] UnityImageDrawer unityImageDrawer;
        [SerializeProperty(nameof(unityImageDrawer))]
        public UnityImageDrawer UnityImageDrawer => unityImageDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] Shapes2DDrawer shapes2DDrawer;
        [SerializeProperty(nameof(shapes2DDrawer))]
        public Shapes2DDrawer Shapes2DDrawer => shapes2DDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] ShapesDrawer shapesDrawer;
        [SerializeProperty(nameof(shapesDrawer))]
        public ShapesDrawer ShapesDrawer => shapesDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] JoshPuiDrawer joshPuiDrawer;
        [SerializeProperty(nameof(joshPuiDrawer))]
        public JoshPuiDrawer JoshPuiDrawer => joshPuiDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] DttPuiDrawer dttPuiDrawer;
        [SerializeProperty(nameof(dttPuiDrawer))]
        public DttPuiDrawer DttPuiDrawer => dttPuiDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] MPUIKitDrawer mpuikitDrawer;
        [SerializeProperty(nameof(mpuikitDrawer))]
        public MPUIKitDrawer MPUIKitDrawer => mpuikitDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] SpriteRendererDrawer spriteRendererDrawer;
        [SerializeProperty(nameof(spriteRendererDrawer))]
        public SpriteRendererDrawer SpriteRendererDrawer => spriteRendererDrawer.SetMonoBehaviour(monoBeh);

#if NOVA_UI_EXISTS
        [SerializeField] NovaImageDrawer novaImageDrawer;
        [SerializeProperty(nameof(novaImageDrawer))]
        public NovaImageDrawer NovaImageDrawer => novaImageDrawer.SetMonoBehaviour(monoBeh);
#endif
    }
}