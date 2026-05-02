using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
namespace xn.world
{
    internal static class SpaceRiftSystem
    {
        private const int ISLAND_RADIUS = 3;     
        private const int BORDER_RADIUS = 25;     
        private const int COOLDOWN_YEARS = 30;   
        private const int SEARCH_TRIES = 200;    
        private const int TICK_INTERVAL = 10;    
        private const int MAX_DURATION_YEARS = 3; 
        private static bool  s_active;
        private static long  s_attackerId;
        private static long  s_targetId;
        private static Vector2Int s_center; 
        private static int   s_tick;
        private static int   s_startYear; 
        private static readonly List<RecordTile> s_changed = new List<RecordTile>(512);
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
        private static readonly string[] REALM_IDS = {
            "realm_01_qi","realm_02_foundation","realm_03_core","realm_04_nascent",
            "realm_05_deity","realm_06_infantchg","realm_07_wending","realm_08_kuinie",
            "realm_09_jingnie","realm_10_suinie","realm_11_kongnie","realm_12_kongling",
            "realm_13_kongxuan","realm_14_gtianzun","realm_15_half_tatian","realm_16_tatian"
        };
        private const string KEY_CD_UNTIL_YEAR = "xn.space.cd_until_year"; 
        private const string KEY_BACK_X = "xn.space.back_x";               
        private const string KEY_BACK_Y = "xn.space.back_y";               
        private const string KEY_SPACE_ACTIVE = "xn.space.active";         
        private const string KEY_AMB_DEMON  = AmbitionSystem.KEY_AMB_DEMON;   
        private const string KEY_AMB_DRAGON = AmbitionSystem.KEY_AMB_DRAGON;  
        private static bool IsRealmTianzunOrHigher(Actor a)
        {
            if (a == null) return false;
            int realmIdx = GetRealmIndex(a);
            return realmIdx >= 10; 
        }
        private static int GetRealmIndex(Actor a)
        {
            if (a == null) return -1;
            var ts = a.getTraits();
            if (ts == null) return -1;
            int idx = -1;
            foreach (var t in ts)
            {
                if (t == null) continue;
                for (int i = 0; i < REALM_IDS.Length; i++)
                {
                    if (t.id == REALM_IDS[i])
                    {
                        if (i > idx) idx = i;
                    }
                }
            }
            return idx;
        }
        private static bool IsRealmDifferenceAllowed(Actor attacker, Actor target)
        {
            int attackerRealm = GetRealmIndex(attacker);
            int targetRealm = GetRealmIndex(target);
            if (attackerRealm < 0 || targetRealm < 0) return false;
            int diff = Math.Abs(attackerRealm - targetRealm);
            return diff <= 1;
        }
        private static bool IsAmbitionUnit(Actor a)
        {
            if (a == null) return false;
            int v;
            xn.access.ActorAccess.GetData(a).get(KEY_AMB_DEMON, out v, 0);   if (v == 1) return true;
            xn.access.ActorAccess.GetData(a).get(KEY_AMB_DRAGON, out v, 0);  if (v == 1) return true;
            return false;
        }
        private static bool IsSpaceParticipant(Actor a)
        {
            if (a == null) return false;
            if (!s_active) return false;
            return xn.access.ActorAccess.GetData(a).id == s_attackerId || xn.access.ActorAccess.GetData(a).id == s_targetId;
        }
        internal static bool IsSpaceActiveFor(Actor a)
        {
            if (!s_active || a == null) return false;
            int flag; xn.access.ActorAccess.GetData(a).get(KEY_SPACE_ACTIVE, out flag, 0);
            return flag == 1;
        }
        [HarmonyPatch(typeof(Actor), "getHit", new Type[] {
            typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject),
            typeof(bool), typeof(bool), typeof(bool)
        })]
        internal static class Patch_Actor_getHit_TryOpenSpace
        {
            static void Postfix(Actor __instance, float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker,
                                bool pSkipIfShake, bool pMetallicWeapon, bool pCheckDamageReduction)
            {
                if (s_active) return; 
                var target = __instance;
                if (xn.tournament.TournamentManager.IsRunning &&
                    (xn.tournament.TournamentManager.IsParticipant(__instance) ||
                     (pAttacker != null && xn.access.BaseSimObjectAccess.GetActor(pAttacker) != null && xn.tournament.TournamentManager.IsParticipant(xn.access.BaseSimObjectAccess.GetActor(pAttacker)))))
                    return;
                Actor attacker = pAttacker != null ? xn.access.BaseSimObjectAccess.GetActor(pAttacker) : null;
                if (attacker == null && xn.access.ActorAccess.GetAttackedBy(target) != null) attacker = xn.access.BaseSimObjectAccess.GetActor(xn.access.ActorAccess.GetAttackedBy(target));
                if (attacker == null || target == null) return;
                if (!attacker.isAlive() || !target.isAlive()) return;
                if (!IsRealmTianzunOrHigher(attacker)) return;
                if (IsAmbitionUnit(attacker) || IsAmbitionUnit(target)) return;
                if (!IsRealmDifferenceAllowed(attacker, target)) return;
                if (!attacker.areFoes(target)) return;
                int cdUntil; xn.access.ActorAccess.GetData(attacker).get(KEY_CD_UNTIL_YEAR, out cdUntil, 0);
                if (cdUntil > 0 && Date.getCurrentYear() < cdUntil) return;
                WorldTile center;
                if (!TryPickOceanAndBuildIsland(out center))
                {
                    BroadcastSystem.PostActor(attacker, T("broadcast_space_rift_no_ocean", "{0} failed to open a space rift: no large enough ocean was found", attacker.getName()));
                    xn.access.ActorAccess.GetData(attacker).set(KEY_CD_UNTIL_YEAR, Date.getCurrentYear() + 1);
                    return;
                }
                s_active = true;
                s_attackerId = xn.access.ActorAccess.GetData(attacker).id;
                s_targetId = xn.access.ActorAccess.GetData(target).id;
                s_center = center.pos;
                s_tick = 0;
                s_startYear = Date.getCurrentYear(); 
                SaveBackPos(attacker);
                SaveBackPos(target);
                attacker.cancelAllBeh();
                target.cancelAllBeh();
                TeleportToIsland(attacker, true);
                TeleportToIsland(target, false);
                xn.access.ActorAccess.GetData(attacker).set(KEY_SPACE_ACTIVE, 1);
                xn.access.ActorAccess.GetData(target).set(KEY_SPACE_ACTIVE, 1);
                attacker.startFightingWith(target); 
                target.startFightingWith(attacker);
                xn.access.ActorAccess.GetData(attacker).set(KEY_CD_UNTIL_YEAR, Date.getCurrentYear() + COOLDOWN_YEARS);
                xn.world.TerritoryFX.StartFor(attacker);
                xn.world.TerritoryFX.StartFor(target);
                BroadcastSystem.PostAtTile(center, T("broadcast_space_rift_opened", "{0} opened a space rift against {1} ({2},{3})", attacker.getName(), target.getName(), s_center.x, s_center.y));
            }
        }
        [HarmonyPatch(typeof(MapBox), "updateSimulation")]
        internal static class Patch_MapBox_updateSimulation_ArenaMaintain
        {
            static void Postfix()
            {
                if (!s_active) return;
                s_tick++;
                if ((s_tick % TICK_INTERVAL) != 0) return;
                var units = World.world.units.getSimpleList(); 
                int maxDist2 = (BORDER_RADIUS + 1) * (BORDER_RADIUS + 1);
                int innerDist2 = (BORDER_RADIUS - 1) * (BORDER_RADIUS - 1);
                Actor a1 = World.world.units.get(s_attackerId);
                Actor a2 = World.world.units.get(s_targetId);
                int currentYear = Date.getCurrentYear();
                if (currentYear >= s_startYear + MAX_DURATION_YEARS)
                {
                    EndAndRestore(a1, a2);
                    if (a1 != null && a1.isAlive() && a2 != null && a2.isAlive())
                    {
                        BroadcastSystem.PostActor(a1, T("broadcast_space_rift_auto_end", "{0} and {1}'s space rift lasted 3 years and ended automatically", a1.getName(), a2.getName()));
                    }
                    return;
                }
                if (a1 == null || !a1.isAlive() || a2 == null || !a2.isAlive())
                {
                    EndAndRestore(a1, a2);
                    return;
                }
                int a1SpaceFlag; xn.access.ActorAccess.GetData(a1).get(KEY_SPACE_ACTIVE, out a1SpaceFlag, 0);
                int a2SpaceFlag; xn.access.ActorAccess.GetData(a2).get(KEY_SPACE_ACTIVE, out a2SpaceFlag, 0);
                if (a1SpaceFlag != 1 || a2SpaceFlag != 1)
                {
                    EndAndRestore(a1, a2);
                    return;
                }
                for (int i = 0; i < units.Count; i++)
                {
                    var u = units[i];
                    if (!u.isAlive()) continue;
                    Vector2Int p = u.current_tile.pos;
                    int dx = p.x - s_center.x;
                    int dy = p.y - s_center.y;
                    int d2 = dx*dx + dy*dy;
                    bool isParticipant = (xn.access.ActorAccess.GetData(u).id == s_attackerId || xn.access.ActorAccess.GetData(u).id == s_targetId);
                    if (isParticipant)
                    {
                        if (d2 > innerDist2)
                        {
                            var inTile = ClampIntoCircle(p, s_center, BORDER_RADIUS - 1);
                            if (inTile != null)
                            {
                                u.cancelAllBeh();
                                u.setCurrentTile(inTile);
                                Actor other = (xn.access.ActorAccess.GetData(u).id == s_attackerId) ? a2 : a1;
                                if (other != null && other.isAlive())
                                {
                                    u.startFightingWith(other);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (d2 <= maxDist2)
                        {
                            var outTile = ClampOutsideCircle(p, s_center, BORDER_RADIUS + 1);
                            if (outTile != null) u.setCurrentTile(outTile);
                        }
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Actor), "die", new Type[] { typeof(bool), typeof(AttackType), typeof(bool), typeof(bool) })]
        internal static class Patch_Actor_die_ArenaEnd
        {
            static void Postfix(Actor __instance, bool pDestroy, AttackType pType, bool pCountDeath, bool pLogFavorite)
            {
                if (!s_active) return;
                if (__instance == null) return;
                if (xn.access.ActorAccess.GetData(__instance).id == s_attackerId || xn.access.ActorAccess.GetData(__instance).id == s_targetId)
                {
                    var a1 = World.world.units.get(s_attackerId);
                    var a2 = World.world.units.get(s_targetId);
                    EndAndRestore(a1, a2);
                }
            }
        }
        [HarmonyPatch(typeof(Actor), "getHit", new Type[] {
            typeof(float), typeof(bool), typeof(AttackType), typeof(BaseSimObject),
            typeof(bool), typeof(bool), typeof(bool)
        })]
        internal static class Patch_Actor_getHit_BlockOutsideToInside
        {
            static bool Prefix(Actor __instance, float pDamage, bool pFlash, AttackType pAttackType, BaseSimObject pAttacker,
                               bool pSkipIfShake, bool pMetallicWeapon, bool pCheckDamageReduction)
            {
                if (!s_active) return true;
                var target = __instance;
                Actor attacker = (pAttacker != null) ? xn.access.BaseSimObjectAccess.GetActor(pAttacker) : null;
                if (attacker == null && xn.access.ActorAccess.GetAttackedBy(target) != null) attacker = xn.access.BaseSimObjectAccess.GetActor(xn.access.ActorAccess.GetAttackedBy(target));
                if (attacker == null) return true; 
                bool attackerIsParticipant = (xn.access.ActorAccess.GetData(attacker).id == s_attackerId || xn.access.ActorAccess.GetData(attacker).id == s_targetId);
                bool targetIsParticipant = (xn.access.ActorAccess.GetData(target).id == s_attackerId || xn.access.ActorAccess.GetData(target).id == s_targetId);
                if (!attackerIsParticipant && targetIsParticipant)
                    return false;
                return true;
            }
        }
        private static bool TryPickOceanAndBuildIsland(out WorldTile center)
        {
            center = null;
            for (int tries = 0; tries < SEARCH_TRIES; tries++)
            {
                int x = Randy.randomInt(0, MapBox.width - 1);
                int y = Randy.randomInt(0, MapBox.height - 1);
                var t = World.world.GetTileSimple(x, y);
                if (t == null || !t.Type.ocean) continue;
                if (!CheckOceanDisk(t, BORDER_RADIUS)) continue;
                s_changed.Clear();
                var tileSand = TileLibrary.sand != null ? TileLibrary.sand : TileLibrary.soil_low; 
                FillDiskAsIsland(t, ISLAND_RADIUS, tileSand);
                center = t;
                return true;
            }
            return false;
        }
        private static bool CheckOceanDisk(WorldTile pTile, int r)
        {
            int r2 = r * r;
            int cx = pTile.pos.x;
            int cy = pTile.pos.y;
            for (int dy = -r; dy <= r; dy++)
            {
                int yy = cy + dy;
                if (yy < 0 || yy >= MapBox.height) return false;
                int maxdx = r - Math.Abs(dy);
                for (int dx = -maxdx; dx <= maxdx; dx++)
                {
                    int xx = cx + dx;
                    if (xx < 0 || xx >= MapBox.width) return false;
                    var tt = World.world.GetTileSimple(xx, yy);
                    if (tt == null || !tt.Type.ocean) return false;
                }
            }
            return true;
        }
        private static void FillDiskAsIsland(WorldTile pCenter, int r, TileType ground)
        {
            int r2 = r * r;
            int cx = pCenter.pos.x;
            int cy = pCenter.pos.y;
            for (int dy = -r; dy <= r; dy++)
            {
                int yy = cy + dy;
                if (yy < 0 || yy >= MapBox.height) continue;
                int maxdx = r - Math.Abs(dy);
                for (int dx = -maxdx; dx <= maxdx; dx++)
                {
                    int xx = cx + dx;
                    if (xx < 0 || xx >= MapBox.width) continue;
                    var t = World.world.GetTileSimple(xx, yy);
                    if (t == null) continue;
                    s_changed.Add(new RecordTile { tile = t, main = t.main_type, top = t.top_type });
                    MapAction.terraformMain(t, ground, TerraformLibrary.flash, pSkipTerraform:false);
                }
            }
        }
        private static void EndAndRestore(Actor a1, Actor a2)
        {
            s_active = false;
            if (a1 != null) xn.access.ActorAccess.GetData(a1).set(KEY_SPACE_ACTIVE, 0);
            if (a2 != null) xn.access.ActorAccess.GetData(a2).set(KEY_SPACE_ACTIVE, 0);
            if (a1 != null) xn.world.TerritoryFX.StopFor(a1);
            if (a2 != null) xn.world.TerritoryFX.StopFor(a2);
            if (a1 != null && a1.isAlive()) { RestoreBackPos(a1); a1.cancelAllBeh(); }
            if (a2 != null && a2.isAlive()) { RestoreBackPos(a2); a2.cancelAllBeh(); }
            for (int i = 0; i < s_changed.Count; i++)
            {
                var rec = s_changed[i];
                if (rec.tile == null) continue;
                rec.tile.setTileTypes(rec.main, rec.top);
                MapAction.checkTileState(rec.tile, rec.tile.Type);
            }
            s_changed.Clear();
            s_attackerId = 0;
            s_targetId = 0;
            s_startYear = 0;
        }
        private static void SaveBackPos(Actor a)
        {
            var p = a.current_tile.pos;
            xn.access.ActorAccess.GetData(a).set(KEY_BACK_X, p.x);
            xn.access.ActorAccess.GetData(a).set(KEY_BACK_Y, p.y);
        }
        private static void RestoreBackPos(Actor a)
        {
            int x; xn.access.ActorAccess.GetData(a).get(KEY_BACK_X, out x, a.current_tile.pos.x);
            int y; xn.access.ActorAccess.GetData(a).get(KEY_BACK_Y, out y, a.current_tile.pos.y);
            var t = World.world.GetTileSimple(x, y);
            if (t != null) xn.access.ActorAccess.SetCurrentTilePosition(a, t); 
        }
        private static void TeleportToIsland(Actor a, bool leftSide)
        {
            int off = ISLAND_RADIUS - 1;
            int tx = s_center.x + (leftSide ? -off : +off);
            int ty = s_center.y;
            var t = World.world.GetTileSimple(tx, ty);
            if (t == null) t = World.world.GetTileSimple(s_center.x, s_center.y);
            if (t != null) xn.access.ActorAccess.SetCurrentTilePosition(a, t); 
        }
        private static WorldTile ClampIntoCircle(Vector2Int p, Vector2Int c, int r)
        {
            int dx = p.x - c.x;
            int dy = p.y - c.y;
            float d = Mathf.Sqrt(dx*dx + dy*dy);
            if (d <= r) return World.world.GetTileSimple(p.x, p.y);
            float k = r / (d + 0.0001f);
            int nx = c.x + Mathf.RoundToInt(dx * k);
            int ny = c.y + Mathf.RoundToInt(dy * k);
            return World.world.GetTileSimple(nx, ny);
        }
        private static WorldTile ClampOutsideCircle(Vector2Int p, Vector2Int c, int r)
        {
            int dx = p.x - c.x;
            int dy = p.y - c.y;
            float d = Mathf.Sqrt(dx*dx + dy*dy);
            if (d >= r) return World.world.GetTileSimple(p.x, p.y);
            float k = r / (d + 0.0001f);
            int nx = c.x + Mathf.RoundToInt(dx * k);
            int ny = c.y + Mathf.RoundToInt(dy * k);
            return World.world.GetTileSimple(nx, ny);
        }
        private struct RecordTile
        {
            public WorldTile tile;
            public TileType  main;
            public TopTileType top;
        }
    }
}
