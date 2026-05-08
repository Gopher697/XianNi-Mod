using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace cultivation
{
    public static class DeathSystemOverride
    {
        private const string HARMONY_ID = "xianni.death.override";
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
            (typeof(Actor), "die", new Type[] {
                typeof(bool), typeof(AttackType), typeof(bool), typeof(bool)
            }),
        };
        public static void Init(Harmony existingHarmony)
        {
            if (_initialized) return;
            _initialized = true;
            if (!xn.config.ModConfigHooks.EnableDeathLaw)
            {
                return;
            }
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
                    Debug.LogError($"[XN-DeathOverride] Error patching {type.Name}.{methodName}: {ex.Message}");
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
                        try
                        {
                            _harmony.Unpatch(method, prefix.PatchMethod);
                        }
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
                        try
                        {
                            _harmony.Unpatch(method, postfix.PatchMethod);
                        }
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
                        try
                        {
                            _harmony.Unpatch(method, transpiler.PatchMethod);
                        }
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
                        try
                        {
                            _harmony.Unpatch(method, finalizer.PatchMethod);
                        }
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