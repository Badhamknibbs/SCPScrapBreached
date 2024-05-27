using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalLevelLoader;
using LobbyCompatibility.Attributes;
using LobbyCompatibility.Enums;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace SCPScrapBreached
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(LethalLevelLoader.Plugin.ModGUID, BepInDependency.DependencyFlags.HardDependency)]
    [LobbyCompatibility(CompatibilityLevel.Everyone, VersionStrictness.Patch)]
    public class SCPScrapBreached : BaseUnityPlugin
    {
        public static SCPScrapBreached Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }

        public static AssetBundle? SCPScrapAssets = null;

        private void Awake() {
            Logger = base.Logger;
            Instance = this;

            NetcodePatcher();

            string AssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            SCPScrapAssets = AssetBundle.LoadFromFile(Path.Combine(AssemblyLocation, "scpscrap"));
            if (SCPScrapAssets == null) {
                Logger.LogError("Failed to load SCP scrap assets.");
                return;
            }

            ExtendedMod? ExtendedMod = SCPScrapAssets.LoadAsset<ExtendedMod>("SCPScrap");
            if (ExtendedMod == null) {
                Logger.LogError("Failed to load extended mod from bundle.");
                return;
            }

            PatchedContent.RegisterExtendedMod(ExtendedMod);

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        private void NetcodePatcher() {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types) {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods) {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0) {
                        method.Invoke(null, null);
                    }
                }
            }
        }
    }
}
