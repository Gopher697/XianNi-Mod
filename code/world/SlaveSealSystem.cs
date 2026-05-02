using HarmonyLib;
using UnityEngine;
using System;
namespace xn.world
{
    internal static class SlaveSealSystem
    {
        const string KEY_MASTER_ID      = "xn_slave_master_id";  
        const string KEY_SLAVE_ID       = "xn_slave_id";         
        const string KEY_EXPIRE_YEAR    = "xn_slave_expire_year";
        const string KEY_LAST_XP        = "xn_slave_last_xp";    
        const string KEY_UI_SCALE       = "xn_tyz_scale_pct";    
        const string KEY_SAVE_ONCE = "xn_slave_save_once_master_id"; 
        static readonly string[] REALM_IDS = new[]{
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        const string KEY_XP = "xn.stat.xiuwei";
        const float  LOW_HP_RATIO        = 0.20f;  
        const int    PROB_DEFEAT_CHECK   = 20;     
        const int    PROB_SEAL_SUCCESS   = 50;     
        const int    FOLLOW_RANGE_TILES  = 20;     
        const float  TICK_SEC            = 1f;     
        const float  RUSH_RECHECK_SEC    = 3f;     
        static double s_nextTick = 0;
        static double s_nextRushReset = 0;
        static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
        internal static void TrySeal(Actor caster, Actor target)
        {
            if (xn.world.SpaceRiftSystem.IsSpaceActiveFor(caster) || xn.world.SpaceRiftSystem.IsSpaceActiveFor(target)) return;
            if (xn.tournament.TournamentManager.IsRunning &&
                (xn.tournament.TournamentManager.IsParticipant(caster) || xn.tournament.TournamentManager.IsParticipant(target)))
                return;
            if (caster == null || target == null) return;
            if (!caster.isAlive()) return;
            if (caster == target) return;
            long hasSlave; xn.access.ActorAccess.GetData(caster).get(KEY_SLAVE_ID, out hasSlave, 0L);
            if (hasSlave > 0) return;
            long casterMaster; xn.access.ActorAccess.GetData(caster).get(KEY_MASTER_ID, out casterMaster, 0L);
            if (casterMaster > 0) return;
            long targetMaster; xn.access.ActorAccess.GetData(target).get(KEY_MASTER_ID, out targetMaster, 0L);
            if (targetMaster > 0) return;
            int cr = GetRealmIndex(caster);
            int tr = GetRealmIndex(target);
            int idxInfant = IndexOf("realm_06_infantchg");
            if (cr < 0 || tr < 0 || idxInfant < 0) return;
            if (cr < idxInfant) return;
            if (cr <= tr + 1) return; 
            if (target.getHealthRatio() >= LOW_HP_RATIO) return;
            if (!Randy.randomChance(PROB_DEFEAT_CHECK)) return;
            if (!Randy.randomChance(PROB_SEAL_SUCCESS)) return;
            int years = (cr - idxInfant + 1) * 5; 
            int expireYear = Date.getCurrentYear() + years;
            xn.access.ActorAccess.GetData(target).set(KEY_MASTER_ID, xn.access.ActorAccess.GetData(caster).id);
            xn.access.ActorAccess.GetData(target).set(KEY_EXPIRE_YEAR, expireYear);
            xn.access.ActorAccess.GetData(caster).set(KEY_SLAVE_ID, xn.access.ActorAccess.GetData(target).id);
            target.cancelAllBeh();
            caster.cancelAllBeh();
            string tName = target.getName() ?? T("common_unknown", "Unknown");
            string cName = caster.getName() ?? T("common_unknown", "Unknown");
            xn.world.BroadcastSystem.Custom(T("broadcast_slave_seal_applied", "{0} was marked with {1}'s slave seal", tName, cName));
            long xp; xn.access.ActorAccess.GetData(target).get(KEY_XP, out xp, 0L);
            xn.access.ActorAccess.GetData(target).set(KEY_LAST_XP, xp);
            xn.access.ActorAccess.GetData(target).set(KEY_SAVE_ONCE, xn.access.ActorAccess.GetData(caster).id); 
        }
        [HarmonyPatch(typeof(MapBox), "updateSimulation")]
        internal static class Patch_Map_Update
        {
            static void Postfix(MapBox __instance)
            {
                double now = World.world.getCurWorldTime();
                if (now >= s_nextTick)
                {
                    s_nextTick = now + TICK_SEC;
                    var list = __instance.units != null ? __instance.units.getSimpleList() : null;
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var slave = list[i];
                            if (slave == null || !slave.isAlive()) continue;
                            long masterId; xn.access.ActorAccess.GetData(slave).get(KEY_MASTER_ID, out masterId, 0L);
                            if (masterId <= 0) continue;
                            var master = World.world.units.get(masterId);
                            if (master == null || master.isRekt())
                            {
                                ClearRelation(slave, null);
                                continue;
                            }
                            int ey; xn.access.ActorAccess.GetData(slave).get(KEY_EXPIRE_YEAR, out ey, 0);
                            if (ey > 0 && Date.getCurrentYear() >= ey)
                            {
                                ClearRelation(slave, master);
                                xn.world.BroadcastSystem.Custom(T("broadcast_slave_seal_expired", "{0}'s slave seal expired", slave.getName() ?? T("common_unknown", "Unknown")));
                                continue;
                            }
                            BaseSimObject slaveAttackTarget = xn.access.ActorAccess.GetAttackTarget(slave);
                            if (xn.access.ActorAccess.HasAttackTarget(slave) && slaveAttackTarget != null && xn.access.BaseSimObjectAccess.IsActor(slaveAttackTarget) && xn.access.BaseSimObjectAccess.GetActor(slaveAttackTarget) == master)
                            {
                                slave.cancelAllBeh();
                            }
                            BaseSimObject masterAttackTarget = xn.access.ActorAccess.GetAttackTarget(master);
                            if (xn.access.ActorAccess.HasAttackTarget(master) && masterAttackTarget != null && xn.access.BaseSimObjectAccess.IsActor(masterAttackTarget) && xn.access.BaseSimObjectAccess.GetActor(masterAttackTarget) == slave)
                            {
                                master.cancelAllBeh();
                            }
                            if (slave.current_tile != null && master.current_tile != null)
                            {
                                if (xn.access.ActorAccess.GetTileTarget(slave) != master.current_tile || slave.current_path == null || slave.current_path.Count == 0)
                                {
                                    ActorMove.goTo(slave, master.current_tile, pPathOnLiquid: false, pWalkOnBlocks: true, pPathOnLava: false, pLimitPathfindingRegions: 0);
                                }
                            }
                            Actor enemy = null;
                            if (masterAttackTarget != null && xn.access.BaseSimObjectAccess.IsActor(masterAttackTarget))
                                enemy = xn.access.BaseSimObjectAccess.GetActor(masterAttackTarget);
                            else if (xn.access.ActorAccess.GetAttackedBy(master) != null && xn.access.BaseSimObjectAccess.IsActor(xn.access.ActorAccess.GetAttackedBy(master)))
                                enemy = xn.access.BaseSimObjectAccess.GetActor(xn.access.ActorAccess.GetAttackedBy(master));
                            if (enemy != null && enemy.isAlive() && enemy != slave && enemy.current_tile != null)
                            {
                                slave.startFightingWith(enemy);
                                ActorMove.goTo(slave, enemy.current_tile, pPathOnLiquid: false, pWalkOnBlocks: true, pPathOnLava: false, pLimitPathfindingRegions: 0);
                            }
                            long cur; xn.access.ActorAccess.GetData(slave).get(KEY_XP, out cur, 0L);
                            long last; xn.access.ActorAccess.GetData(slave).get(KEY_LAST_XP, out last, cur);
                            if (cur > last)
                            {
                                long delta = cur - last;
                                long tribute = (long)(delta * 0.3f);
                                if (tribute > 0)
                                {
                                    xn.access.ActorAccess.GetData(slave).set(KEY_XP, cur - tribute);
                                    long mxp; xn.access.ActorAccess.GetData(master).get(KEY_XP, out mxp, 0L);
                                    xn.access.ActorAccess.GetData(master).set(KEY_XP, mxp + tribute);
                                }
                            }
                            xn.access.ActorAccess.GetData(slave).set(KEY_LAST_XP, cur);
                        }
                    }
                }
                if (now >= s_nextRushReset)
                {
                    s_nextRushReset = now + RUSH_RECHECK_SEC;
                    var list = __instance.units != null ? __instance.units.getSimpleList() : null;
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var s = list[i];
                            if (s == null || !s.isAlive()) continue;
                            long mid; xn.access.ActorAccess.GetData(s).get(KEY_MASTER_ID, out mid, 0L);
                            if (mid <= 0) continue;
                            var m = World.world.units.get(mid);
                            if (m == null || m.isRekt()) continue;
                            Actor enemy = null;
                            BaseSimObject attackTarget = xn.access.ActorAccess.GetAttackTarget(m);
                            BaseSimObject attackedBy = xn.access.ActorAccess.GetAttackedBy(m);
                            if (attackTarget != null && xn.access.BaseSimObjectAccess.IsActor(attackTarget))
                                enemy = xn.access.BaseSimObjectAccess.GetActor(attackTarget);
                            else if (attackedBy != null && xn.access.BaseSimObjectAccess.IsActor(attackedBy))
                                enemy = xn.access.BaseSimObjectAccess.GetActor(attackedBy);
                            if (enemy == null || !enemy.isAlive()) continue;
                            s.cancelAllBeh();
                            s.startFightingWith(enemy);
                            if (enemy.current_tile != null)
                                ActorMove.goTo(s, enemy.current_tile, pPathOnLiquid: false, pWalkOnBlocks: true, pPathOnLava: false, pLimitPathfindingRegions: 0);
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Actor), "die", new Type[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) })]
        internal static class Patch_Actor_Die_SlaveSaveOnce
        {
            private static bool Prefix(Actor __instance, bool pDestroy, AttackType pType, bool pCountDeath, bool pLogFavorite)
            {
                if (__instance == null) return true;
                if (pDestroy) return true; 
                long masterId; xn.access.ActorAccess.GetData(__instance).get(KEY_SAVE_ONCE, out masterId, 0L);
                if (masterId <= 0) return true;
                xn.access.ActorAccess.GetData(__instance).set(KEY_SAVE_ONCE, 0L);
                int healSelf = Mathf.CeilToInt(__instance.getMaxHealth() * 0.3f);
                if (healSelf < 1) healSelf = 1; 
                __instance.changeHealth(healSelf);
                return false;
            }
        }
        [HarmonyPatch(typeof(Actor), "startFightingWith", new Type[] { typeof(BaseSimObject) })]
        internal static class Patch_Actor_startFightingWith_NoMasterSlave
        {
            private static bool Prefix(Actor __instance, BaseSimObject pSimObject)
            {
                if (__instance == null || pSimObject == null) return true;
                if (!xn.access.BaseSimObjectAccess.IsActor(pSimObject)) return true; 
                var pTarget = xn.access.BaseSimObjectAccess.GetActor(pSimObject);
                long aMaster; xn.access.ActorAccess.GetData(__instance).get(KEY_MASTER_ID, out aMaster, 0L);
                long aSlave; xn.access.ActorAccess.GetData(__instance).get(KEY_SLAVE_ID, out aSlave, 0L);
                long bMaster; xn.access.ActorAccess.GetData(pTarget).get(KEY_MASTER_ID, out bMaster, 0L);
                long bSlave; xn.access.ActorAccess.GetData(pTarget).get(KEY_SLAVE_ID, out bSlave, 0L);
                bool paired =
                    (aMaster > 0 && aMaster == xn.access.ActorAccess.GetData(pTarget).id) ||
                    (aSlave > 0 && aSlave == xn.access.ActorAccess.GetData(pTarget).id) ||
                    (bMaster > 0 && bMaster == xn.access.ActorAccess.GetData(__instance).id) ||
                    (bSlave > 0 && bSlave == xn.access.ActorAccess.GetData(__instance).id);
                if (!paired) return true;
                __instance.cancelAllBeh(); 
                return false;              
            }
        }
        [HarmonyPatch(typeof(Actor), "getHit", new Type[] {
            typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject),
            typeof(bool),  typeof(bool), typeof(bool)
        })]
        internal static class Patch_Actor_getHit_Seal
        {
            static void Postfix(
                Actor __instance,
                float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker,
                bool pSkipIfShake, bool pMetallicWeapon, bool pCheckDamageReduction)
            {
                var caster = xn.access.BaseSimObjectAccess.GetActor(pAttacker);
                var target = __instance;
                if (caster == null || target == null) return;
                TrySeal(caster, target);
            }
        }
        private static void ClearRelation(Actor slave, Actor masterOrNull)
        {
            if (slave != null)
            {
                xn.access.ActorAccess.GetData(slave).set(KEY_MASTER_ID, 0L);
                xn.access.ActorAccess.GetData(slave).set(KEY_EXPIRE_YEAR, 0);
                xn.access.ActorAccess.GetData(slave).set(KEY_LAST_XP, 0L);
            }
            var m = masterOrNull;
            if (m == null && slave != null)
            {
                long mid; xn.access.ActorAccess.GetData(slave).get(KEY_MASTER_ID, out mid, 0L);
                if (mid > 0) m = World.world.units.get(mid);
            }
            if (m != null) xn.access.ActorAccess.GetData(m).set(KEY_SLAVE_ID, 0L);
        }
        private static int GetRealmIndex(Actor a)
        {
            if (a == null) return -1;
            int idx = -1;
            var set = a.traits;
            int count = set != null ? set.Count : 0;
            if (count == 0) return -1;
            for (int i = 0; i < REALM_IDS.Length; i++)
            {
                if (a.hasTrait(REALM_IDS[i])) idx = i;
            }
            return idx;
        }
        private static int IndexOf(string id)
        {
            for (int i = 0; i < REALM_IDS.Length; i++)
                if (REALM_IDS[i] == id) return i;
            return -1;
        }
    }
}
