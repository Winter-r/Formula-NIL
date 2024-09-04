using DA_Assets.FCU.Extensions;
using DA_Assets.Shared;
using DA_Assets.Shared.Extensions;
using System;

namespace DA_Assets.FCU
{
    [Serializable]
    public class FolderCreator : MonoBehaviourBinder<FigmaConverterUnity>
    {
        public void CreateAll()
        {
            monoBeh.FontLoader.TtfFontsPath.CreateFolderIfNotExists();
            monoBeh.FontLoader.TmpFontsPath.CreateFolderIfNotExists();

            if (monoBeh.IsUITK())
            {
                monoBeh.Settings.MainSettings.UitkOutputPath.CreateFolderIfNotExists();
            }

            monoBeh.Settings.MainSettings.SpritesPath.CreateFolderIfNotExists();

            if (monoBeh.Settings.ScriptGeneratorSettings.IsEnabled)
            {
                monoBeh.Settings.ScriptGeneratorSettings.OutputPath.CreateFolderIfNotExists();
            }

            monoBeh.Settings.PrefabSettings.PrefabsPath.CreateFolderIfNotExists();
        }
    }
}