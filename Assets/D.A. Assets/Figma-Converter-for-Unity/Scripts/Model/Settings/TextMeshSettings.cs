using UnityEngine;
using System;
using DA_Assets.Shared;

#if TextMeshPro
using TMPro;
#endif

namespace DA_Assets.FCU.Model
{
    [Serializable]
    public class TextMeshSettings : MonoBehaviourBinder<FigmaConverterUnity>
    {
        [SerializeField] bool autoSize = false;
        public bool AutoSize { get => autoSize; set => SetValue(ref autoSize, value); }

        [SerializeField] bool overrideTags = false;
        public bool OverrideTags { get => overrideTags; set => SetValue(ref overrideTags, value); }

        [SerializeField] bool wrapping = true;
        public bool Wrapping { get => wrapping; set => SetValue(ref wrapping, value); }

        [SerializeField] bool orthographicMode = true;
        /// <summary>
        /// For NOVA only.
        /// </summary>
        public bool OrthographicMode { get => orthographicMode; set => SetValue(ref orthographicMode, value); }

        [SerializeField] bool richText = true;
        public bool RichText { get => richText; set => SetValue(ref richText, value); }

        [SerializeField] bool raycastTarget = true;
        public bool RaycastTarget { get => raycastTarget; set => SetValue(ref raycastTarget, value); }

        [SerializeField] bool parseEscapeCharacters = true;
        public bool ParseEscapeCharacters { get => parseEscapeCharacters; set => SetValue(ref parseEscapeCharacters, value); }

        [SerializeField] bool visibleDescender = true;
        public bool VisibleDescender { get => visibleDescender; set => SetValue(ref visibleDescender, value); }

        [SerializeField] bool kerning = true;
        public bool Kerning { get => kerning; set => SetValue(ref kerning, value); }

        [SerializeField] bool extraPadding = false;
        public bool ExtraPadding { get => extraPadding; set => SetValue(ref extraPadding, value); }

#if TextMeshPro
        [SerializeField] TextOverflowModes overflow = TextOverflowModes.Overflow;
        public TextOverflowModes Overflow { get => overflow; set => SetValue(ref overflow, value); }

        [SerializeField] TextureMappingOptions horizontalMapping = TextureMappingOptions.Character;
        public TextureMappingOptions HorizontalMapping { get => horizontalMapping; set => SetValue(ref horizontalMapping, value); }

        [SerializeField] TextureMappingOptions verticalMapping = TextureMappingOptions.Character;
        public TextureMappingOptions VerticalMapping { get => verticalMapping; set => SetValue(ref verticalMapping, value); }

        [SerializeField] VertexSortingOrder geometrySorting = VertexSortingOrder.Normal;
        public VertexSortingOrder GeometrySorting { get => geometrySorting; set => SetValue(ref geometrySorting, value); }

#if RTLTMP_EXISTS
        [SerializeField] bool farsi = true;
        public bool Farsi { get => farsi; set => SetValue(ref farsi, value); }

        [SerializeField] bool forceFix = false;
        public bool ForceFix { get => forceFix; set => SetValue(ref forceFix, value); }

        [SerializeField] bool preserveNumbers = false;
        public bool PreserveNumbers { get => preserveNumbers; set => SetValue(ref preserveNumbers, value); }

        [SerializeField] bool fixTags = true;
        public bool FixTags { get => fixTags; set => SetValue(ref fixTags, value); }
#endif
#endif

        [SerializeField] Shader shader;
        public Shader Shader { get => shader; set => SetValue(ref shader, value); }
    }

    [Flags]
    public enum FontSubset
    {
        Latin = 1 << 0,
        LatinExt = 1 << 1,
        Sinhala = 1 << 2,
        Greek = 1 << 3,
        Hebrew = 1 << 4,
        Vietnamese = 1 << 5,
        Cyrillic = 1 << 6,
        CyrillicExt = 1 << 7,
        Devanagari = 1 << 8,
        Arabic = 1 << 9,
        Khmer = 1 << 10,
        Tamil = 1 << 11,
        GreekExt = 1 << 12,
        Thai = 1 << 13,
        Bengali = 1 << 14,
        Gujarati = 1 << 15,
        Oriya = 1 << 16,
        Malayalam = 1 << 17,
        Gurmukhi = 1 << 18,
        Kannada = 1 << 19,
        Telugu = 1 << 20,
        Myanmar = 1 << 21,
    }
}