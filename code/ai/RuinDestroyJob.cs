using System.Collections.Generic;
using HarmonyLib;
namespace xn.world
{
    public static class RuinDestroyJob
    {
        const string KEY_RUIN_ACTIVE   = "xn_ruin_active";    
        const string KEY_RUIN_TARGETID = "xn_ruin_target_id"; 
        private static readonly HashSet<long> _activeActorIds = new HashSet<long>();
        private static readonly List<long> _toRemove = new List<long>(64);
        private static readonly Dictionary<long, double> _nextGoToTime = new Dictionary<long, double>();
        const double GOTO_INTERVAL = 1.0; 
        const float NEAR_DIST_SQR = 9f;   
        const float FAR_DIST_SQR  = 100f; 
        public static void Init(Harmony h)
        {
            h.PatchAll(typeof(Hook_Map_Update));
        }
        public static void Mark(Actor a, Building target)
        {
            if (a == null || target == null) return;
            if (!a.isAlive() || !target.isAlive()) return;
            a.data.set(KEY_RUIN_ACTIVE, 1);
            a.data.set(KEY_RUIN_TARGETID, target.data.id);
            _activeActorIds.Add(a.data.id);
            a.startFightingWith(target);
        }
        public static void ClearMark(Actor a)
        {
            if (a == null) return;
            a.data.set(KEY_RUIN_ACTIVE, 0);
            a.data.set(KEY_RUIN_TARGETID, 0L);
            _activeActorIds.Remove(a.data.id);
            _nextGoToTime.Remove(a.data.id);
        }
        [HarmonyPatch(typeof(MapBox), "updateSimulation")]
        static class Hook_Map_Update
        {
            static void Postfix(MapBox __instance, float pElapsed)
            {
                if (_activeActorIds.Count == 0) return;
                double curTime = World.world.getCurSessionTime();
                _toRemove.Clear();
                foreach (long actorId in _activeActorIds)
                {
                    Actor a = World.world.units.get(actorId);
                    if (a == null || !a.isAlive())
                    {
                        _toRemove.Add(actorId);
                        continue;
                    }
                    if (a.isInsideSomething()) continue;
                    int active; a.data.get(KEY_RUIN_ACTIVE, out active, 0);
                    if (active != 1)
                    {
                        _toRemove.Add(actorId);
                        continue;
                    }
                    long bid; a.data.get(KEY_RUIN_TARGETID, out bid, 0L);
                    if (bid <= 0)
                    {
                        _toRemove.Add(actorId);
                        continue;
                    }
                    Building b = World.world.buildings.get(bid);
                    if (b == null || !b.isAlive())
                    {
                        _toRemove.Add(actorId);
                        continue;
                    }
                    if (!a.current_tile.isSameIsland(b.current_tile))
                    {
                        _toRemove.Add(actorId);
                        continue;
                    }
                    if (b.asset != null && b.asset.id == RuinBuildingAssets.ID && !b.isUsable())
                    {
                        b.removeBuildingFinal();
                        _toRemove.Add(actorId);
                        continue;
                    }
                    float sqrDist = Toolbox.SquaredDist(
                        a.current_tile.x, a.current_tile.y,
                        b.current_tile.x, b.current_tile.y);
                    if (sqrDist <= NEAR_DIST_SQR)
                    {
                        if (!a.has_attack_target || a.attack_target != b)
                        {
                            a.startFightingWith(b);
                        }
                    }
                    else if (sqrDist > FAR_DIST_SQR)
                    {
                        double nextTime;
                        if (!_nextGoToTime.TryGetValue(actorId, out nextTime) || curTime >= nextTime)
                        {
                            ActorMove.goTo(a, b.current_tile,
                                pPathOnLiquid: false,
                                pWalkOnBlocks: true,
                                pPathOnLava: false,
                                pLimitPathfindingRegions: 0);
                            _nextGoToTime[actorId] = curTime + GOTO_INTERVAL;
                        }
                        if (!a.has_attack_target || a.attack_target != b)
                        {
                            a.startFightingWith(b);
                        }
                    }
                    else
                    {
                        if (!a.has_attack_target || a.attack_target != b)
                        {
                            a.startFightingWith(b);
                        }
                    }
                }
                for (int i = 0; i < _toRemove.Count; i++)
                {
                    long id = _toRemove[i];
                    _activeActorIds.Remove(id);
                    _nextGoToTime.Remove(id);
                    Actor a = World.world.units.get(id);
                    if (a != null)
                    {
                        a.data.set(KEY_RUIN_ACTIVE, 0);
                        a.data.set(KEY_RUIN_TARGETID, 0L);
                    }
                }
            }
        }
    }
}