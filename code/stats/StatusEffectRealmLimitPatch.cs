using HarmonyLib;
using System.Diagnostics;
using UnityEngine;
namespace cultivation
{
    internal static class StatusEffectRealmLimitPatch
    {
        public static void Init(Harmony h)
        {
            h.Patch(
                AccessTools.Method(typeof(StatusLibrary), "burningEffect"),
                prefix: new HarmonyMethod(typeof(StatusEffectRealmLimitPatch), nameof(Prefix_BurningEffect))
            );
            h.Patch(
                AccessTools.Method(typeof(StatusLibrary), "ashFeverEffect"),
                prefix: new HarmonyMethod(typeof(StatusEffectRealmLimitPatch), nameof(Prefix_AshFeverEffect))
            );
            h.Patch(
                AccessTools.Method(typeof(StatusLibrary), "poisonedEffect"),
                prefix: new HarmonyMethod(typeof(StatusEffectRealmLimitPatch), nameof(Prefix_PoisonedEffect))
            );
            h.Patch(
                AccessTools.Method(typeof(ActionLibrary), "addFrozenEffectOnTarget"),
                prefix: new HarmonyMethod(typeof(StatusEffectRealmLimitPatch), nameof(Prefix_AddFrozenEffect))
            );
            h.Patch(
                AccessTools.Method(typeof(ActionLibrary), "addStunnedEffectOnTarget"),
                prefix: new HarmonyMethod(typeof(StatusEffectRealmLimitPatch), nameof(Prefix_AddStunnedEffect))
            );
            h.Patch(
                AccessTools.Method(typeof(BaseSimObject), "addStatusEffect", new System.Type[] { typeof(string), typeof(float), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(StatusEffectRealmLimitPatch), nameof(Prefix_AddStatusEffect))
            );
        }
        private static bool ShouldBlockPercentDamage(BaseSimObject target, out Actor attacker)
        {
            attacker = null;
            if (!xn.access.BaseSimObjectAccess.IsActor(target))
            {
                return false;
            }
            Actor targetActor = xn.access.BaseSimObjectAccess.GetActor(target);
            if (xn.access.ActorAccess.GetAttackedBy(targetActor) != null && xn.access.BaseSimObjectAccess.IsActor(xn.access.ActorAccess.GetAttackedBy(targetActor)))
            {
                attacker = xn.access.BaseSimObjectAccess.GetActor(xn.access.ActorAccess.GetAttackedBy(targetActor));
            }
            else if (xn.access.ActorAccess.HasAttackTarget(targetActor) && xn.access.ActorAccess.GetAttackTarget(targetActor) != null && xn.access.BaseSimObjectAccess.IsActor(xn.access.ActorAccess.GetAttackTarget(targetActor)))
            {
                return false;
            }
            else
            {
                return false;
            }
            if (attacker != null)
            {
                int attackerRealm = GetUnifiedRealmIndex(attacker);
                int targetRealm = GetUnifiedRealmIndex(targetActor);
                if (targetRealm >= 0 && (attackerRealm < 0 || attackerRealm < targetRealm))
                {
                    return true; 
                }
            }
            return false; 
        }
        private static bool ShouldBlockFireDamage(BaseSimObject target, out Actor attacker)
        {
            attacker = null;
            if (!xn.access.BaseSimObjectAccess.IsActor(target))
            {
                return false;
            }
            Actor targetActor = xn.access.BaseSimObjectAccess.GetActor(target);
            return targetActor.isImmuneToFire();
        }
        private static bool ShouldBlockControlEffect(BaseSimObject target, BaseSimObject caster)
        {
            if (!xn.access.BaseSimObjectAccess.IsActor(target))
            {
                return false;
            }
            Actor targetActor = xn.access.BaseSimObjectAccess.GetActor(target);
            int targetRealm = GetUnifiedRealmIndex(targetActor);
            if (targetRealm < 0)
            {
                return false;
            }
            Actor casterActor = null;
            if (caster != null && xn.access.BaseSimObjectAccess.IsActor(caster))
            {
                casterActor = xn.access.BaseSimObjectAccess.GetActor(caster);
            }
            else if (xn.access.ActorAccess.GetAttackedBy(targetActor) != null && xn.access.BaseSimObjectAccess.IsActor(xn.access.ActorAccess.GetAttackedBy(targetActor)))
            {
                casterActor = xn.access.BaseSimObjectAccess.GetActor(xn.access.ActorAccess.GetAttackedBy(targetActor));
            }
            if (casterActor == null)
            {
                return false;
            }
            int casterRealm = GetUnifiedRealmIndex(casterActor);
            if (casterRealm < 0 || casterRealm < targetRealm)
            {
                return true; 
            }
            return false; 
        }
        private static bool IsModCall()
        {
            var stackTrace = new StackTrace(skipFrames: 2, fNeedFileInfo: false);
            var frames = stackTrace.GetFrames();
            if (frames == null) return false;
            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                if (method == null) continue;
                var declaringType = method.DeclaringType;
                if (declaringType == null) continue;
                string ns = declaringType.Namespace;
                if (ns != null && (ns.StartsWith("xn.world") || ns.StartsWith("cultivation")))
                {
                    return true; 
                }
            }
            return false; 
        }
        private static int GetUnifiedRealmIndex(Actor a)
        {
            if (a == null) return -1;
            string[] REALM_IDS = new string[]
            {
                "realm_01_qi", "realm_02_foundation", "realm_03_core", "realm_04_nascent",
                "realm_05_deity", "realm_06_infantchg", "realm_07_wending", "realm_08_kuinie",
                "realm_09_jingnie", "realm_10_suinie", "realm_11_kongnie", "realm_12_kongling",
                "realm_13_kongxuan", "realm_14_gtianzun", "realm_15_half_tatian", "realm_16_tatian"
            };
            int realmIdx = -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
            {
                if (a.hasTrait(REALM_IDS[i]))
                {
                    realmIdx = i;
                }
            }
            if (realmIdx >= 0) return realmIdx;
            string[] ANCIENT_IDS = new string[]
            {
                "ancient_01_star", "ancient_02_star", "ancient_03_star", "ancient_04_star", "ancient_05_star",
                "ancient_06_star", "ancient_07_star", "ancient_08_star", "ancient_09_star", "ancient_10_star"
            };
            int ancIdx = -1;
            for (int i = 0; i < ANCIENT_IDS.Length; i++)
            {
                if (a.hasTrait(ANCIENT_IDS[i]))
                {
                    ancIdx = i;
                }
            }
            if (ancIdx >= 0)
            {
                int star = ancIdx + 1; 
                return ConvertAncientBeastToRealmIndex(star);
            }
            string[] BEAST_IDS = new string[]
            {
                "beast_01_stage", "beast_02_stage", "beast_03_stage", "beast_04_stage", "beast_05_stage",
                "beast_06_stage", "beast_07_stage", "beast_08_stage", "beast_09_stage", "beast_10_stage"
            };
            int beastIdx = -1;
            for (int i = 0; i < BEAST_IDS.Length; i++)
            {
                if (a.hasTrait(BEAST_IDS[i]))
                {
                    beastIdx = i;
                }
            }
            if (beastIdx >= 0)
            {
                int stage = beastIdx + 1; 
                return ConvertAncientBeastToRealmIndex(stage);
            }
            return -1; 
        }
        private static int ConvertAncientBeastToRealmIndex(int starOrStage)
        {
            switch (starOrStage)
            {
                case 1: return 2;   
                case 2: return 4;   
                case 3: return 6;   
                case 4: return 7;   
                case 5: return 8;   
                case 6: return 9;   
                case 7: return 10;  
                case 8: return 11;  
                case 9: return 13;  
                case 10: return 14; 
                default: return -1;
            }
        }
        private static bool Prefix_BurningEffect(BaseSimObject pTarget, WorldTile pTile, ref bool __result)
        {
            if (ShouldBlockFireDamage(pTarget, out Actor attacker))
            {
                if (xn.access.BaseSimObjectAccess.IsActor(pTarget) && xn.access.BaseSimObjectAccess.GetActor(pTarget).asset.has_skin && Randy.randomBool())
                {
                    xn.access.BaseSimObjectAccess.GetActor(pTarget).addInjuryTrait("skin_burns");
                }
                __result = true;
                return false; 
            }
            return true; 
        }
        private static bool Prefix_AshFeverEffect(BaseSimObject pTarget, WorldTile pTile, ref bool __result)
        {
            if (ShouldBlockPercentDamage(pTarget, out Actor attacker))
            {
                __result = true;
                return false; 
            }
            return true; 
        }
        private static bool Prefix_PoisonedEffect(BaseSimObject pTarget, WorldTile pTile, ref bool __result)
        {
            if (ShouldBlockPercentDamage(pTarget, out Actor attacker))
            {
                xn.access.BaseSimObjectAccess.GetActor(pTarget).spawnParticle(Toolbox.color_infected);
                xn.access.BaseSimObjectAccess.GetActor(pTarget).startShake(0.4f, 0.2f, pHorizontal: true, pVertical: false);
                __result = true;
                return false; 
            }
            return true; 
        }
        private static bool Prefix_AddFrozenEffect(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile, ref bool __result)
        {
            if (ShouldBlockControlEffect(pTarget, pSelf))
            {
                __result = false;
                return false; 
            }
            return true; 
        }
        private static bool Prefix_AddStunnedEffect(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile, ref bool __result)
        {
            if (ShouldBlockControlEffect(pTarget, pSelf))
            {
                __result = false;
                return false; 
            }
            return true; 
        }
        private static bool Prefix_AddStatusEffect(BaseSimObject __instance, string pID, float pOverrideTimer, bool pColorEffect, ref bool __result)
        {
            if (pID != "frozen" && pID != "stunned")
            {
                return true; 
            }
            if (IsModCall())
            {
                return true; 
            }
            if (ShouldBlockControlEffect(__instance, null))
            {
                __result = false;
                return false; 
            }
            return true; 
        }
    }
}
