using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace DA_Assets.FCU.Model
{
    [Serializable]
    public class SyncData : IHaveId
    {
        [SerializeField] string id;
        public string Id { get => id; set => id = value; }

        [SerializeField] string fileName;
        public string FileName { get => fileName; set => fileName = value; }

        [SerializeField] string objectName;
        public string ObjectName { get => objectName; set => objectName = value; }

        [SerializeField] string uitkGuid;
        public string UitkGuid { get => uitkGuid; set => uitkGuid = value; }

        [SerializeField] string fieldName;
        public string FieldName { get => fieldName; set => fieldName = value; }

        [SerializeField] string methodName;
        public string MethodName { get => methodName; set => methodName = value; }

        [SerializeField] string className;
        public string ClassName { get => className; set => className = value; }

        [SerializeField] string ussClassName;
        public string UssClassName { get => ussClassName; set => ussClassName = value; }

        [Space]

        [SerializeField] List<FcuTag> tags = new List<FcuTag>();
        public List<FcuTag> Tags { get => tags; set => tags = value; }

        [SerializeField] List<int> childIndexes = new List<int>();
        public List<int> ChildIndexes { get => childIndexes; set => childIndexes = value; }

        [SerializeField] FigmaConverterUnity fcuInstance;
        public FigmaConverterUnity FigmaConverterUnity { get => fcuInstance; set => fcuInstance = value; }

        [SerializeField] GameObject gameObject;
        public GameObject GameObject { get => gameObject; set => gameObject = value; }

        [SerializeField] GameObject rootFrameGO;
        [SerializeField] SyncData rootFrameSD;

        private XmlElement xmlElement;
        public XmlElement XmlElement { get => xmlElement; set => xmlElement = value; }

        [Space]

        [SerializeField] FObject parent;
        public FObject Parent { get => parent; set => parent = value; }

        [SerializeField] Color singleColor;
        public Color SingleColor { get => singleColor; set => singleColor = value; }

        [SerializeField] Vector2Int spriteSize;
        public Vector2Int SpriteSize { get => spriteSize; set => spriteSize = value; }

        [SerializeField] Vector2 size;
        public Vector2 Size { get => size; set => size = value; }

        [SerializeField] Vector2 position;
        public Vector2 Position { get => position; set => position = value; }

        [Space]

        [SerializeField] FcuImageType fcuImageType;
        public FcuImageType FcuImageType { get => fcuImageType; set => fcuImageType = value; }

        [SerializeField] ButtonComponent buttonComponent;
        public ButtonComponent ButtonComponent { get => buttonComponent; set => buttonComponent = value; }

        [Space]

        [SerializeField] int hash;
        public int Hash { get => hash; set => hash = value; }

        [SerializeField] int parentIndex;
        public int ParentIndex { get => parentIndex; set => parentIndex = value; }

        [SerializeField] List<FcuHierarchy> hierarchy = new List<FcuHierarchy>();
        public List<FcuHierarchy> Hierarchy { get => hierarchy; set => hierarchy = value; }

#pragma warning disable IDE0052
        [SerializeField] string nameHierarchy;
#pragma warning restore IDE0052
        public void DisplayNameHierarchyInField()
        {
            nameHierarchy = this.NameHierarchy;
        }

        public string NameHierarchy
        {
            get
            {
                if (hierarchy.IsEmpty())
                    return null;

                string h = string.Join(FcuConfig.HierarchyDelimiter.ToString(), hierarchy.Select(x => x.Name));
                return h;
            }
        }

        [SerializeField] string spritePath;
        public string SpritePath { get => spritePath; set => spritePath = value; }

        [SerializeField] string link;
        public string Link { get => link; set => link = value; }

        [SerializeField] string humanizedTextPrefabName;
        public string HumanizedTextPrefabName { get => humanizedTextPrefabName; set => humanizedTextPrefabName = value; }

        [SerializeField] string tagReason;
        public string TagReason { get => tagReason; set => tagReason = value; }

        [SerializeField] string downloadableReason;
        public string DownloadableReason { get => downloadableReason; set => downloadableReason = value; }

        [SerializeField] string generativeReason;
        public string GenerativeReason { get => generativeReason; set => generativeReason = value; }

        [SerializeField] int downloadAttempsCount;
        public int DownloadAttempsCount { get => downloadAttempsCount; set => downloadAttempsCount = value; }

        [SerializeField] float angle;
        public float Angle { get => angle; set => angle = value; }

        [Space]

        [SerializeField] bool isDuplicate;
        public bool IsDuplicate { get => isDuplicate; set => isDuplicate = value; }

        [SerializeField] bool isMutual;
        public bool IsMutual { get => isMutual; set => isMutual = value; }


        [SerializeField] bool isEmpty;
        public bool IsEmpty { get => isEmpty; set => isEmpty = value; }

        [Space]

        [SerializeField] bool needDownload;
        public bool NeedDownload { get => needDownload; set => needDownload = value; }

        [SerializeField] bool needGenerate;
        public bool NeedGenerate { get => needGenerate; set => needGenerate = value; }

        [SerializeField] bool forceImage;
        public bool ForceImage { get => forceImage; set => forceImage = value; }

        [SerializeField] bool isOverlappedByStroke;
        public bool IsOverlappedByStroke { get => isOverlappedByStroke; set => isOverlappedByStroke = value; }

        [SerializeField] bool forceContainer;
        public bool ForceContainer { get => forceContainer; set => forceContainer = value; }

        [SerializeField] bool isInsideDownloadable;
        public bool InsideDownloadable { get => isInsideDownloadable; set => isInsideDownloadable = value; }

        [SerializeField] bool ignore;
        public bool Ignore { get => ignore; set => ignore = value; }

        [SerializeField] bool hasFontAsset;
        public bool HasFontAsset { get => hasFontAsset; set => hasFontAsset = value; }

        public SyncData RootFrame
        {
            get
            {
                if (rootFrameGO == null)
                {
                    return rootFrameSD;
                }

                SyncHelper sh = rootFrameGO.GetComponent<SyncHelper>();

                if (sh == null || sh.Data == null)
                {
                    return rootFrameSD;
                }
                else
                {
                    return sh.Data;
                }
            }
            set
            {
                if (value?.GameObject != null)
                {
                    rootFrameGO = value.GameObject;
                }

                if (rootFrameGO == null)
                {
                    rootFrameSD = value;
                    return;
                }

                SyncHelper sh = rootFrameGO.GetComponent<SyncHelper>();

                if (sh != null)
                {
                    sh.Data = value;
                }
                else
                {
                    rootFrameSD = value;
                }
            }
        }

        [SerializeField] UguiTransformData uguiTransformData;
        public UguiTransformData UguiTransformData { get => uguiTransformData; set => uguiTransformData = value; }
#if NOVA_UI_EXISTS

        [SerializeField] NovaTransformData novaTransformData;
        public NovaTransformData NovaTransformData { get => novaTransformData; set => novaTransformData = value; }
#endif

        public string UitkType { get; set; }
        public string HashData { get; set; }
        public string HashDataTree { get; set; }
#if UNITY_2021_3_OR_NEWER
        public UnityEngine.UIElements.UIDocument UIDocument { get; internal set; }
#endif
        [SerializeField] string uxmlPath;
        public string UxmlPath { get => uxmlPath; internal set => uxmlPath = value; }
        public Vector2 TopLeft { get; set; }

    }

    [Serializable]
    public struct FcuHierarchy
    {
        public int Index;
        public string Name;
        public string Guid;
    }
}