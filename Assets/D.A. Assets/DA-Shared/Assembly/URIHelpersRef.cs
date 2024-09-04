using System;
using System.Reflection;
using UnityEngine;

namespace DA_Assets.Shared
{
    public class URIHelpersRef
    {
        private static Type _uriHelpersType;
        private static MethodInfo _makeAssetUriMethod;

        /// <summary>
        /// https://github.com/Unity-Technologies/UnityCsReference/blob/b1c78d185a6f77aee1c1da32db971eaf1006f83e/Editor/Mono/UIElements/StyleSheets/URIHelpers.cs
        /// </summary>
        public static string MakeAssetUri(UnityEngine.Object asset)
        {
            if (asset == null) return null;
            if (_uriHelpersType == null)
            {
                _uriHelpersType = Type.GetType("UnityEditor.UIElements.StyleSheets.URIHelpers, UnityEditor");
            }

            if (_uriHelpersType == null)
            {
                Debug.LogError("Failed to find the type URIHelpers. Ensure that you are using this code within the Unity Editor.");
                return null;
            }

            if (_makeAssetUriMethod == null)
            {
                _makeAssetUriMethod = _uriHelpersType.GetMethod("MakeAssetUri", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(UnityEngine.Object) }, null);
            }

            if (_makeAssetUriMethod == null)
            {
                Debug.LogError("Failed to find the method MakeAssetUri. Check if the method is accessible.");
                return null;
            }

            object result = _makeAssetUriMethod.Invoke(null, new object[] { asset });

            string input = (string)result;

            int index = input.IndexOf("?fileID=");
            if (index >= 0)
                input = input.Substring(0, index);

            return input;
        }
    }
}
