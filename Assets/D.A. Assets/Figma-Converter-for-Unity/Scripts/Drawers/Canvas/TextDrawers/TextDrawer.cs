using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace DA_Assets.FCU.Drawers.CanvasDrawers
{
    [Serializable]
    public class TextDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        [SerializeField] List<FObject> texts;
        public List<FObject> Texts => texts;

        public void Init()
        {
            texts = new List<FObject>();
        }

        public void Draw(FObject fobject)
        {
            if (fobject.Data.GameObject.IsPartOfAnyPrefab() == false)
            {
                if (fobject.Data.GameObject.TryGetComponent(out Graphic oldGraphic))
                {
                    Type curType = monoBeh.GetCurrentTextType();

                    if (oldGraphic.GetType().Equals(curType) == false)
                    {
                        //TODO
                       //oldGraphic.RemoveComponentsDependingOn();
                        oldGraphic.Destroy();
                    }
                }
            }

            if (monoBeh.IsNova())
            {
                this.TextMeshDrawer.DrawNovaTMP(fobject);
            }
            else
            {
                switch (monoBeh.Settings.ComponentSettings.TextComponent)
                {
                    case TextComponent.TextMeshPro:
                        this.TextMeshDrawer.DrawTMP(fobject);
                        break;
                    case TextComponent.RTLTextMeshPro:
                        this.TextMeshDrawer.DrawRTL(fobject);
                        break;
                    case TextComponent.UnityText:
                        this.UnityTextDrawer.Draw(fobject);
                        break;
                }
            }

            texts.Add(fobject); 
        }

        [SerializeField] UnityTextDrawer unityTextDrawer;
        [SerializeProperty(nameof(unityTextDrawer))]
        public UnityTextDrawer UnityTextDrawer => unityTextDrawer.SetMonoBehaviour(monoBeh);

        [SerializeField] TextMeshDrawer textMeshDrawer;
        [SerializeProperty(nameof(textMeshDrawer))]
        public TextMeshDrawer TextMeshDrawer => textMeshDrawer.SetMonoBehaviour(monoBeh);
    }
}
