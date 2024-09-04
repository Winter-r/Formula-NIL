using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

#if NOVA_UI_EXISTS
using Nova;
#endif

namespace DA_Assets.FCU
{
    [Serializable]
    public class TransformSetter : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public IEnumerator SetTransformPos(List<FObject> fobjects)
        {
            DALogger.Log(FcuLocKey.log_start_setting_transform.Localize());

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                fobject.Data.Angle = fobject.GetFigmaRotationAngle(monoBeh);
            }

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                RectTransform rt = fobject.Data.GameObject.GetComponent<RectTransform>();
                rt.SetSmartAnchor(AnchorType.TopLeft);
                rt.SetSmartPivot(PivotType.TopLeft);

                Rect rect = GetGlobalRect(fobject);
                fobject.Data.Size = rect.size;
                fobject.Data.Position = rect.position;

                rt.sizeDelta = rect.size;
                rt.position = rect.position;

                fobject.Data.GameObject.transform.localScale = Vector3.one;
            }

            GameObject tempGO = MonoBehExtensions.CreateEmptyGameObject();
            tempGO.transform.parent = monoBeh.transform;
             Transform tempTr = tempGO.transform;

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                RectTransform rt = fobject.Data.GameObject.GetComponent<RectTransform>();

                List<Transform> children = new List<Transform>();
                foreach (Transform child in fobject.Data.GameObject.transform)
                {
                    children.Add(child);
                }

                foreach (Transform child in children)
                {
                    child.SetParent(tempTr);
                }


                rt.SetSmartPivot(PivotType.MiddleCenter);

                if (fobject.IsNeedRotate(monoBeh))
                {
                    fobject.SetFigmaRotation(monoBeh);
                }


                foreach (Transform child in children)
                {
                    child.SetParent(fobject.Data.GameObject.transform);
                }
            }

            tempGO.Destroy();

            yield return null;
        }

        public IEnumerator SetTransformPosAndAnchors(List<FObject> fobjects)
        {
            DALogger.Log(FcuLocKey.log_start_setting_transform.Localize());

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                if (!fobject.IsInsideAutoLayout(out FObject _, out var __, monoBeh))
                    continue;

                RectTransform rt = fobject.Data.GameObject.GetComponent<RectTransform>();
                Rect rect = GetAutolayoutRect(fobject);
                fobject.Data.Size = rect.size;
                rt.sizeDelta = fobject.Data.Size;
            }

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                RectTransform rt = fobject.Data.GameObject.GetComponent<RectTransform>();

                rt.SetSmartPivot(monoBeh.Settings.MainSettings.PivotType);

                if (!fobject.ContainsTag(FcuTag.Frame) && !fobject.Data.Parent.ContainsTag(FcuTag.AutoLayoutGroup))
                {
                    rt.SetSmartAnchor(fobject.GetFigmaAnchor());
                }
            }

            yield return SetRootFramesPosition(fobjects);

            yield return monoBeh.ReEnableRectTransform();
        }

        public Rect GetAutolayoutRect(FObject fobject)
        {
            Rect rect = new Rect();
            Vector2 position = new Vector2();
            Vector2 size = new Vector2();

            size = fobject.Data.Size;

            if (fobject.TryFixSizeWithStroke(size.y, out float newY))
            {
                size.y = newY;
            }

            monoBeh.Log($"{nameof(GetAutolayoutRect)} | {fobject.Data.NameHierarchy} | {size} | {position}", FcuLogType.Transform);

            rect.size = size;
            rect.position = position;

            return rect;
        }

        public Rect GetGlobalRect(FObject fobject)
        {
            Rect rect = new Rect();
            Vector2 position = new Vector2();
            Vector2 size = new Vector2();

            bool hasBoundingSize = fobject.GetBoundingSize(out Vector2 bSize);
            bool hasBoundingPos = fobject.GetBoundingPosition(out Vector2 bPos);

            bool hasRenderSize = fobject.GetRenderSize(out Vector2 rSize);
            bool hasRenderPos = fobject.GetRenderPosition(out Vector2 rPos);

            bool hasLocalPos = fobject.TryGetLocalPosition(out Vector3 lPos);

            float scale = 1;
            bool hasScaleInName = fobject.IsDownloadableType() && fobject.Data.SpritePath.TryParseSpriteName(out scale, out var _);

            int state = 0;

            if (hasScaleInName)
            {
                if (hasRenderPos)
                {
                    state = 1;

                    position.x = rPos.x;
                    position.y = monoBeh.IsUGUI() || monoBeh.IsNova() ? -rPos.y : rPos.y;
                }
                else
                {
                    state = 2;

                    position.x = bPos.x;
                    position.y = monoBeh.IsUGUI() || monoBeh.IsNova() ? -bPos.y : bPos.y;
                }

                size.x = fobject.Data.SpriteSize.x / scale;
                size.y = fobject.Data.SpriteSize.y / scale;
            }
            else if (fobject.IsGenerativeType() || fobject.IsDrawableType())
            {
                state = 3;
                size = fobject.Size;

                position.x = bPos.x;
                position.y = monoBeh.IsUGUI() || monoBeh.IsNova() ? -bPos.y : bPos.y;
            }
            else
            {
                state = 4;
                size.x = bSize.x;
                size.y = bSize.y;

                position.x = bPos.x;
                position.y = monoBeh.IsUGUI() || monoBeh.IsNova() ? -bPos.y : bPos.y;
            }

            if (fobject.TryFixSizeWithStroke(size.y, out float newY))
            {
                size.y = newY;
            }

            monoBeh.Log($"{nameof(GetGlobalRect)} | {fobject.Data.NameHierarchy} | state: {state} | {size} | {position}", FcuLogType.Transform);

            rect.size = size;
            rect.position = position;

            return rect;
        }

        private IEnumerator SetRootFramesPosition(List<FObject> fobjects)
        {
            IEnumerable<FrameGroup> fobjectsByFrame = fobjects
                .GroupBy(x => x.Data.RootFrame)
                .Select(g => new FrameGroup
                {
                    Childs = g.Select(x => x).ToList(),
                    RootFrame = g.First()
                });

            if (monoBeh.Settings.MainSettings.PositioningMode == PositioningMode.Absolute)
            {
                foreach (FrameGroup rootFrame in fobjectsByFrame)
                {
                    if (rootFrame.RootFrame.Data.GameObject == null)
                        continue;

                    RectTransform rt = rootFrame.RootFrame.Data.GameObject.GetComponent<RectTransform>();
                    rt.SetSmartAnchor(AnchorType.TopLeft);
                }
            }
            else
            {
                yield return monoBeh.AssetTools.ReselectFcu();
                monoBeh.AssetTools.CacheResolutionData();

                foreach (FrameGroup rootFrame in fobjectsByFrame)
                {
                    if (rootFrame.RootFrame.Data.GameObject == null)
                        continue;

                    yield return WaitFor.Delay001();
                    monoBeh.DelegateHolder.SetGameViewSize(rootFrame.RootFrame.Size);
                    yield return WaitFor.Delay01();

                    RectTransform rt = rootFrame.RootFrame.Data.GameObject.GetComponent<RectTransform>();
                    rt.SetSmartAnchor(AnchorType.StretchAll);
                    rt.offsetMin = new Vector2(0, 0);
                    rt.offsetMax = new Vector2(0, 0);
                    rt.localScale = Vector3.one;
                }

                yield return monoBeh.AssetTools.ReselectFcu();
                monoBeh.AssetTools.RestoreResolutionData();
            }
        }

        public IEnumerator SetNovaPositions(List<FObject> fobjects)
        {
#if NOVA_UI_EXISTS
            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                fobject.Data.GameObject.TryAddComponent(out UIBlock2D uiBlock2d);

                if (fobject.ContainsTag(FcuTag.Text))
                {
                    Length3 blockPos = uiBlock2d.Layout.Position;

                    yield return null;
                    uiBlock2d.Destroy();

                    fobject.Data.GameObject.TryAddComponent(out TextBlock textBlock);
                    textBlock.Layout.Position = blockPos;               
                }

                UIBlock uiBlock = fobject.Data.GameObject.GetComponent<UIBlock>();
                uiBlock.Color = default;

                uiBlock.Layout.Size = new Length3
                {
                    X = fobject.Data.Size.x,
                    Y = fobject.Data.Size.y,
                };
            }
#endif

            foreach (FObject fobject in fobjects)
            {
                if (fobject.Data.GameObject == null)
                    continue;

                if (fobject.Data.GameObject.TryGetComponent(out TMP_Text txt))
                    continue;

                if (fobject.Data.GameObject.TryGetComponent(out RectTransform rt))
                {
                    try
                    {
                        Transform newTransform = fobject.Data.GameObject.AddComponent<Transform>();
                        newTransform.position = rt.position;
                        newTransform.rotation = rt.rotation;
                        newTransform.localScale = rt.localScale;

                        UnityEngine.Object.DestroyImmediate(rt);
                    }
                    catch
                    {

                    }
                }
            }

            yield return null;
        }
    }

    public struct FrameGroup
    {
        public FObject RootFrame { get; set; }
        public List<FObject> Childs { get; set; }
    }
}