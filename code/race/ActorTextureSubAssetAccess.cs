using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.race
{
    internal static class ActorTextureSubAssetAccess
    {
        private static readonly FieldInfo BasePathField = AccessTools.Field(typeof(ActorTextureSubAsset), "_base_path");
        private static readonly MethodInfo LoadShadowMethod = AccessTools.Method(typeof(ActorTextureSubAsset), "loadShadow");

        public static void SetTextureBasePath(ActorTextureSubAsset textureAsset, string basePath)
        {
            if (textureAsset == null) return;

            if (BasePathField == null)
            {
                Debug.LogWarning("[XN] ActorTextureSubAsset._base_path field not found; texture base path was not set.");
                return;
            }

            BasePathField.SetValue(textureAsset, basePath);
        }

        public static void LoadShadow(ActorTextureSubAsset textureAsset)
        {
            if (textureAsset == null) return;

            if (LoadShadowMethod == null)
            {
                Debug.LogWarning("[XN] ActorTextureSubAsset.loadShadow method not found; shadow was not loaded.");
                return;
            }

            LoadShadowMethod.Invoke(textureAsset, null);
        }
    }
}
