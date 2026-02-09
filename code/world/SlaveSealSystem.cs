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
        internal static void TrySeal(Actor caster, Actor target)
        {
            if (xn.world.SpaceRiftSystem.IsSpaceActiveFor(caster) || xn.world.SpaceRiftSystem.IsSpaceActiveFor(target)) return;
            if (xn.tournament.TournamentManager.IsRunning &&
                (xn.tournament.TournamentManager.IsParticipant(caster) || xn.tournament.TournamentManager.IsParticipant(target)))
                return;
            if (caster == null || target == null) return;
            if (!caster.isAlive()) return;
            if (caster == target) return;
            long hasSlave; caster.data.get(KEY_SLAVE_ID, out hasSlave, 0L);
            if (hasSlave > 0) return;
            long casterMaster; caster.data.get(KEY_MASTER_ID, out casterMaster, 0L);
            if (casterMaster > 0) return;
            long targetMaster; target.data.get(KEY_MASTER_ID, out targetMaster, 0L);
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
            target.data.set(KEY_MASTER_ID, caster.data.id);
            target.data.set(KEY_EXPIRE_YEAR, expireYear);
            caster.data.set(KEY_SLAVE_ID, target.data.id);
            target.cancelAllBeh();
            caster.cancelAllBeh();
            string tName = target.getName() ?? "未知";
            string cName = caster.getName() ?? "未知";
            xn.world.BroadcastSystem.Custom($"{tName}被{cName}种下了奴印");
            long xp; target.data.get(KEY_XP, out xp, 0L);
            target.data.set(KEY_LAST_XP, xp);
            target.data.set(KEY_SAVE_ONCE, caster.data.id); 
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
                            long masterId; slave.data.get(KEY_MASTER_ID, out masterId, 0L);
                            if (masterId <= 0) continue;
                            var master = World.world.units.get(masterId);
                            if (master == null || master.isRekt())
                            {
                                ClearRelation(slave, null);
                                continue;
                            }
                            int ey; slave.data.get(KEY_EXPIRE_YEAR, out ey, 0);
                            if (ey > 0 && Date.getCurrentYear() >= ey)
                            {
                                ClearRelation(slave, master);
                                xn.world.BroadcastSystem.Custom($"{slave.getName() ?? "未知"}的奴印到期解除了");
                                continue;
                            }
                            if (slave.has_attack_target && slave.attack_target != null && slave.attack_target.isActor() && slave.attack_target.a == master)
                            {
                                slave.cancelAllBeh();
                            }
                            if (master.has_attack_target && master.attack_target != null && master.attack_target.isActor() && master.attack_target.a == slave)
                            {
                                master.cancelAllBeh();
                            }
                            if (slave.current_tile != null && master.current_tile != null)
                            {
                                if (slave.tile_target != master.current_tile || slave.current_path == null || slave.current_path.Count == 0)
                                {
                                    ActorMove.goTo(slave, master.current_tile, pPathOnLiquid: false, pWalkOnBlocks: true, pPathOnLava: false, pLimitPathfindingRegions: 0);
                                }
                            }
                            Actor enemy = null;
                            if (master.attack_target != null && master.attack_target.isActor())
                                enemy = master.attack_target.a;
                            else if (master.attackedBy != null && master.attackedBy.isActor())
                                enemy = master.attackedBy.a;
                            if (enemy != null && enemy.isAlive() && enemy != slave && enemy.current_tile != null)
                            {
                                slave.startFightingWith(enemy);
                                ActorMove.goTo(slave, enemy.current_tile, pPathOnLiquid: false, pWalkOnBlocks: true, pPathOnLava: false, pLimitPathfindingRegions: 0);
                            }
                            long cur; slave.data.get(KEY_XP, out cur, 0L);
                            long last; slave.data.get(KEY_LAST_XP, out last, cur);
                            if (cur > last)
                            {
                                long delta = cur - last;
                                long tribute = (long)(delta * 0.3f);
                                if (tribute > 0)
                                {
                                    slave.data.set(KEY_XP, cur - tribute);
                                    long mxp; master.data.get(KEY_XP, out mxp, 0L);
                                    master.data.set(KEY_XP, mxp + tribute);
                                }
                            }
                            slave.data.set(KEY_LAST_XP, cur);
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
                            long mid; s.data.get(KEY_MASTER_ID, out mid, 0L);
                            if (mid <= 0) continue;
                            var m = World.world.units.get(mid);
                            if (m == null || m.isRekt()) continue;
                            Actor enemy = null;
                            if (m.attack_target != null && m.attack_target.isActor())
                                enemy = m.attack_target.a;
                            else if (m.attackedBy != null && m.attackedBy.isActor())
                                enemy = m.attackedBy.a;
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
                long masterId; __instance.data.get(KEY_SAVE_ONCE, out masterId, 0L);
                if (masterId <= 0) return true;
                __instance.data.set(KEY_SAVE_ONCE, 0L);
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
                if (!pSimObject.isActor()) return true; 
                var pTarget = pSimObject.a;
                long aMaster; __instance.data.get(KEY_MASTER_ID, out aMaster, 0L);
                long aSlave; __instance.data.get(KEY_SLAVE_ID, out aSlave, 0L);
                long bMaster; pTarget.data.get(KEY_MASTER_ID, out bMaster, 0L);
                long bSlave; pTarget.data.get(KEY_SLAVE_ID, out bSlave, 0L);
                bool paired =
                    (aMaster > 0 && aMaster == pTarget.data.id) ||
                    (aSlave > 0 && aSlave == pTarget.data.id) ||
                    (bMaster > 0 && bMaster == __instance.data.id) ||
                    (bSlave > 0 && bSlave == __instance.data.id);
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
                var caster = pAttacker?.a;
                var target = __instance;
                if (caster == null || target == null) return;
                TrySeal(caster, target);
            }
        }
        private static void ClearRelation(Actor slave, Actor masterOrNull)
        {
            if (slave != null)
            {
                slave.data.set(KEY_MASTER_ID, 0L);
                slave.data.set(KEY_EXPIRE_YEAR, 0);
                slave.data.set(KEY_LAST_XP, 0L);
            }
            var m = masterOrNull;
            if (m == null && slave != null)
            {
                long mid; slave.data.get(KEY_MASTER_ID, out mid, 0L);
                if (mid > 0) m = World.world.units.get(mid);
            }
            if (m != null) m.data.set(KEY_SLAVE_ID, 0L);
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