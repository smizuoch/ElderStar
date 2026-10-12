using UnityEditor;

namespace Nova.Editor.Core.Scripts
{
    [FilePath("ProjectSettings/NovaBuiltInSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class BuiltInProjectSettings : ScriptableSingleton<BuiltInProjectSettings>
    {
        public bool automaticLightingSetup = true;
        public int initializedColorSpace = -1;

        public void Persist() => Save(true);
    }
}
