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
            BuildingData targetData = xn.access.BuildingAccess.GetData(target);
            if (targetData == null) return;
            xn.access.ActorAccess.GetData(a).set(KEY_RUIN_ACTIVE, 1);
            xn.access.ActorAccess.GetData(a).set(KEY_RUIN_TARGETID, targetData.id);
            _activeActorIds.Add(xn.access.ActorAccess.GetData(a).id);
            a.startFightingWith(target);
        }
        public static void ClearMark(Actor a)
        {
            if (a == null) return;
            xn.access.ActorAccess.GetData(a).set(KEY_RUIN_ACTIVE, 0);
            xn.access.ActorAccess.GetData(a).set(KEY_RUIN_TARGETID, 0L);
            _activeActorIds.Remove(xn.access.ActorAccess.GetData(a).id);
            _nextGoToTime.Remove(xn.access.ActorAccess.GetData(a).id);
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
                    int active; xn.access.ActorAccess.GetData(a).get(KEY_RUIN_ACTIVE, out active, 0);
                    if (active != 1)
                    {
                        _toRemove.Add(actorId);
                        continue;
                    }
                    long bid; xn.access.ActorAccess.GetData(a).get(KEY_RUIN_TARGETID, out bid, 0L);
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
                    if (xn.access.BuildingAccess.GetAsset(b) != null && xn.access.BuildingAccess.GetAsset(b).id == RuinBuildingAssets.ID && !b.isUsable())
                    {
                        xn.access.BuildingAccess.RemoveBuildingFinal(b);
                        _toRemove.Add(actorId);
                        continue;
                    }
                    float sqrDist = Toolbox.SquaredDist(
                        a.current_tile.x, a.current_tile.y,
                        b.current_tile.x, b.current_tile.y);
                    if (sqrDist <= NEAR_DIST_SQR)
                    {
                        if (!xn.access.ActorAccess.HasAttackTarget(a) || xn.access.ActorAccess.GetAttackTarget(a) != b)
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
                        if (!xn.access.ActorAccess.HasAttackTarget(a) || xn.access.ActorAccess.GetAttackTarget(a) != b)
                        {
                            a.startFightingWith(b);
                        }
                    }
                    else
                    {
                        if (!xn.access.ActorAccess.HasAttackTarget(a) || xn.access.ActorAccess.GetAttackTarget(a) != b)
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
                        xn.access.ActorAccess.GetData(a).set(KEY_RUIN_ACTIVE, 0);
                        xn.access.ActorAccess.GetData(a).set(KEY_RUIN_TARGETID, 0L);
                    }
                }
            }
        }
    }
}
