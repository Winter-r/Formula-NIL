using DA_Assets.FCU.Extensions;
using DA_Assets.FCU.Model;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DA_Assets.FCU
{
    [Serializable]
    public class NameSetter : MonoBehaviourBinder<FigmaConverterUnity>
    {
        private static Dictionary<string, int> fieldNames = new Dictionary<string, int>();
        private static Dictionary<string, int> methodNames = new Dictionary<string, int>();
        private static Dictionary<string, int> classNames = new Dictionary<string, int>();

        public void SetNames(FObject child)
        {
            child.Data.ObjectName = GetFcuName(child, FcuNameType.Object);
            child.Data.FileName = GetFcuName(child, FcuNameType.File);
            child.Data.FieldName = GetFcuName(child, FcuNameType.Field);
            child.Data.MethodName = GetFcuName(child, FcuNameType.Method);
            child.Data.ClassName = GetFcuName(child, FcuNameType.Class);

            child.Data.UitkGuid = GetFcuName(child, FcuNameType.UitkGuid);
            child.Data.UssClassName = GetFcuName(child, FcuNameType.UssClass);
        }

        public string GetFcuName(FObject fobject, FcuNameType nameType)
        {
            string name = fobject.Name;

            name = RestoreName(name, fobject);
            name = name.RemoveInvalidCharsFromFileName();

            if (!name.IsEmpty() && name.Length > 0 && name[0] == '.')
            {
                name = name.Remove(0, 1);
            }

            switch (nameType)
            {
                case FcuNameType.UitkGuid:
                    {
                        name = Guid.NewGuid().ToString();
                        name = name.Replace("-", "");
                    }
                    break;
                case FcuNameType.Object:
                    {
                        if (monoBeh.IsUGUI())
                        {
                            name = name.RemoveRepeats('-');
                            name = name.RemoveRepeats(' ');
                            name = name.RemoveRepeats('_');
                            name = RestoreName(name, fobject);
                        }
                        else
                        {
                            name = Regex.Replace(name, "[^a-zA-Z0-9_-]", "-");

                            if (char.IsDigit(name[0]))
                            {
                                name = "_" + name;
                            }

                            name = name.RemoveRepeats('-');
                            name = name.RemoveRepeats(' ');
                            name = name.RemoveRepeats('_');
                            name = RestoreName(name, fobject);

                            name = name.Replace(" ", "-");
                            name = name.RemoveRepeats('-');
                        }
                    }
                    break;
                case FcuNameType.File:
                    {
                        name = name.RemoveRepeats('-');
                        name = name.RemoveRepeats(' ');
                        name = name.RemoveRepeats('_');
                        name = RestoreName(name, fobject);
                    }
                    break;
                case FcuNameType.Field:
                case FcuNameType.Method:
                case FcuNameType.Class:
                    {
                        name = GetCsSharpName(name, nameType, fobject);
                    }
                    break;
                case FcuNameType.UssClass:
                    {
                        if (GetManualUssClassName(fobject, out string ussClass))
                        {
                            name = ussClass;    
                        }
                        else
                        {
                            string objectName = GetFcuName(fobject, FcuNameType.Object);
                            name = $"style-{objectName}-{fobject.Id.ReplaceNotNumbers('-')}";
                        }
                    }
                    break;
            }

            if (monoBeh.IsUGUI() && nameType == FcuNameType.Object)
            {
                if (fobject.Type == NodeType.TEXT)
                {
                    name = name.SubstringSafe(FcuConfig.Instance.TextObjectNameLength);
                }
                else
                {
                    name = name.SubstringSafe(FcuConfig.Instance.GameObjectNameLength);
                }
            }

            name = name.Trim();

            return name;
        }

        /// <summary>
        /// For classes, methods, fields.
        /// </summary>
        private string GetCsSharpName(string name, FcuNameType nameType, FObject fobject)
        {
            char prefix = ' ';

            switch (nameType)
            {
                case FcuNameType.Field:
                    prefix = '_';
                    break;
                case FcuNameType.Method:
                    prefix = 'M';
                    break;
                case FcuNameType.Class:
                    prefix = 'C';
                    break;
            }

            name = name.ToPascalCase();
            name = RestoreName(name, fobject);

            if (char.IsDigit(name[0]) || prefix == '_' || OtherExtensions.CsSharpKeywords.Contains(name))
            {
                name = prefix + name;
                name = name.MakeCharUpper(1);
            }

            if (name[0] == '_')
            {
                if (prefix == 'C' || prefix == 'M')
                {
                    name = prefix + name;
                    name = name.MakeCharUpper(2);
                }
                else
                {
                    name = name.MakeCharLower(1);
                }
            }

            int number = 0;

            switch (nameType)
            {
                case FcuNameType.Field:
                    {
                        FindNameInDict(name, ref fieldNames, out number);
                    }
                    break;
                case FcuNameType.Method:
                    {
                        FindNameInDict(name, ref methodNames, out number);
                    }
                    break;
                case FcuNameType.Class:
                    {
                        if (classNames.Count < 1)
                        {
                            int maxNumber = AssetTools.GetMaxFileNumber("Assets", name, "cs");

                            Debug.Log($"FcuNameType.Class: {name} | GetMaxFileNumber: {maxNumber}");

                            if (maxNumber >= 0)
                            {
                                classNames.Add(name, maxNumber);
                            }
                        }

                        FindNameInDict(name, ref classNames, out number);

                        Debug.Log($"FcuNameType.Class: {name} | NewNumber: {number}");
                    }
                    break;
            }

            if (number > 0)
            {
                name = $"{name}_{number}";
            }

            return name;
        }

        private void FindNameInDict(string name, ref Dictionary<string, int> dict, out int number)
        {
            if (dict.TryGetValue(name, out number))
            {
                number++;
                dict[name] = number;
            }
            else
            {
                dict.Add(name, 0);
            }
        }

        public static void ClearNames()
        {
            fieldNames.Clear();
            methodNames.Clear();
            classNames.Clear();
        }

        private string RestoreName(string name, FObject fobject)
        {
            bool containsLatinLetters = !name.IsEmpty() && Regex.Matches(name, @"[a-zA-Z]").Count > 0;

            if (name.IsEmpty() || !containsLatinLetters)
            {
                if (fobject.Data.Parent.IsDefault())
                {
                    name = $"unnamed {FcuConfig.Instance.RealTagSeparator} {fobject.Id.ReplaceNotNumbers('-')}";
                }
                else
                {
                    name = $"unnamed {FcuConfig.Instance.RealTagSeparator} {fobject.Data.Parent.Id.ReplaceNotNumbers('-')} {fobject.Id.ReplaceNotNumbers('-')}";
                }
            }

            return name;
        }

        public bool GetManualUssClassName(FObject fobject, out string className)
        {
            className = "";

            string input = fobject.Name;

            if (string.IsNullOrWhiteSpace(input) || !input.StartsWith(".") || !input.Contains($" {FcuConfig.Instance.RealTagSeparator} "))
            {
                return false;
            }

            int startIndex = 1;
            int endIndex = input.IndexOf($" {FcuConfig.Instance.RealTagSeparator} ");
            if (endIndex == -1)
            {
                return false;
            }

            className = input.Substring(startIndex, endIndex - startIndex);
            return true;
        }
    }
}