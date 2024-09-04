using DA_Assets.Shared;
using System;
using UnityEngine;

namespace DA_Assets.FCU.Model
{
    [Serializable]
    public class NovaSettings : MonoBehaviourBinder<FigmaConverterUnity>
    {
        [SerializeField] Texture inputTexture;
        [SerializeProperty(nameof(inputTexture))]
        public Texture InputTexture { get => inputTexture; set => SetValue(ref inputTexture, value); }
    }
}
