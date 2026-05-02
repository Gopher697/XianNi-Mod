using System.Collections.Generic;
using HarmonyLib;
using cultivation.ai;
namespace xn.world
{
    internal static class RuinQuestSystem
    {
        public const string RUIN_ID = RuinBuildingAssets.ID;
        private static readonly Dictionary<long, List<long>> _participants = new Dictionary<long, List<long>>(64);
        private static readonly HashSet<long> _allRuinIds = new HashSet<long>();
        private static readonly List<Actor> _tmpActors = new List<Actor>(2048);
        private static readonly List<long> _tmpIds = new List<long>(256);
        private static readonly List<long> _tmpRuinIds = new List<long>(64);
        private const string KEY_LINGSHI = "xn.stat.lingshi";
        private const string KEY_LINGSHI_SUPREME = "xn.stat.lingshi_supreme";
        private static ActorTrait[] _pool_divines;
        private static ActorTrait[] _pool_arts;
        private static int _lastCheckedYear = -1;
        public static void Init(Harmony harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Building), "kill"),
                postfix: new HarmonyMethod(typeof(RuinQuestSystem), nameof(Po_Kill)));
            harmony.Patch(
                AccessTools.Method(typeof(MapStats), "updateWorldTime"),
                postfix: new HarmonyMethod(typeof(RuinQuestSystem), nameof(Po_UpdateWorldTime)));
        }
        private static void Po_UpdateWorldTime(MapStats __instance, float pElapsed)
        {
            int currentYear = Date.getCurrentYear();
            if (_lastCheckedYear == currentYear) return;
            _lastCheckedYear = currentYear;
            CheckAndReassignRuins();
        }
        private static void CheckAndReassignRuins()
        {
            if (_allRuinIds.Count == 0) return;
            _tmpRuinIds.Clear();
            _tmpRuinIds.AddRange(_allRuinIds);
            for (int i = 0; i < _tmpRuinIds.Count; i++)
            {
                long ruinId = _tmpRuinIds[i];
                Building b = World.world.buildings.get(ruinId);
                if (b == null || !b.isAlive())
                {
                    _allRuinIds.Remove(ruinId);
                    _participants.Remove(ruinId);
                    continue;
                }
                if (!HasActiveParticipants(ruinId))
                {
                    TryAssignParticipants(b);
                }
            }
        }
        private static bool HasActiveParticipants(long ruinId)
        {
            if (!_participants.TryGetValue(ruinId, out var plist) || plist == null || plist.Count == 0)
                return false;
            for (int i = 0; i < plist.Count; i++)
            {
                Actor a = World.world.units.get(plist[i]);
                if (a != null && a.isAlive())
                {
                    int active;
                    xn.access.ActorAccess.GetData(a).get("xn_ruin_active", out active, 0);
                    if (active == 1) return true;
                }
            }
            return false;
        }
        public static void OnRuinPlaced(Building b)
        {
            if (b == null || !b.isAlive()) return;
            if (xn.access.BuildingAccess.GetAsset(b) == null || xn.access.BuildingAccess.GetAsset(b).id != RUIN_ID) return;
            if (_pool_divines == null || _pool_arts == null)
                BuildTraitPools();
            BuildingData data = xn.access.BuildingAccess.GetData(b);
            if (data == null) return;
            _allRuinIds.Add(data.id);
            TryAssignParticipants(b);
        }
        private static void TryAssignParticipants(Building b)
        {
            if (b == null || !b.isAlive()) return;
            BuildingData data = xn.access.BuildingAccess.GetData(b);
            if (data == null) return;
            _tmpActors.Clear();
            foreach (Actor a in World.world.units)
            {
                if (a == null || !a.isAlive()) continue;
                if (!a.current_tile.isSameIsland(b.current_tile)) continue;
                if (!HasAnyCultivationTrait(a)) continue;
                _tmpActors.Add(a);
            }
            if (_tmpActors.Count == 0)
            {
                _participants[data.id] = new List<long>();
                return;
            }
            int want = Randy.randomInt(1, 30);
            if (want > _tmpActors.Count) want = _tmpActors.Count;
            _tmpIds.Clear();
            int n = _tmpActors.Count;
            for (int k = 0; k < want; k++)
            {
                int pick = Randy.randomInt(0, n - 1);
                Actor chosen = _tmpActors[pick];
                GiveRuinDestroyJob(chosen, b);
                _tmpIds.Add(xn.access.ActorAccess.GetData(chosen).id);
                _tmpActors[pick] = _tmpActors[n - 1];
                n--;
            }
            _participants[data.id] = new List<long>(_tmpIds);
        }
        private static bool HasAnyCultivationTrait(Actor a)
        {
            var traits = a.getTraits();
            foreach (var tr in traits)
            {
                if (tr == null) continue;
                string id = tr.id;
                if (string.IsNullOrEmpty(id) || id.Length < 6) continue;
                char c = id[0];
                if (c == 'r' && id.StartsWith("realm_")) return true;
                if (c == 'a' && id.StartsWith("ancient_")) return true;
                if (c == 'b' && id.StartsWith("beast_")) return true;
            }
            return false;
        }
        private static void GiveRuinDestroyJob(Actor a, Building target)
        {
            if (a == null || target == null) return;
            RuinDestroyJob.Mark(a, target);
        }
        private static void Po_Kill(Building __instance)
        {
            Building b = __instance;
            if (b == null || xn.access.BuildingAccess.GetAsset(b) == null) return;
            if (xn.access.BuildingAccess.GetAsset(b).id != RUIN_ID) return;
            BuildingData data = xn.access.BuildingAccess.GetData(b);
            if (data == null) return;
            long bid = data.id;
            _allRuinIds.Remove(bid);
            if (!_participants.TryGetValue(bid, out var plist) || plist == null || plist.Count == 0)
            {
                _participants.Remove(bid);
                return;
            }
            Actor killer = FindNearestParticipant(plist, b);
            if (killer == null)
            {
                _participants.Remove(bid);
                return;
            }
            RewardAndBroadcast(killer);
            _participants.Remove(bid);
        }
        private static Actor FindNearestParticipant(List<long> plist, Building b)
        {
            Actor best = null;
            float bestDist = float.MaxValue;
            foreach (Actor a in Finder.getUnitsFromChunk(b.current_tile, 2, 0f, pRandom: false))
            {
                if (a == null || !a.isAlive()) continue;
                long id = xn.access.ActorAccess.GetData(a).id;
                bool isParticipant = false;
                for (int i = 0; i < plist.Count; i++)
                {
                    if (plist[i] == id) { isParticipant = true; break; }
                }
                if (!isParticipant) continue;
                float d = Toolbox.SquaredDistTile(a.current_tile, b.current_tile);
                if (d < bestDist) { bestDist = d; best = a; }
            }
            if (best == null)
            {
                for (int i = 0; i < plist.Count; i++)
                {
                    Actor a = World.world.units.get(plist[i]);
                    if (a == null || !a.isAlive()) continue;
                    if (!a.current_tile.isSameIsland(b.current_tile)) continue;
                    float d = Toolbox.SquaredDistTile(a.current_tile, b.current_tile);
                    if (d < bestDist) { bestDist = d; best = a; }
                }
            }
            return best;
        }
        private static void RewardAndBroadcast(Actor a)
        {
            float r = Randy.randomFloat(0f, 1f);
            string what;
            if (r < 0.30f)
            {
                var t = PickRandomTrait(_pool_divines, a, 10);
                if (t != null) { a.addTrait(t); what = "神通·" + t.id; }
                else what = "空手而归";
            }
            else if (r < 0.70f)
            {
                int add = Randy.randomInt(1, 1000);
                int cur; xn.access.ActorAccess.GetData(a).get(KEY_LINGSHI, out cur, 0);
                xn.access.ActorAccess.GetData(a).set(KEY_LINGSHI, cur + add);
                what = add + " 枚灵石";
            }
            else if (r < 0.79f)
            {
                int add = Randy.randomInt(1, 100);
                int cur; xn.access.ActorAccess.GetData(a).get(KEY_LINGSHI_SUPREME, out cur, 0);
                xn.access.ActorAccess.GetData(a).set(KEY_LINGSHI_SUPREME, cur + add);
                what = add + " 枚极品灵石";
            }
            else if (r < 0.80f)
            {
                var t = PickRandomTrait(_pool_arts, a, 10);
                if (t != null) { a.addTrait(t); what = "仙术·" + t.id; }
                else what = "空手而归";
            }
            else
            {
                what = "空手而归";
            }
            BroadcastSystem.RuinExploreReward(a, what);
        }
        private static ActorTrait PickRandomTrait(ActorTrait[] pool, Actor a, int maxRetry)
        {
            if (pool == null || pool.Length == 0) return null;
            for (int i = 0; i < maxRetry; i++)
            {
                int idx = Randy.randomInt(0, pool.Length - 1);
                var t = pool[idx];
                if (t != null && !a.hasTrait(t))
                    return t;
            }
            return null;
        }
        private static void BuildTraitPools()
        {
            var list = AssetManager.traits.list;
            var div = new List<ActorTrait>(64);
            var art = new List<ActorTrait>(64);
            foreach (var t in list)
            {
                if (t == null) continue;
                string id = t.id;
                if (string.IsNullOrEmpty(id) || id.Length < 5) continue;
                if (id.StartsWith("divine_")) div.Add(t);
                else if (id.StartsWith("art_")) art.Add(t);
            }
            _pool_divines = div.ToArray();
            _pool_arts = art.ToArray();
        }
    }
}
