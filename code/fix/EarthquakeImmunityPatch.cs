using HarmonyLib;
using UnityEngine;
namespace xn.fix
{
    [HarmonyPatch(typeof(Earthquake), "unitAction")]
    internal static class EarthquakeImmunityPatch
    {
        private const string REALM_PREFIX = "realm_";
        private const string ANCIENT_PREFIX = "ancient_";
        private const string BEAST_PREFIX = "beast_";
        private const string PATH_BEAST = "path_03_beast";
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor)
        {
            if (pActor == null || !pActor.isAlive()) return true;
            if (IsCultivator(pActor))
            {
                xn.access.ActorAccess.ApplyRandomForce(pActor);
                return false; 
            }
            return true; 
        }
        internal static bool IsCultivator(Actor a)
        {
            if (a == null) return false;
            var traits = a.getTraits();
            if (traits == null) return false;
            foreach (var t in traits)
            {
                if (t == null || t.id == null) continue;
                if (t.id.StartsWith(REALM_PREFIX)) return true;
                if (t.id.StartsWith(ANCIENT_PREFIX)) return true;
                if (t.id.StartsWith(BEAST_PREFIX) || t.id == PATH_BEAST) return true;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(MapBox), "applyForceOnTile")]
    internal static class ExplosionStunImmunityPatch
    {
        public static void ConditionalStun(Actor actor, float pTime)
        {
            if (!EarthquakeImmunityPatch.IsCultivator(actor))
                actor.makeStunned(pTime);
        }
        [HarmonyTranspiler]
        private static System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction> Transpiler(
            System.Collections.Generic.IEnumerable<HarmonyLib.CodeInstruction> instructions)
        {
            var original = AccessTools.Method(typeof(Actor), "makeStunned", new[] { typeof(float) });
            var replacement = AccessTools.Method(typeof(ExplosionStunImmunityPatch), "ConditionalStun",
                new[] { typeof(Actor), typeof(float) });
            foreach (var inst in instructions)
            {
                if (inst.Calls(original))
                {
                    yield return new HarmonyLib.CodeInstruction(
                        System.Reflection.Emit.OpCodes.Call, replacement);
                }
                else
                {
                    yield return inst;
                }
            }
        }
    }
}
