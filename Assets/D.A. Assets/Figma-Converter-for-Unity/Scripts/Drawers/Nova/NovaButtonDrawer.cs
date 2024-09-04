#if NOVA_UI_EXISTS
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using Nova;
using NovaSamples.UIControls;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class NovaButtonDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject)
        {
            fobject.Data.GameObject.TryGetComponent(out UIBlock2D uIBlock2D);
            fobject.Data.GameObject.TryAddComponent(out Button btn);
            fobject.Data.GameObject.TryAddComponent(out Interactable interactable);
            fobject.Data.GameObject.TryAddComponent(out ItemView itemView);

            Type itemViewType = typeof(ItemView);

            string fieldName = "visuals";

            FieldInfo visualsField = itemViewType.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            ButtonVisuals newVisuals = new ButtonVisuals();

            if (visualsField != null)
            {
                visualsField.SetValue(itemView, newVisuals);
            }
            else
            {
                DALogger.LogError($"{fieldName} not found in '{itemViewType.Name}'");
            }

            SetupNovaButton(fobject, newVisuals);
        }

        public void SetupNovaButton(FObject fobject, ButtonVisuals btn)
        {
            monoBeh.CanvasDrawer.ButtonDrawer.UnityButtonDrawer.SetupSelectable(fobject.Data, out SyncHelper[] btnChilds, out bool hasCustomButtonBackgrounds);

            if (hasCustomButtonBackgrounds)
            {
                SetCustomTargetGraphics(btnChilds, btn);
            }
            else
            {
                SetDefaultTargetGraphic(fobject, btnChilds, btn);
            }
        }

        public void SetDefaultTargetGraphic(FObject fobject, SyncHelper[] syncHelpers, ButtonVisuals btnVisuals)
        {
            UIBlock gr1 = null;
            bool exists = !syncHelpers.IsEmpty() && syncHelpers.First().TryGetComponent(out gr1);

            //If the first element of the hierarchy can be used as a target graphic.
            if (exists)
            {
                btnVisuals.TransitionTarget = gr1;
            }
            else
            {
                //If there is at least some image, assign it to the targetGraphic.
                foreach (SyncHelper meta in syncHelpers)
                {
                    if (meta.TryGetComponent(out UIBlock2D gr2))
                    {
                        btnVisuals.TransitionTarget = gr2;
                        return;
                    }
                }

                //If there is at least some graphic, assign it to the targetGraphic.
                foreach (SyncHelper meta in syncHelpers)
                {
                    if (meta.TryGetComponent(out UIBlock gr3))
                    {
                        btnVisuals.TransitionTarget = gr3;
                        return;
                    }
                }

                //If there is a graphic on the button itself, assign it to the targetGraphic.
                if (fobject.Data.GameObject.TryGetComponent(out UIBlock gr4))
                {
                    btnVisuals.TransitionTarget = gr4;
                }
            }
        }

        private void SetCustomTargetGraphics(SyncHelper[] syncHelpers, ButtonVisuals btn)
        {
            foreach (SyncHelper syncHelper in syncHelpers)
            {
                if (syncHelper.ContainsTag(FcuTag.Image))
                {
                    if (btn.TransitionType == TransitionType.SpriteSwap)
                    {
                        SetSprite(btn, syncHelper);
                    }
                    else
                    {
                        SetImageColor(syncHelper, btn);
                    }
                }
                else if (syncHelper.ContainsTag(FcuTag.Text))
                {
                    SetText(syncHelper, btn);
                }
            }
        }

        public void SetSprite(ButtonVisuals selectable, SyncHelper syncHelper)
        {
            selectable.TransitionType = TransitionType.SpriteSwap;
            SpriteStates spriteState = selectable.Sprites;

            if (syncHelper.ContainsTag(FcuTag.BtnDefault))
            {
                if (syncHelper.TryGetComponent(out UIBlock2D graphic))
                {
                    selectable.TransitionTarget = graphic;
                    spriteState.DefaultSprite = graphic.Sprite;
                }
            }
            else
            {
                if (syncHelper.TryGetComponent(out UIBlock2D img))
                {
                    if (syncHelper.ContainsTag(FcuTag.BtnPressed))
                    {
                        spriteState.PressedSprite = img.Sprite;
                        DisableBody(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnHover))
                    {
                        spriteState.HoveredSprite = img.Sprite;
                        DisableBody(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnSelected))
                    {
                        DisableBody(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnDisabled))
                    {
                        DisableBody(syncHelper.gameObject);
                    }
                }
            }

            selectable.Sprites = spriteState;
        }

        public void SetImageColor(SyncHelper syncHelper, ButtonVisuals selectable)
        {
            selectable.TransitionType = TransitionType.ColorChange;

            if (syncHelper.TryGetComponent(out UIBlock graphic))
            {
                if (syncHelper.ContainsTag(FcuTag.BtnDefault))
                {
                    selectable.TransitionTarget = graphic;
                    selectable.DefaultColor = graphic.Color;
                }
                else
                {
                    if (syncHelper.ContainsTag(FcuTag.BtnPressed))
                    {
                        selectable.PressedColor = graphic.Color;
                        DisableBody(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnHover))
                    {
                        selectable.HoveredColor = graphic.Color;
                        DisableBody(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnSelected))
                    {
                        DisableBody(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnDisabled))
                    {
                        DisableBody(syncHelper.gameObject);
                    }
                }
            }
        }

        private void SetText(SyncHelper syncHelper, ButtonVisuals btn)
        {
            if (syncHelper.TryGetComponent(out TextBlock textBlock))
            {
                if (syncHelper.ContainsTag(FcuTag.BtnDefault))
                {
                    btn.Label = textBlock;
                }
                else
                {
                    if (syncHelper.ContainsTag(FcuTag.BtnPressed))
                    {
                        DisableText(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnHover))
                    {
                        DisableText(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnSelected))
                    {
                        DisableText(syncHelper.gameObject);
                    }
                    else if (syncHelper.ContainsTag(FcuTag.BtnDisabled))
                    {
                        DisableText(syncHelper.gameObject);
                    }
                }
            }
        }

        private void DisableBody(GameObject go)
        {
            if (go.TryGetComponent(out UIBlock2D uiBlock2D))
            {
                uiBlock2D.BodyEnabled = false;
            }

            //TODO: crash when destroying Nova component.
            //go.Destroy();
        }

        private void DisableText(GameObject go)
        {
            if (go.TryGetComponent(out TextBlock textBlock))
            {
                textBlock.Text = "";
            }

            //TODO: crash when destroying Nova component.
            //go.Destroy();
        }
    }
}
#endif