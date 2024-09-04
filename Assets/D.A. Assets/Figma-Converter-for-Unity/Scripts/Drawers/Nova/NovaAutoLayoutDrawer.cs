#if NOVA_UI_EXISTS
using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using Nova;
using System;
using UnityEngine;

namespace DA_Assets.FCU.Drawers
{
    [Serializable]
    public class NovaAutoLayoutDrawer : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void Draw(FObject fobject)
        {
            //TODO: Nova crash.
            return;
            fobject.Data.GameObject.TryGetComponent(out UIBlock2D uIBlock2D);

            AutoLayout autoLayout = new AutoLayout();

            TextAnchor childAlignment = TextAnchor.UpperLeft;

            if (fobject.LayoutWrap == LayoutWrap.WRAP)
            {
                return;
                //this.GridLayoutDrawer.Draw(fobject);
            }
            else if (fobject.LayoutMode == LayoutMode.HORIZONTAL)
            {
                autoLayout.Axis = Axis.X;
                childAlignment = fobject.GetHorLayoutAnchor();
            }
            else if (fobject.LayoutMode == LayoutMode.VERTICAL)
            {
                autoLayout.Axis = Axis.Y;
                childAlignment = fobject.GetVertLayoutAnchor();
            }

            SetAligment(childAlignment, ref autoLayout);    

            autoLayout.SpacingMinMax = new MinMax(-Mathf.Infinity, Mathf.Infinity);

            if (fobject.PrimaryAxisAlignItems == PrimaryAxisAlignItem.SPACE_BETWEEN ||
                fobject.CounterAxisAlignItems == CounterAxisAlignItem.SPACE_BETWEEN)
            {
                autoLayout.Spacing = 0f;
                autoLayout.AutoSpace = true;
            }
            else
            {
                autoLayout.AutoSpace = false;
                autoLayout.Spacing = fobject.GetHorSpacing();
            }

            uIBlock2D.AutoLayout = autoLayout;
        }

        private void SetAligment(TextAnchor childAlignment, ref AutoLayout autoLayout)
        {
            int al = -1;

            switch (childAlignment)
            {
                case TextAnchor.MiddleLeft:
                    al = -1;
                    break;
                case TextAnchor.MiddleCenter:
                    al = 0;
                    break;
                case TextAnchor.MiddleRight:
                    al = 1;
                    break;
            }

            autoLayout.Alignment = al;
        }
    }
}
#endif