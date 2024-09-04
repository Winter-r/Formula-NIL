using DA_Assets.Shared;
using System;
using UnityEngine;

namespace DA_Assets.FCU.Model
{
    [Serializable]
    public class ScriptGeneratorSettings : MonoBehaviourBinder<FigmaConverterUnity>
    {
        [SerializeField] bool _isEnabled = false;
        public bool IsEnabled { get => _isEnabled; set => SetValue(ref _isEnabled, value); }

        [SerializeField] string _namespace = "MyNameSpace";
        public string Namespace { get => _namespace; set => SetValue(ref _namespace, value); }

        [SerializeField] string _outputPath = "Assets/GeneratedScripts";
        public string OutputPath { get => _outputPath; set => SetValue(ref _outputPath, value); }
    }
}
