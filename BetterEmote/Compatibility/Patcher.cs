using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Reflection;

namespace BetterEmote.Compatibility
{
    internal class Patcher
    {
        public static void patchCompat(Harmony patcher)
        {
            Plugin.Debug("patchCompat()");
            AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()).Do((type) =>
            {
                try
                {
                    var attribute = (CompatPatchAttribute)Attribute.GetCustomAttribute(type, typeof(CompatPatchAttribute));
                    if (attribute == null)
                        return;
                    Plugin.Debug($"Checking for mod to patch: {attribute.Dependency}");
                    if (hasMod(attribute.Dependency))
                    {
                        Plugin.Debug($"Patching now");
                        patcher.CreateClassProcessor(type).Patch();
                    }
                }
                catch (Exception e)
                {
                    Plugin.StaticLogger.LogError($"Failed to apply patches from {type}: {e.Message}");
                }
            });
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        internal class CompatPatchAttribute(string dependency) : Attribute
        {
            public string Dependency { get; } = dependency;
        }

        public static bool hasMod(string guid)
        {
            Plugin.Debug($"Compatibility.hasMod({guid})");
            foreach (BepInEx.PluginInfo plugin in Chainloader.PluginInfos.Values)
            {
                Plugin.Debug($"Checking against {plugin.Metadata.GUID}");
                if (plugin.Metadata.GUID == guid)
                {
                    Plugin.Debug($"has mod!");
                    return true;
                }
            }
            return false;
        }

    }
}
