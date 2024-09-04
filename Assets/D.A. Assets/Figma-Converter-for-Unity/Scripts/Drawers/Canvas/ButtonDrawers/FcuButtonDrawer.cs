using DA_Assets.Shared.Extensions;
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.FCU.UI;
using DA_Assets.Shared;
using UnityEngine.UI;
using TMPro;
using System;

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class FcuButtonDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void SetupFcuButton(SyncData btnSyncData)
        {
            monoBeh.CanvasDrawer.ButtonDrawer.UnityButtonDrawer.SetupSelectable(btnSyncData, out SyncHelper[] btnChilds, out bool hasCustomButtonBackgrounds);

            FcuButton btn = btnSyncData.GameObject.GetComponent<FcuButton>();

            if (hasCustomButtonBackgrounds)
            {
                SetCustomTargetGraphics(btnChilds, btn);
            }
            else
            {
                monoBeh.CanvasDrawer.ButtonDrawer.UnityButtonDrawer.SetDefaultTargetGraphic(btnChilds, btn);
            }
        }

        private void SetCustomTargetGraphics(SyncHelper[] syncHelpers, FcuButton btn)
        {
            foreach (SyncHelper syncHelper in syncHelpers)
            {
                if (syncHelper.ContainsTag(FcuTag.Image))
                {
                    if (btn.transition == Selectable.Transition.SpriteSwap)
                    {
                        monoBeh.CanvasDrawer.ButtonDrawer.UnityButtonDrawer.SetSprite(btn, syncHelper);
                    }
                    else
                    {
                        monoBeh.CanvasDrawer.ButtonDrawer.UnityButtonDrawer.SetImageColor(btn, syncHelper);
                    }
                }
                else if (syncHelper.ContainsTag(FcuTag.Text))
                {
                    SetText(btn, syncHelper);
                }
            }
        }

        private void SetText(FcuButton btn, SyncHelper syncHelper)
        {
            if (syncHelper.TryGetComponent(out Graphic graphic))
            {
                if (syncHelper.ContainsAnyTag(
                   FcuTag.BtnDefault,
                   FcuTag.BtnDisabled,
                   FcuTag.BtnHover,
                   FcuTag.BtnPressed,
                   FcuTag.BtnSelected))
                {
                    btn.ChangeTextColor = true;
                }

                if (syncHelper.ContainsTag(FcuTag.BtnDefault))
                {
                    btn.TextDefaultColor = graphic.color;

                    switch (monoBeh.Settings.ComponentSettings.TextComponent)
                    {
                        case TextComponent.UnityText:
                            btn.ButtonText = syncHelper.GetComponent<Text>();
                            break;
                        case TextComponent.TextMeshPro:
#if TextMeshPro
                            btn.ButtonText = syncHelper.GetComponent<TMP_Text>();
#endif
                            break;
                    }
                }
                else
                {
                    if (syncHelper.ContainsTag(FcuTag.BtnPressed))
                    {
                        btn.TextPressedColor = graphic.color;
                        syncHelper.gameObject.Destroy();
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnHover))
                    {
                        btn.TextHoverColor = graphic.color;
                        syncHelper.gameObject.Destroy();
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnSelected))
                    {
                        btn.TextSelectedColor = graphic.color;
                        syncHelper.gameObject.Destroy();
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnDisabled))
                    {
                        btn.TextDisabledColor = graphic.color;
                        syncHelper.gameObject.Destroy();
                    }
                }
            }
        }
    }
}
