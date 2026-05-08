using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using ai;
namespace cultivation
{
    public static class DamageSystemOverride
    {
        private const string HARMONY_ID = "xianni.damage.override";
        private static Harmony _harmony;
        private static bool _initialized = false;
        private static readonly string[] XianniOwnerKeywords = new[]
        {
            "xianni",
            "xn",
            "cultivation",
            "xn.worldbox.mod.cultivation",
            HARMONY_ID
        };
        private static readonly List<(Type type, string methodName, Type[] parameters)> TargetMethods = new List<(Type, string, Type[])>
        {
            (typeof(Actor), "getHit", new Type[] {
                typeof(float), typeof(bool), typeof(AttackType),
                typeof(BaseSimObject), typeof(bool), typeof(bool), typeof(bool)
            }),
            (typeof(BaseSimObject), "changeHealth", new Type[] { typeof(int) }),
            (typeof(Actor), "addForce", new Type[] { typeof(float), typeof(float), typeof(float), typeof(bool), typeof(bool) }),
            (typeof(ActorTool), "applyForceToUnit", new Type[] {
                typeof(AttackData), typeof(BaseSimObject), typeof(float), typeof(bool)
            }),
        };
        public static void Init(Harmony existingHarmony)
        {
            if (_initialized) return;
            _initialized = true;
            if (!xn.config.ModConfigHooks.EnableXianniLaw)
            {
                UnityEngine.Debug.Log("[XN-DamageOverride] Xian Ni law disabled");
                return;
            }
            UnityEngine.Debug.Log("[XN-DamageOverride] Xian Ni law enabled");
            _harmony = new Harmony(HARMONY_ID);
            foreach (var (type, methodName, parameters) in TargetMethods)
            {
                try
                {
                    var method = AccessTools.Method(type, methodName, parameters);
                    if (method != null)
                    {
                        UnpatchOthers(method);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[XN-DamageOverride] Error patching {type.Name}.{methodName}: {ex.Message}");
                }
            }
            try
            {
                ReapplyXianniPatches(existingHarmony);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[XN-DamageOverride] Re-apply patch failed: {ex}");
            }
        }
        private static void ReapplyXianniPatches(Harmony h)
        {
            var getHitMethod = AccessTools.Method(typeof(Actor), "getHit", new Type[] {
                typeof(float), typeof(bool), typeof(AttackType),
                typeof(BaseSimObject), typeof(bool), typeof(bool), typeof(bool)
            });
            if (getHitMethod != null)
            {
                var patches = Harmony.GetPatchInfo(getHitMethod);
                bool hasXianniPatch = patches?.Prefixes?.Any(p => IsXianniPatch(p.owner)) ?? false;
                if (!hasXianniPatch)
                {
                    var prefixMethod = AccessTools.Method(
                        typeof(StatsCombatPatches).GetNestedType("GetHitPatch", System.Reflection.BindingFlags.NonPublic),
                        "Prefix"
                    );
                    if (prefixMethod != null)
                    {
                        h.Patch(getHitMethod, prefix: new HarmonyMethod(prefixMethod) { priority = Priority.First });
                    }
                }
            }
            var changeHealthMethod = AccessTools.Method(typeof(BaseSimObject), "changeHealth", new Type[] { typeof(int) });
            if (changeHealthMethod != null)
            {
                var patches = Harmony.GetPatchInfo(changeHealthMethod);
                bool hasXianniPatch = patches?.Prefixes?.Any(p => IsXianniPatch(p.owner)) ?? false;
                if (!hasXianniPatch)
                {
                    var prefixMethod = AccessTools.Method(
                        typeof(StatsCombatPatches).GetNestedType("ChangeHealthRealmCheckPatch", System.Reflection.BindingFlags.NonPublic),
                        "Prefix"
                    );
                    if (prefixMethod != null)
                    {
                        h.Patch(changeHealthMethod, prefix: new HarmonyMethod(prefixMethod) { priority = Priority.First });
                    }
                }
            }
        }
        private static void UnpatchOthers(System.Reflection.MethodBase method)
        {
            var patches = Harmony.GetPatchInfo(method);
            if (patches == null) return;
            if (patches.Prefixes != null)
            {
                foreach (var prefix in patches.Prefixes.ToList())
                {
                    if (!IsXianniPatch(prefix.owner))
                    {
                        try { _harmony.Unpatch(method, prefix.PatchMethod); }
                        catch { }
                    }
                }
            }
            if (patches.Postfixes != null)
            {
                foreach (var postfix in patches.Postfixes.ToList())
                {
                    if (!IsXianniPatch(postfix.owner))
                    {
                        try { _harmony.Unpatch(method, postfix.PatchMethod); }
                        catch { }
                    }
                }
            }
            if (patches.Transpilers != null)
            {
                foreach (var transpiler in patches.Transpilers.ToList())
                {
                    if (!IsXianniPatch(transpiler.owner))
                    {
                        try { _harmony.Unpatch(method, transpiler.PatchMethod); }
                        catch { }
                    }
                }
            }
            if (patches.Finalizers != null)
            {
                foreach (var finalizer in patches.Finalizers.ToList())
                {
                    if (!IsXianniPatch(finalizer.owner))
                    {
                        try { _harmony.Unpatch(method, finalizer.PatchMethod); }
                        catch { }
                    }
                }
            }
        }
        private static bool IsXianniPatch(string owner)
        {
            if (string.IsNullOrEmpty(owner)) return false;
            string lowerOwner = owner.ToLowerInvariant();
            foreach (var keyword in XianniOwnerKeywords)
            {
                if (lowerOwner.Contains(keyword.ToLowerInvariant()))
                    return true;
            }
            return false;
        }
    }
}
