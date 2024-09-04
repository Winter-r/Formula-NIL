#if NOVA_UI_EXISTS
using Nova;
using System;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    public struct NovaTransformData
    {
        [SerializeField] Layout layout;

        public NovaTransformData(UIBlock source)
        {
            layout = source.Layout;
        }

        public void ApplyTo(UIBlock target)
        {
            target.Layout = layout;
        }
    }
}
#endif