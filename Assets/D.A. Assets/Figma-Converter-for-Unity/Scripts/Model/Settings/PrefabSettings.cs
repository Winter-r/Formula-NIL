using DA_Assets.Shared;
using System;
using UnityEngine;

namespace DA_Assets.FCU.Model
{
    [Serializable]
    public class PrefabSettings : MonoBehaviourBinder<FigmaConverterUnity>
    {
        [SerializeField] string prefabsPath = "Assets/Prefabs";
        public string PrefabsPath { get => prefabsPath; set => SetValue(ref prefabsPath, value); }

        [SerializeField] TextPrefabNameType textPrefabNameType = TextPrefabNameType.HumanizedColorString;
        public TextPrefabNameType TextPrefabNameType { get => textPrefabNameType; set => SetValue(ref textPrefabNameType, value); }
    }

    public enum TextPrefabNameType
    {
        HumanizedColorString,
        HumanizedColorHEX,
        Figma,
    }
}
