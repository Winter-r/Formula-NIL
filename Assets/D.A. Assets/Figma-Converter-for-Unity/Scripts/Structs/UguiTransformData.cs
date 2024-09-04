using System;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    public struct UguiTransformData
    {
        [SerializeField] Vector2 anchorMin;
        [SerializeField] Vector2 anchorMax;
        [SerializeField] Vector2 anchoredPosition;
        [SerializeField] Vector3 sizeDelta;
        [SerializeField] Quaternion localRotation;
        [SerializeField] Vector3 localScale;

        public UguiTransformData(RectTransform source)
        {
            anchorMin = source.anchorMin;
            anchorMax = source.anchorMax;
            anchoredPosition = source.anchoredPosition;
            sizeDelta = source.sizeDelta;
            localRotation = source.localRotation;
            localScale = source.localScale;
        }

        public void ApplyTo(RectTransform target)
        {
            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.anchoredPosition = anchoredPosition;
            target.sizeDelta = sizeDelta;
            target.localRotation = localRotation;
            target.localScale = localScale;
        }
    }
}
