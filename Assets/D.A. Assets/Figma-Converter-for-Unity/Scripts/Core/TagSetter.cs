using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DA_Assets.FCU
{
    [Serializable]
    public class TagSetter : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public Dictionary<FcuTag, int> TagsCounter { get; set; } = new Dictionary<FcuTag, int>();

        public void SetTags(FObject page)
        {
            DALogger.Log(FcuLocKey.log_tagging.Localize());

            NameSetter.ClearNames();

            SetTagsByFigma(page);
            SetSmartTags(page);
        }

        private void SetTagsByFigma(FObject parent)
        {
            if (parent.ContainsTag(FcuTag.Frame))
            {
                parent.Data.Hierarchy = new List<FcuHierarchy>
                {
                    new FcuHierarchy
                    {
                        Index = -1,
                        Name = parent.Data.ObjectName,
                        Guid = parent.Data.UitkGuid
                    }
                };
            }

            for (int i = 0; i < parent.Children.Count; i++)
            {
                FObject child = parent.Children[i];

                child.Data = new SyncData
                {
                    Id = child.Id,
                    ChildIndexes = new List<int>(),
                    Ignore = !child.IsVisible(),
                    Parent = parent,
                };

                monoBeh.NameSetter.SetNames(child);

                if (GetManualTag(child, out FcuTag manualTag))
                {
                    child.AddTag(manualTag);
                    monoBeh.Log($"GetManualTag | {child.Name} | {manualTag}", FcuLogType.SetTag);

                    if (manualTag == FcuTag.Image)
                    {
                        child.Data.ForceImage = true;
                    }
                    else if (manualTag == FcuTag.Container)
                    {
                        child.Data.ForceContainer = true;
                    }
                }

                if (parent.ContainsTag(FcuTag.Page))
                {
                    child.AddTag(FcuTag.Frame);
                }

                if (child.Type == NodeType.INSTANCE)
                {
                    //TODO
                }

                if (child.LayoutWrap == LayoutWrap.WRAP ||
                    child.LayoutMode == LayoutMode.HORIZONTAL ||
                    child.LayoutMode == LayoutMode.VERTICAL)
                {
                    if (child.HasActiveProperty(x => x.Children))
                    {
                        child.AddTag(FcuTag.AutoLayoutGroup);
                    }
                }

                if (child.PreserveRatio.ToBoolNullFalse())
                {
                    child.AddTag(FcuTag.AspectRatioFitter);
                }

                if (child.IsAnyMask())
                {
                    child.AddTag(FcuTag.Mask);
                }

                if (child.Name.ToLower() == "button")
                {
                    child.AddTag(FcuTag.Button);
                }

                if (child.Type == NodeType.TEXT)
                {
                    child.AddTag(FcuTag.Text);

                    if (child.Style.IsDefault() == false)
                    {
                        if (child.Style.TextAutoResize == "WIDTH_AND_HEIGHT")
                        {
                            child.AddTag(FcuTag.ContentSizeFitter);
                        }
                    }
                }
                else if (child.Type == NodeType.VECTOR)
                {
                    child.AddTag(FcuTag.Image);
                }
                else if (child.HasActiveProperty(x => x.Fills) || child.HasActiveProperty(x => x.Strokes))
                {
                    child.AddTag(FcuTag.Image);
                }

                if (child.Effects.IsEmpty() == false)
                {
                    IEnumerable<Effect> activeEffects;

                    if (monoBeh.IsNova())
                    {
                        activeEffects = child.Effects;
                    }
                    else
                    {
                        activeEffects = child.Effects.Where(x => x.Visible.ToBoolNullFalse());
                    }

                    Effect[] allShadows = activeEffects.Where(x => x.IsShadowType()).ToArray();
                    Effect[] dropShadows = activeEffects.Where(x => x.Type == EffectType.DROP_SHADOW).ToArray();

                    if (monoBeh.IsUGUI() && monoBeh.UsingTrueShadow() && !monoBeh.UsingSpriteRenderer())
                    {
                        if (allShadows.Length > 0)
                        {
                            child.AddTag(FcuTag.Shadow);
                        }
                    }
                    else if (monoBeh.IsUITK())
                    {

                    }
                    else if (monoBeh.IsNova())
                    {
                        if (allShadows.Length == 1 && dropShadows.Length == 1)
                        {
                            child.AddTag(FcuTag.Shadow);
                        }
                    }

                    if (monoBeh.IsNova())
                    {
                        foreach (Effect effect in child.Effects)
                        {
                            if (effect.Type == EffectType.BACKGROUND_BLUR)
                            {
                                child.AddTag(FcuTag.Blur);
                            }
                        }
                    }
                }

                child.Data.IsOverlappedByStroke = IsOverlappedByStroke(child);

                if (child.Opacity.HasValue && child.Opacity != 1)
                {
                    child.AddTag(FcuTag.CanvasGroup);
                }

                child.Data.Hierarchy.AddRange(parent.Data.Hierarchy);

                int sceneIndex = GetNewIndex(parent, i);
                child.Data.Hierarchy.Add(new FcuHierarchy
                {
                    Index = sceneIndex,
                    Name = child.Data.ObjectName,
                    Guid = child.Data.UitkGuid,
                });

                parent.Children[i] = child;

                if (child.Children.IsEmpty())
                {
                    continue;
                }

                SetTagsByFigma(child);
            }
        }

        /// <summary>
        /// If the stroke is too thick relative to the height or width of the object, it overlaps the fill.
        /// In such a case, we do not download the image for this component, and use the stroke color as the fill.
        /// </summary>
        private bool IsOverlappedByStroke(FObject fobject)
        {
            bool blockedByStroke = false;

            if (fobject.HasActiveProperty(x => x.Fills) && fobject.HasActiveProperty(x => x.Strokes) && !fobject.ContainsTag(FcuTag.Shadow))
            {
                if (fobject.IndividualStrokeWeights.IsDefault())
                {
                    float twoSides = fobject.StrokeWeight * 2;

                    if (twoSides >= fobject.Size.y)
                    {
                        blockedByStroke = true;
                    }
                    else if (twoSides >= fobject.Size.x)
                    {
                        blockedByStroke = true;
                    }
                }
                else
                {
                    float topBottomStrokes = fobject.IndividualStrokeWeights.Top + fobject.IndividualStrokeWeights.Bottom;
                    float leftRightStrokes = fobject.IndividualStrokeWeights.Left + fobject.IndividualStrokeWeights.Right;

                    if (topBottomStrokes >= fobject.Size.y)
                    {
                        blockedByStroke = true;
                    }
                    else if (leftRightStrokes >= fobject.Size.x)
                    {
                        blockedByStroke = true;
                    }
                }
            }

            return blockedByStroke;
        }

        /// <summary>
        /// Retrieving the index of an element in the hierarchy, considering the <see cref="FObject.Data.Ignore"/> flag.
        /// </summary>
        private int GetNewIndex(FObject parent, int figmaIndex)
        {
            int count = 0;

            for (int i = 0; i < figmaIndex; i++)
            {
                FObject child = parent.Children[i];

                if (child.Data == null)
                {
                    break;
                }

                if (!child.Data.Ignore)
                {
                    count++;
                }
            }

            return count;
        }
        private void SetSmartTags(FObject parent)
        {
            foreach (FObject fobject in parent.Children)
            {
                fobject.Data.IsEmpty = IsEmpty(fobject);

                if (fobject.Data.IsEmpty)
                {
                    fobject.Data.TagReason = nameof(fobject.Data.IsEmpty);
                    monoBeh.Log($"{nameof(SetSmartTags)} | {fobject.Data.TagReason} | {fobject.Data.NameHierarchy}", FcuLogType.SetTag);
                    continue;
                }

                if (fobject.Data.ForceImage)
                {
                    ///If a component is tagged with the 'img' tag, it will downloaded as a single image,
                    ///which means there is no need to look for child components for it.
                    fobject.Data.TagReason = nameof(fobject.Data.ForceImage);
                    monoBeh.Log($"{nameof(SetSmartTags)} | {fobject.Data.TagReason} | {fobject.Data.NameHierarchy}", FcuLogType.SetTag);
                    continue;
                }

                if (fobject.IsRootSprite(parent))
                {
                    ///If the component is a vector that is at the root of your frame, 
                    ///then we recognize it as a single image and do not look for child components for it, 
                    ///because vectors do not have it.
                    fobject.AddTag(FcuTag.Image);
                    fobject.Data.ForceImage = true;

                    fobject.Data.TagReason = nameof(TagExtensions.IsRootSprite);
                    monoBeh.Log($"{nameof(SetSmartTags)} | {fobject.Data.TagReason} | {fobject.Data.NameHierarchy}", FcuLogType.SetTag);
                    continue;
                }

                if (monoBeh.Settings.MainSettings.RawImport == false)
                {
                    bool hasButtonTags = fobject.ContainsCustomButtonTags();
                    bool hasIcon = ContainsIcon(fobject);
                    bool singleImage = CanBeSingleImage(fobject);

                    if (hasIcon)
                    {
                        fobject.Data.ForceContainer = true;
                        fobject.AddTag(FcuTag.Container);

                        fobject.Data.TagReason = nameof(ContainsIcon);
                        monoBeh.Log($"{nameof(SetSmartTags)} | {fobject.Data.TagReason} | {fobject.Data.NameHierarchy}", FcuLogType.SetTag);
                    }
                    else if (singleImage && hasButtonTags)
                    {
                        fobject.Data.ForceImage = true;
                        fobject.AddTag(FcuTag.Image);
                        fobject.RemoveNotDownloadableTags();

                        fobject.Data.TagReason = nameof(TagExtensions.ContainsCustomButtonTags);
                        monoBeh.Log($"{nameof(SetSmartTags)} | {fobject.Data.TagReason} | {fobject.Data.NameHierarchy}", FcuLogType.SetTag);
                        continue;
                    }
                    else if (singleImage)
                    {
                        ///If the component tree contains only vectors and/or components whose tags
                        ///have flag 'CanBeInsideSingleImage == false', recognize that component as a single image.
                        fobject.Data.ForceImage = true;
                        fobject.AddTag(FcuTag.Image);
                        fobject.RemoveNotDownloadableTags();

                        fobject.Data.TagReason = "SingleImage";
                        monoBeh.Log($"{nameof(SetSmartTags)} | {fobject.Data.TagReason} | {fobject.Data.NameHierarchy}", FcuLogType.SetTag);
                        continue;
                    }
                    else if (fobject.Type == NodeType.BOOLEAN_OPERATION)
                    {
                        fobject.Data.ForceImage = true;
                        fobject.AddTag(FcuTag.Image);

                        fobject.Data.TagReason = "BOOLEAN_OPERATION";
                        continue;
                    }
                }

                if (fobject.HasActiveProperty(x => x.Children))
                {
                    fobject.Data.TagReason = "children not empty";
                    monoBeh.Log($"{nameof(SetSmartTags)} | {fobject.Data.TagReason} | {fobject.Data.NameHierarchy}", FcuLogType.SetTag);
                    fobject.AddTag(FcuTag.Container);
                }

                if (!fobject.HasActiveProperty(x => x.Children))
                    continue;

                SetSmartTags(fobject);
            }
        }

        private bool GetManualTag(FObject fobject, out FcuTag manualTag)
        {
            if (fobject.Name.Contains(FcuConfig.Instance.RealTagSeparator) == false)
            {
                manualTag = FcuTag.None;
                return false;
            }

            IEnumerable<FcuTag> fcuTags = Enum.GetValues(typeof(FcuTag))
               .Cast<FcuTag>()
               .Where(x => x != FcuTag.None);

            foreach (FcuTag fcuTag in fcuTags)
            {
                bool tagFind = FindManualTag(fobject.Data.ObjectName, fcuTag);

                if (tagFind)
                {
                    manualTag = fcuTag;
                    return true;
                }
            }

            manualTag = FcuTag.None;
            return false;
        }

        private bool FindManualTag(string name, FcuTag fcuTag)
        {
            string figmaTag = fcuTag.GetTagConfig().FigmaTag.ToLower();

            if (figmaTag.IsEmpty())
                return false;

            string tempName = name.ToLower().Replace(" ", "");

            string[] nameParts = tempName.Split(FcuConfig.Instance.RealTagSeparator);

            if (nameParts.Length > 0)
            {
                string tagPart = nameParts[0];
                string cleaned = Regex.Replace(tagPart, "[^a-z]", "");

                if (cleaned == figmaTag)
                {
                    monoBeh.Log($"CheckForTag | GetFigmaType | {name} | tag: {figmaTag}", FcuLogType.SetTag);
                    return true;
                }
            }

            return false;
        }

        private bool ContainsIcon(FObject fobject)
        {
            if (fobject.Children.IsEmpty())
                return false;

            foreach (FObject item in fobject.Children)
            {
                if (item.Name.ToLower().Contains("icon"))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanBeSingleImage(FObject fobject)
        {
            if (fobject.Children.IsEmpty())
                return false;

            int count = 0;

            CanBeSingleImageRecursive(fobject, ref count);
            return count == 0;
        }

        private void CanBeSingleImageRecursive(FObject fobject, ref int count)
        {
            if (CanBeInsideSingleImage(fobject) == false)
            {
                count++;
                return;
            }

            if (fobject.Children.IsEmpty())
                return;

            foreach (FObject child in fobject.Children)
                CanBeSingleImageRecursive(child, ref count);
        }

        private bool CanBeInsideSingleImage(FObject fobject)
        {
            if (fobject.Data.ForceContainer)
                return false;

            if (fobject.Data.ForceImage)
                return false;

            foreach (FcuTag fcuTag in fobject.Data.Tags)
            {
                TagConfig tc = fcuTag.GetTagConfig();

                if (tc.CanBeInsideSingleImage == false)
                    return false;
            }

            return true;
        }

        public bool IsEmpty(FObject fobject)
        {
            int count = 0;
            IsEmptyRecursive(fobject, ref count);
            return count == 0;
        }

        public void IsEmptyRecursive(FObject fobject, ref int count)
        {
            if (count > 0)
                return;

            if (fobject.Opacity == 0)
                return;

            if (fobject.IsZeroSize() && fobject.Type != NodeType.LINE)
                return;

            else if (fobject.IsVisible() == false)
                return;
            else if (fobject.Fills.IsEmpty() &&
                fobject.Strokes.IsEmpty() &&
                fobject.Effects.IsEmpty())
            {

            }
            else
            {
                count++;
                return;
            }

            if (fobject.Children.IsEmpty())
                return;

            foreach (var item in fobject.Children)
                IsEmptyRecursive(item, ref count);
        }

        public void CountTags(List<FObject> fobjects)
        {
            ConcurrentDictionary<FcuTag, ConcurrentBag<bool>> tagsCounter = new ConcurrentDictionary<FcuTag, ConcurrentBag<bool>>();

            Array fcuTags = Enum.GetValues(typeof(FcuTag));

            foreach (FcuTag tag in fcuTags)
            {
                tagsCounter.TryAdd(tag, new ConcurrentBag<bool>());
            }

            Parallel.ForEach(fobjects, fobject =>
            {
                if (fobject.Data.GameObject == null)
                {
                    return;
                }

                foreach (FcuTag tag in fobject.Data.Tags)
                {
                    tagsCounter[tag].Add(true);
                }
            });

            Dictionary<FcuTag, int> dictionary = tagsCounter.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Count
            );

            this.TagsCounter = dictionary;
        }
    }
}