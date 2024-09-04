#if NOVA_UI_EXISTS
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using Nova;
using NovaSamples.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#pragma warning disable IDE0003
#pragma warning disable CS0649

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class NovaDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator DrawToScene(List<FObject> fobjects)
        {
            monoBeh.AssetTools.SelectFcu();

            monoBeh.CanvasDrawer.TextDrawer.Init();
            yield return monoBeh.CanvasDrawer.DrawComponents(fobjects, DrawByTag);

            yield return null;
        }

        private IEnumerator DrawByTag(FObject fobject, FcuTag tag, Action onDraw)
        {
            try
            {
                if (fobject.Data.GameObject == null)
                {
                    yield break;
                }

                switch (tag)
                {
                    case FcuTag.Blur:
                        this.NovaBlurDrawer.Draw(fobject);
                        break;

                    case FcuTag.Shadow:
                        this.NovaShadowDrawer.Draw(fobject);
                        break;

                    case FcuTag.AutoLayoutGroup:
                        this.NovaAutoLayoutDrawer.Draw(fobject);
                        break;

                    case FcuTag.ContentSizeFitter:

                        break;

                    case FcuTag.AspectRatioFitter:

                        break;

                    case FcuTag.InputField:

                        break;

                    case FcuTag.Button:
                        this.NovaButtonDrawer.Draw(fobject);
                        break;

                    case FcuTag.Mask:
                        this.NovaMaskDrawer.Draw(fobject);
                        break;

                    case FcuTag.CanvasGroup:

                        break;

                    case FcuTag.Placeholder:
                    case FcuTag.Text:
                        monoBeh.CanvasDrawer.TextDrawer.Draw(fobject);
                        monoBeh.CanvasDrawer.I2LocalizationDrawer.AddI2Localize(fobject);
                        break;

                    case FcuTag.Image:
                        monoBeh.CanvasDrawer.ImageDrawer.Draw(fobject);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            onDraw.Invoke();
            yield return null;
        }

        internal void SetupSpace()
        {
            return;
            if (monoBeh.TryGetComponent(out Canvas canvas))
            {
                canvas.RemoveComponentsDependingOn();
                canvas.Destroy();
            }

            if (monoBeh.TagSetter.TagsCounter.TryGetValue(FcuTag.Blur, out int blurCount))
            {
                if (blurCount > 0)
                {
                    Camera mc = CameraTools.GetOrCreateMainCamera();
                    Camera bgbc = CameraTools.GetOrCreateBackgroundBlurCamera();

                    monoBeh.gameObject.TryAddComponent(out BackgroundBlurGroup blurGroup);
                    blurGroup.PropertyMatchCamera = mc;
                    blurGroup.BackgroundCamera = bgbc;

                    blurGroup.BlurEffects = blurGroup.BlurEffects.Where(x => x != null).ToList();

                    monoBeh.gameObject.TryAddComponent(out ScreenSpace screenSpace);
                    screenSpace.TargetCamera = mc;
                    screenSpace.enabled = false;
                    screenSpace.AddAdditionalCamera(bgbc);
                }
            }
        }

        internal void EnableScreenSpaceComponent()
        {
            if (monoBeh.gameObject.TryGetComponent(out ScreenSpace screenSpace))
            {
                screenSpace.enabled = true;
            }
        }

        [SerializeField] NovaButtonDrawer novaButtonDrawer;
        [SerializeProperty(nameof(novaButtonDrawer))]
        public NovaButtonDrawer NovaButtonDrawer => novaButtonDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] NovaShadowDrawer novaShadowDrawer;
        [SerializeProperty(nameof(novaShadowDrawer))]
        public NovaShadowDrawer NovaShadowDrawer => novaShadowDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] NovaBlurDrawer novaBlurDrawer;
        [SerializeProperty(nameof(novaBlurDrawer))]
        public NovaBlurDrawer NovaBlurDrawer => novaBlurDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] NovaMaskDrawer novaMaskDrawer;
        [SerializeProperty(nameof(novaMaskDrawer))]
        public NovaMaskDrawer NovaMaskDrawer => novaMaskDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] NovaAutoLayoutDrawer novaAutoLayoutDrawer;
        [SerializeProperty(nameof(novaAutoLayoutDrawer))]
        public NovaAutoLayoutDrawer NovaAutoLayoutDrawer => novaAutoLayoutDrawer.SetMonoBehaviour(monoBeh);
    }
}
#endif