using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using ai.behaviours;
namespace xn.world
{
    internal static class BenyuanSystem
    {
        private const string ATTR_METAL = "attr_01_metal";
        private const string ATTR_WOOD  = "attr_02_wood";
        private const string ATTR_WATER = "attr_03_water";
        private const string ATTR_FIRE = "attr_04_fire";
        private const string ATTR_EARTH = "attr_05_earth";
        private const string KEY_ON_WATER = "xn.benyuan.water_on";
        private const string KEY_ON_FIRE = "xn.benyuan.fire_on";
        private const string KEY_ON_EARTH = "xn.benyuan.earth_on";
        private const string KEY_NEXT_DRAIN_WATER = "xn.benyuan.next_drain_water";
        private const string KEY_NEXT_DRAIN_FIRE = "xn.benyuan.next_drain_fire";
        private const string KEY_NEXT_DRAIN_EARTH = "xn.benyuan.next_drain_earth";
        private const string KEY_FIRE_DOT_STACKS = "xn.benyuan.fire_dot_stacks";
        private const string KEY_FIRE_DOT_UNTIL = "xn.benyuan.fire_dot_until";
        private const string KEY_FIRE_DOT_NEXTTICK = "xn.benyuan.fire_dot_next";
        private const string KEY_NIELI = "xn.stat.nieli";
        private const string KEY_ON_METAL = "xn.benyuan.metal_on";
        private const string KEY_ON_WOOD  = "xn.benyuan.wood_on";
        private const string KEY_NEXT_DRAIN_METAL = "xn.benyuan.next_drain_metal";
        private const string KEY_NEXT_DRAIN_WOOD  = "xn.benyuan.next_drain_wood";
        private const float  COST_PERCENT = 0.10f;
        private const float  TICK_SEC     = 10f;
        private static readonly HashSet<long> s_activeUnits = new HashSet<long>();
        private static readonly HashSet<long> s_benyuanActors = new HashSet<long>();
        private static bool s_initialScanDone = false;
        private static float s_lastScanTime = 0f;
        private const float SCAN_INTERVAL = 5f; 
        public static void Init(Harmony h)
        {
            BenyuanFX.Init(h);
            h.Patch(AccessTools.Method(typeof(MapBox), "Update"),
                    postfix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Post_Map_Update)));
            h.Patch(AccessTools.Method(typeof(Actor), "restoreHealth", new System.Type[] { typeof(int) }),
                    prefix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Pre_restoreHealth)));
            h.Patch(AccessTools.Method(typeof(Actor), "addTrait", new System.Type[] { typeof(string), typeof(bool) }),
                    postfix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Post_Actor_addTrait_String)));
            h.Patch(AccessTools.Method(typeof(Actor), "addTrait", new System.Type[] { typeof(ActorTrait), typeof(bool) }),
                    postfix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Post_Actor_addTrait_Trait)));
            h.Patch(AccessTools.Method(typeof(Actor), "isInStablePlace"),
                    postfix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Post_isInStablePlace)));
            h.Patch(AccessTools.Method(typeof(RegionPathFinder), "finalPath"),
                    prefix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Pre_RegionPathFinder_finalPath)));
            h.Patch(AccessTools.Method(typeof(ai.behaviours.BehFightCheckEnemyIsOk), "execute"),
                    prefix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Pre_BehFightCheckEnemyIsOk)));
            h.Patch(AccessTools.Method(typeof(Projectile), "checkHitForUnit"),
                    prefix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Pre_Projectile_checkHitForUnit)));
            h.Patch(AccessTools.Method(typeof(Actor), "tryToAttack",
                    new System.Type[] { typeof(BaseSimObject), typeof(bool), typeof(System.Action),
                                        typeof(Vector3), typeof(Kingdom), typeof(WorldTile), typeof(float) }),
                    prefix: new HarmonyMethod(typeof(BenyuanSystem), nameof(Pre_Actor_tryToAttack)));
        }
        private static bool IsDragon(Actor a)
        {
            return a != null && a.asset != null && (a.asset.id == "dragon" || a.asset.id == "zombie_dragon");
        }
        private static bool HasAnyBenyuanTrait(Actor a)
        {
            if (a == null) return false;
            return a.hasTrait(ATTR_METAL) || a.hasTrait(ATTR_WOOD) ||
                   a.hasTrait(ATTR_WATER) || a.hasTrait(ATTR_FIRE) ||
                   a.hasTrait(ATTR_EARTH);
        }
        private static void Post_isInStablePlace(Actor __instance, ref bool __result)
        {
            if (__instance == null || __result) return; 
            if (IsDragon(__instance)) return; 
            if (xn.access.ActorAccess.IsFlying(__instance))
            {
                if (__instance.current_tile != null && __instance.current_tile.Type.ocean)
                {
                    __result = true;
                }
            }
        }
        public static bool OpenMetal(Actor a)
        {
            if (a == null || !a.isAlive()) return false;
            if (!a.hasTrait(ATTR_METAL)) return false;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_METAL, 1);
            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_METAL, Time.time + TICK_SEC);
            BenyuanFX.PlayOnce(a);
            if (!IsDragon(a)) a.setFlying(true);
            s_activeUnits.Add(xn.access.ActorAccess.GetData(a).id);
            BroadcastSystem.PostActor(a, a.getName() + " activates flight");
            return true;
        }
        public static void CloseMetal(Actor a)
        {
            if (a == null) return;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_METAL, 0);
            BenyuanFX.ResetPlayed(a);
            if (!IsDragon(a) && !HasAnyBenyuanTrait(a))
            {
                a.setFlying(false);
            }
            TryRemoveFromCache(a);
        }
        public static bool OpenWood(Actor a)
        {
            if (a == null || !a.isAlive()) return false;
            if (!a.hasTrait(ATTR_WOOD)) return false;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_WOOD, 1);
            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_WOOD, Time.time + TICK_SEC);
            BenyuanFX.PlayOnce(a);
            if (!IsDragon(a)) a.setFlying(true);
            s_activeUnits.Add(xn.access.ActorAccess.GetData(a).id);
            BroadcastSystem.PostActor(a, a.getName() + " activates flight");
            return true;
        }
        public static void CloseWood(Actor a)
        {
            if (a == null) return;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_WOOD, 0);
            BenyuanFX.ResetPlayed(a);
            if (!IsDragon(a) && !HasAnyBenyuanTrait(a))
            {
                a.setFlying(false);
            }
            TryRemoveFromCache(a);
        }
        public static bool OpenWater(Actor a)
        {
            if (a == null || !a.isAlive()) return false;
            if (!a.hasTrait(ATTR_WATER)) return false;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_WATER, 1);
            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_WATER, Time.time + TICK_SEC);
            BenyuanFX.PlayOnce(a);
            if (!IsDragon(a)) a.setFlying(true);
            s_activeUnits.Add(xn.access.ActorAccess.GetData(a).id);
            BroadcastSystem.PostActor(a, a.getName() + " activates flight");
            return true;
        }
        public static void CloseWater(Actor a)
        {
            if (a == null) return;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_WATER, 0);
            BenyuanFX.ResetPlayed(a);
            if (!IsDragon(a) && !HasAnyBenyuanTrait(a))
            {
                a.setFlying(false);
            }
            TryRemoveFromCache(a);
        }
        public static bool OpenFire(Actor a)
        {
            if (a == null || !a.isAlive()) return false;
            if (!a.hasTrait(ATTR_FIRE)) return false;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_FIRE, 1);
            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_FIRE, Time.time + TICK_SEC);
            BenyuanFX.PlayOnce(a);
            if (!IsDragon(a)) a.setFlying(true);
            s_activeUnits.Add(xn.access.ActorAccess.GetData(a).id);
            BroadcastSystem.PostActor(a, a.getName() + " activates flight");
            return true;
        }
        public static void CloseFire(Actor a)
        {
            if (a == null) return;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_FIRE, 0);
            BenyuanFX.ResetPlayed(a);
            if (!IsDragon(a) && !HasAnyBenyuanTrait(a))
            {
                a.setFlying(false);
            }
            TryRemoveFromCache(a);
        }
        public static bool OpenEarth(Actor a)
        {
            if (a == null || !a.isAlive()) return false;
            if (!a.hasTrait(ATTR_EARTH)) return false;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_EARTH, 1);
            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_EARTH, Time.time + TICK_SEC);
            BenyuanFX.PlayOnce(a);
            if (!IsDragon(a)) a.setFlying(true);
            s_activeUnits.Add(xn.access.ActorAccess.GetData(a).id);
            BroadcastSystem.PostActor(a, a.getName() + " activates flight");
            return true;
        }
        public static void CloseEarth(Actor a)
        {
            if (a == null) return;
            xn.access.ActorAccess.GetData(a).set(KEY_ON_EARTH, 0);
            BenyuanFX.ResetPlayed(a);
            if (!IsDragon(a) && !HasAnyBenyuanTrait(a))
            {
                a.setFlying(false);
            }
            TryRemoveFromCache(a);
        }
        private static void TryRemoveFromCache(Actor a)
        {
            if (a == null) return;
            int onM = 0, onW = 0, onWa = 0, onF = 0, onE = 0, dotStacks = 0;
            xn.access.ActorAccess.GetData(a).get(KEY_ON_METAL, out onM, 0);
            xn.access.ActorAccess.GetData(a).get(KEY_ON_WOOD, out onW, 0);
            xn.access.ActorAccess.GetData(a).get(KEY_ON_WATER, out onWa, 0);
            xn.access.ActorAccess.GetData(a).get(KEY_ON_FIRE, out onF, 0);
            xn.access.ActorAccess.GetData(a).get(KEY_ON_EARTH, out onE, 0);
            xn.access.ActorAccess.GetData(a).get(KEY_FIRE_DOT_STACKS, out dotStacks, 0);
            if (onM == 0 && onW == 0 && onWa == 0 && onF == 0 && onE == 0 && dotStacks == 0)
            {
                s_activeUnits.Remove(xn.access.ActorAccess.GetData(a).id);
            }
        }
        public static void AddFireDot(Actor a, int stacks)
        {
            if (a == null || !a.isAlive()) return;
            int cur; xn.access.ActorAccess.GetData(a).get(KEY_FIRE_DOT_STACKS, out cur, 0);
            xn.access.ActorAccess.GetData(a).set(KEY_FIRE_DOT_STACKS, cur + stacks);
            xn.access.ActorAccess.GetData(a).set(KEY_FIRE_DOT_UNTIL, Time.time + 5f);
            xn.access.ActorAccess.GetData(a).set(KEY_FIRE_DOT_NEXTTICK, Time.time + 1f);
            s_activeUnits.Add(xn.access.ActorAccess.GetData(a).id);
        }
        private static float s_nextAutoOpenCheck = 0f;
        private const float AUTO_OPEN_CHECK_INTERVAL = 0.5f; 
        private static void Post_Map_Update(MapBox __instance)
        {
            float now = Time.time;
            if (!s_initialScanDone)
            {
                s_initialScanDone = true;
                s_lastScanTime = now;
                ScanExistingBenyuanActors();
            }
            if (now >= s_nextAutoOpenCheck)
            {
                s_nextAutoOpenCheck = now + AUTO_OPEN_CHECK_INTERVAL;
                CheckAutoOpenBenyuan();
            }
            if (s_activeUnits.Count == 0) return;
            List<long> toRemove = null;
            foreach (long id in s_activeUnits.ToArray())
            {
                Actor a = World.world.units.get(id);
                if (a == null || !a.isAlive())
                {
                    if (toRemove == null) toRemove = new List<long>();
                    toRemove.Add(id);
                    continue;
                }
                int onM = 0, onW = 0, onWa = 0, onF = 0, onE = 0;
                xn.access.ActorAccess.GetData(a).get(KEY_ON_METAL, out onM, 0);
                xn.access.ActorAccess.GetData(a).get(KEY_ON_WOOD,  out onW, 0);
                xn.access.ActorAccess.GetData(a).get(KEY_ON_WATER, out onWa, 0);
                xn.access.ActorAccess.GetData(a).get(KEY_ON_FIRE,  out onF,  0);
                xn.access.ActorAccess.GetData(a).get(KEY_ON_EARTH, out onE,  0);
                if (onM == 1 && !a.hasTrait(ATTR_METAL)) { CloseMetal(a); onM = 0; }
                if (onW == 1 && !a.hasTrait(ATTR_WOOD))  { CloseWood(a);  onW = 0; }
                if (onWa == 1 && !a.hasTrait(ATTR_WATER)) { CloseWater(a); onWa = 0; }
                if (onF == 1 && !a.hasTrait(ATTR_FIRE))  { CloseFire(a);  onF = 0; }
                if (onE == 1 && !a.hasTrait(ATTR_EARTH)) { CloseEarth(a); onE = 0; }
                if (onM == 1)
                {
                    float next; xn.access.ActorAccess.GetData(a).get(KEY_NEXT_DRAIN_METAL, out next, 0f);
                    if (now >= next)
                    {
                        int nl; xn.access.ActorAccess.GetData(a).get(KEY_NIELI, out nl, 0);
                        if (nl <= 0) { CloseMetal(a); }
                        else {
                            int cost = Mathf.Max(1, Mathf.FloorToInt(nl * COST_PERCENT));
                            xn.access.ActorAccess.GetData(a).set(KEY_NIELI, nl - cost);
                            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_METAL, now + TICK_SEC);
                        }
                    }
                }
                if (onW == 1)
                {
                    float next; xn.access.ActorAccess.GetData(a).get(KEY_NEXT_DRAIN_WOOD, out next, 0f);
                    if (now >= next)
                    {
                        int nl; xn.access.ActorAccess.GetData(a).get(KEY_NIELI, out nl, 0);
                        if (nl <= 0) { CloseWood(a); }
                        else {
                            int cost = Mathf.Max(1, Mathf.FloorToInt(nl * COST_PERCENT));
                            xn.access.ActorAccess.GetData(a).set(KEY_NIELI, nl - cost);
                            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_WOOD, now + TICK_SEC);
                        }
                    }
                }
                if (onWa == 1)
                {
                    float next; xn.access.ActorAccess.GetData(a).get(KEY_NEXT_DRAIN_WATER, out next, 0f);
                    if (now >= next)
                    {
                        int nl; xn.access.ActorAccess.GetData(a).get(KEY_NIELI, out nl, 0);
                        if (nl <= 0) { CloseWater(a); }
                        else {
                            int cost = Mathf.Max(1, Mathf.FloorToInt(nl * COST_PERCENT));
                            xn.access.ActorAccess.GetData(a).set(KEY_NIELI, nl - cost);
                            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_WATER, now + TICK_SEC);
                        }
                    }
                    a.finishStatusEffect("burning");
                }
                if (onF == 1)
                {
                    float next; xn.access.ActorAccess.GetData(a).get(KEY_NEXT_DRAIN_FIRE, out next, 0f);
                    if (now >= next)
                    {
                        int nl; xn.access.ActorAccess.GetData(a).get(KEY_NIELI, out nl, 0);
                        if (nl <= 0) { CloseFire(a); }
                        else {
                            int cost = Mathf.Max(1, Mathf.FloorToInt(nl * COST_PERCENT));
                            xn.access.ActorAccess.GetData(a).set(KEY_NIELI, nl - cost);
                            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_FIRE, now + TICK_SEC);
                        }
                    }
                    a.finishStatusEffect("burning");
                }
                if (onE == 1)
                {
                    float next; xn.access.ActorAccess.GetData(a).get(KEY_NEXT_DRAIN_EARTH, out next, 0f);
                    if (now >= next)
                    {
                        int nl; xn.access.ActorAccess.GetData(a).get(KEY_NIELI, out nl, 0);
                        if (nl <= 0) { CloseEarth(a); }
                        else {
                            int cost = Mathf.Max(1, Mathf.FloorToInt(nl * COST_PERCENT));
                            xn.access.ActorAccess.GetData(a).set(KEY_NIELI, nl - cost);
                            xn.access.ActorAccess.GetData(a).set(KEY_NEXT_DRAIN_EARTH, now + TICK_SEC);
                        }
                    }
                    a.finishStatusEffect("slowness");
                    a.finishStatusEffect("frozen");
                }
                int stacks; xn.access.ActorAccess.GetData(a).get(KEY_FIRE_DOT_STACKS, out stacks, 0);
                if (stacks > 0)
                {
                    float until; xn.access.ActorAccess.GetData(a).get(KEY_FIRE_DOT_UNTIL, out until, 0f);
                    if (now >= until)
                    {
                        xn.access.ActorAccess.GetData(a).set(KEY_FIRE_DOT_STACKS, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_FIRE_DOT_UNTIL, 0f);
                        xn.access.ActorAccess.GetData(a).set(KEY_FIRE_DOT_NEXTTICK, 0f);
                    }
                    else
                    {
                        float nextTick; xn.access.ActorAccess.GetData(a).get(KEY_FIRE_DOT_NEXTTICK, out nextTick, 0f);
                        if (now >= nextTick)
                        {
                            int maxhp = xn.access.BaseSimObjectAccess.GetStats(a)["health_max"] > 0 ? (int)xn.access.BaseSimObjectAccess.GetStats(a)["health_max"] : a.getMaxHealth();
                            int dmg = Mathf.Max(1, Mathf.FloorToInt(maxhp * 0.03f * stacks));
                            a.changeHealth(-dmg);
                            if (!a.hasHealth()) a.batch.c_check_deaths.Add(a);
                            xn.access.ActorAccess.GetData(a).set(KEY_FIRE_DOT_NEXTTICK, now + 1f);
                        }
                    }
                }
                if (!IsDragon(a))
                {
                    bool hasTrait = HasAnyBenyuanTrait(a);
                    if (hasTrait)
                    {
                        if (!xn.access.ActorAccess.IsFlyingRaw(a)) a.setFlying(true);
                    }
                    else if (xn.access.ActorAccess.IsFlyingRaw(a) && onM == 0 && onW == 0 && onWa == 0 && onF == 0 && onE == 0)
                    {
                        a.setFlying(false);
                    }
                }
            }
            if (toRemove != null)
            {
                for (int i = 0; i < toRemove.Count; i++)
                    s_activeUnits.Remove(toRemove[i]);
            }
        }
        private static void CheckAutoOpenBenyuan()
        {
            if (s_benyuanActors.Count == 0) return;
            List<long> toRemove = null;
            foreach (long id in s_benyuanActors.ToArray())
            {
                Actor a = World.world.units.get(id);
                if (a == null || !a.isAlive())
                {
                    if (toRemove == null) toRemove = new List<long>();
                    toRemove.Add(id);
                    continue;
                }
                if (!HasAnyBenyuanTrait(a))
                {
                    if (toRemove == null) toRemove = new List<long>();
                    toRemove.Add(id);
                    continue;
                }
                if (!xn.access.ActorAccess.HasAttackTarget(a)) continue; 
                int curNieli = 0; xn.access.ActorAccess.GetData(a).get(KEY_NIELI, out curNieli, 0);
                if (curNieli < 20) continue;
                int onM = 0, onW = 0, onWa = 0, onF = 0, onE = 0;
                xn.access.ActorAccess.GetData(a).get(KEY_ON_METAL, out onM, 0);
                xn.access.ActorAccess.GetData(a).get(KEY_ON_WOOD,  out onW, 0);
                xn.access.ActorAccess.GetData(a).get(KEY_ON_WATER, out onWa, 0);
                xn.access.ActorAccess.GetData(a).get(KEY_ON_FIRE,  out onF,  0);
                xn.access.ActorAccess.GetData(a).get(KEY_ON_EARTH, out onE,  0);
                if (a.hasTrait(ATTR_METAL) && onM == 0) OpenMetal(a);
                if (a.hasTrait(ATTR_WOOD) && onW == 0) OpenWood(a);
                if (a.hasTrait(ATTR_WATER) && onWa == 0) OpenWater(a);
                if (a.hasTrait(ATTR_FIRE) && onF == 0) OpenFire(a);
                if (a.hasTrait(ATTR_EARTH) && onE == 0) OpenEarth(a);
            }
            if (toRemove != null)
            {
                for (int i = 0; i < toRemove.Count; i++)
                    s_benyuanActors.Remove(toRemove[i]);
            }
        }
        private static void ScanExistingBenyuanActors()
        {
            if (World.world == null || World.world.units == null) return;
            foreach (var unit in World.world.units)
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit.hasTrait(ATTR_METAL) || unit.hasTrait(ATTR_WOOD) ||
                    unit.hasTrait(ATTR_WATER) || unit.hasTrait(ATTR_FIRE) ||
                    unit.hasTrait(ATTR_EARTH))
                {
                    s_benyuanActors.Add(xn.access.ActorAccess.GetData(unit).id);
                }
            }
        }
        private static void Post_Actor_addTrait_String(Actor __instance, string pTraitID)
        {
            if (__instance == null || string.IsNullOrEmpty(pTraitID)) return;
            if (pTraitID == ATTR_METAL || pTraitID == ATTR_WOOD || pTraitID == ATTR_WATER ||
                pTraitID == ATTR_FIRE || pTraitID == ATTR_EARTH)
            {
                s_benyuanActors.Add(xn.access.ActorAccess.GetData(__instance).id);
            }
        }
        private static void Post_Actor_addTrait_Trait(Actor __instance, ActorTrait pTrait)
        {
            if (__instance == null || pTrait == null) return;
            string traitId = pTrait.id;
            if (traitId == ATTR_METAL || traitId == ATTR_WOOD || traitId == ATTR_WATER ||
                traitId == ATTR_FIRE || traitId == ATTR_EARTH)
            {
                s_benyuanActors.Add(xn.access.ActorAccess.GetData(__instance).id);
            }
        }
        private const string KEY_NOHEAL_END = "xn.benyuan.noheal_end";
        private static bool Pre_restoreHealth(Actor __instance, ref int pVal)
        {
            if (__instance == null || pVal <= 0) return true;
            float end; xn.access.ActorAccess.GetData(__instance).get(KEY_NOHEAL_END, out end, 0f);
            if (end > 0f && Time.time < end) return false; 
            return true;
        }
        private static bool Pre_RegionPathFinder_finalPath(MapRegion pMainRegion)
        {
            if (pMainRegion == null)
            {
                return false;
            }
            return true; 
        }
        private static bool Pre_BehFightCheckEnemyIsOk(Actor pActor, ref BehResult __result)
        {
            if (pActor == null)
            {
                __result = BehResult.Stop;
                return false;
            }
            if (!xn.access.ActorAccess.HasAttackTarget(pActor))
            {
                __result = BehResult.Stop;
                return false;
            }
            if (!pActor.isEnemyTargetAlive())
            {
                __result = BehResult.Stop;
                return false;
            }
            if (!pActor.shouldContinueToAttackTarget())
            {
                pActor.clearAttackTarget();
                __result = BehResult.Stop;
                return false;
            }
            if (!xn.access.BaseSimObjectAccess.CanAttackTarget(pActor, xn.access.ActorAccess.GetAttackTarget(pActor)))
            {
                pActor.ignoreTarget(xn.access.ActorAccess.GetAttackTarget(pActor));
                pActor.clearAttackTarget();
                __result = BehResult.Stop;
                return false;
            }
            if (!xn.access.ActorAccess.IsInAttackRange(pActor, xn.access.ActorAccess.GetAttackTarget(pActor)))
            {
                Actor target = xn.access.ActorAccess.GetAttackTarget(pActor) as Actor;
                if (target == null)
                {
                    pActor.clearAttackTarget();
                    __result = BehResult.Stop;
                    return false;
                }
                bool attackerFlying = xn.access.ActorAccess.IsFlying(pActor);
                bool targetFlying = xn.access.ActorAccess.IsFlying(target);
                if (pActor.isWaterCreature())
                {
                    bool targetNotInWater = !xn.access.BaseSimObjectAccess.IsInLiquid(target) && (pActor.asset == null || !pActor.asset.force_land_creature);
                    bool targetFlyingCantReach = targetFlying && !attackerFlying;
                    if (targetNotInWater || targetFlyingCantReach)
                    {
                        pActor.ignoreTarget(target);
                        pActor.clearAttackTarget();
                        __result = BehResult.Stop;
                        return false;
                    }
                }
                else
                {
                    bool targetInWaterCantReach = xn.access.BaseSimObjectAccess.IsInLiquid(target) && !pActor.isWaterCreature();
                    bool targetFlyingCantReach = targetFlying && !attackerFlying;
                    if (targetInWaterCantReach || targetFlyingCantReach)
                    {
                        pActor.ignoreTarget(target);
                        pActor.clearAttackTarget();
                        __result = BehResult.Stop;
                        return false;
                    }
                }
            }
            if (pActor.chunk == null || xn.access.ActorAccess.GetAttackTarget(pActor) == null || xn.access.ActorAccess.GetAttackTarget(pActor).chunk == null)
            {
                pActor.clearAttackTarget();
                __result = BehResult.Stop;
                return false;
            }
            int x = pActor.chunk.x;
            int y = pActor.chunk.y;
            int x2 = xn.access.ActorAccess.GetAttackTarget(pActor).chunk.x;
            int y2 = xn.access.ActorAccess.GetAttackTarget(pActor).chunk.y;
            float num = 1f;
            if (Toolbox.Dist(x, y, x2, y2) >= (float)SimGlobals.m.unit_chunk_sight_range + num)
            {
                pActor.clearAttackTarget();
                __result = BehResult.Stop;
                return false;
            }
            xn.access.ActorAccess.SetBehActorTarget(pActor, xn.access.ActorAccess.GetAttackTarget(pActor));
            __result = BehResult.Continue;
            return false; 
        }
        private static bool Pre_Projectile_checkHitForUnit(Projectile __instance, BaseSimObject pObject, AttackData pData, ref AttackDataResult __result)
        {
            if (pObject == null || !pObject.isAlive())
            {
                __result = AttackDataResult.Continue;
                return false;
            }
            Vector3 projectilePosition = xn.access.ProjectileAccess.GetCurrentPosition3D(__instance);
            float projectileZ = projectilePosition.z;
            float targetHeight = xn.access.BaseSimObjectAccess.GetHeight(pObject);
            float heightThreshold = 3f;
            if (pData.initiator != null && xn.access.BaseSimObjectAccess.IsActor(pData.initiator) && xn.access.BaseSimObjectAccess.IsActor(pObject))
            {
                Actor attacker = xn.access.BaseSimObjectAccess.GetActor(pData.initiator);
                Actor target = pObject as Actor;
                if (attacker != null && target != null && xn.access.ActorAccess.IsFlying(attacker) && xn.access.ActorAccess.IsFlying(target))
                {
                    heightThreshold = 10f; 
                }
            }
            if (Mathf.Abs(projectileZ - targetHeight) > heightThreshold)
            {
                __result = AttackDataResult.Continue;
                return false;
            }
            Vector3 targetPos = pObject.current_position;
            Vector3 projPos = projectilePosition;
            float dist = Toolbox.Dist(projPos.x, projPos.y + projPos.z, targetPos.x, targetPos.y + targetHeight);
            float hitRadius = __instance.asset.size + xn.access.BaseSimObjectAccess.GetStats(pObject)["size"];
            if (dist > hitRadius)
            {
                __result = AttackDataResult.Continue;
                return false;
            }
            __result = MapBox.checkAttackFor(pData, pObject);
            return false;
        }
        private static bool Pre_Actor_tryToAttack(Actor __instance, BaseSimObject pTarget, ref bool pDoChecks, ref bool __result)
        {
            if (__instance == null) return true;
            if (!pDoChecks) return true;
            if (!__instance.hasMeleeAttack()) return true;
            if (pTarget == null || pTarget.position_height <= 0f) return true;
            if (xn.access.ActorAccess.IsFlying(__instance))
            {
                if (__instance.isInWaterAndCantAttack())
                {
                    __result = false;
                    return false;
                }
                if (!xn.access.ActorAccess.IsAttackPossible(__instance))
                {
                    __result = false;
                    return false;
                }
                if (!xn.access.ActorAccess.IsInAttackRange(__instance, pTarget))
                {
                    __result = false;
                    return false;
                }
                pDoChecks = false;
            }
            return true;
        }
    }
}
