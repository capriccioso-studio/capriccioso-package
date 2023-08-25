using UnityEngine;

namespace Capriccioso
{
    /// <summary>
    /// Build service, contains the instance and building specifics for the BuildMenu
    /// </summary>
    public class BuildService : MonoSingleton<BuildService>
    {
		[Tooltip("The path where the builds will be stored")]
        public string BuildPath = "Builds";

		[Tooltip("The scenes that will be built")]
        public string[] Scenes = new string[] {"Assets/_Scenes/Main.unity"};

		[Tooltip("The version of the build. Should be MAJOR_MINOR_PATCH")]
        public string Version = "0_0_1";

        [Tooltip("The name of the build")]
        public string Name = "Capriccioso";
    }
}