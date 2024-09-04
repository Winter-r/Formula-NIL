using System;
using UnityEngine;

namespace DA_Assets.FCU.Model
{
    [Serializable]
    public class MPUIKitSettings : BaseImageSettings
    {
        [SerializeField] float falloffDistance = 0.5f;
        public float FalloffDistance { get => falloffDistance; set => SetValue(ref falloffDistance, value); }
    }
}