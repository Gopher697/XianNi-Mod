using System;
using HarmonyLib;
namespace cultivation
{
    internal static class ImmortalTraitControl
    {
        private const string IMMORTAL_ID = "immortal";
        [ThreadStatic]
        private static bool s_isEditorAdding;
        [HarmonyPatch(typeof(ActorTraitsEditor), nameof(ActorTraitsEditor.addTrait))]
        private static class EditorPatch
        {
            static void Prefix() => s_isEditorAdding = true;
            static void Finalizer() => s_isEditorAdding = false;
        }
        [HarmonyPatch(typeof(Actor), nameof(Actor.addTrait), new Type[] { typeof(string), typeof(bool) })]
        private static class StringPatch
        {
            static bool Prefix(ref bool __result, string pTraitID)
            {
                if (pTraitID != IMMORTAL_ID || s_isEditorAdding) return true;
                __result = false;
                return false;
            }
        }
        [HarmonyPatch(typeof(Actor), nameof(Actor.addTrait), new Type[] { typeof(ActorTrait), typeof(bool) })]
        private static class AssetPatch
        {
            static bool Prefix(ref bool __result, ActorTrait pTrait)
            {
                if (pTrait?.id != IMMORTAL_ID || s_isEditorAdding) return true;
                __result = false;
                return false;
            }
        }
    }
}