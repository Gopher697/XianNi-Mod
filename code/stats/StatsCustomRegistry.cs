using ai;
namespace cultivation 
{
    internal static class StatsCustomRegistry
    {
        public static void Init()
        {
            AddIfMissing("Accuracy",         normalize:true,  showAsPercents:true,  min:0, max:99999);
            AddIfMissing("Dodge",            normalize:true,  showAsPercents:true,  min:0, max:99999);
            AddIfMissing("Resist",           normalize:true,  showAsPercents:false, min:0, max:99999); 
            AddIfMissing("ArmorPenPercent",  normalize:true,  showAsPercents:true,  min:0, max:99999);
            AddIfMissing("Vampire",          normalize:true,  showAsPercents:true,  min:0, max:100);
            AddIfMissing("Healback",         normalize:true,  showAsPercents:false, min:0, max:999999999);
            AddIfMissing("LingliMax", normalize: true, showAsPercents: false, min: 0, max: 999999999);
            AddIfMissing("LingliRegenPerYear", normalize: true, showAsPercents: false, min: 0, max: 999999999);
            AddIfMissing("YuanliMax", normalize: true, showAsPercents: false, min: 0, max: 999999999);
            AddIfMissing("YuanliRegenPerYear", normalize: true, showAsPercents: false, min: 0, max: 999999999);
            AddIfMissing("NieliMax", normalize: true, showAsPercents: false, min: 0, max: 999999999);
            AddIfMissing("NieliRegenPerYear", normalize: true, showAsPercents: false, min: 0, max: 999999999);
            AddIfMissing("fire_resistance", normalize: true, showAsPercents: true, min: 0, max: 100);
            AddIfMissing("lightning_resistance", normalize: true, showAsPercents: true, min: 0, max: 100);
            AddIfMissing("health_regen", normalize: true, showAsPercents: false, min: 0, max: 999999);
            AddIfMissing("courage", normalize: true, showAsPercents: false, min: 0, max: 100);
            AddIfMissing("luck", normalize: true, showAsPercents: false, min: 0, max: 999);
            AddIfMissing("cultivation_speed", normalize: true, showAsPercents: false, min: 0, max: 999999);
        }
        private static void AddIfMissing(string id, bool normalize, bool showAsPercents, float min, float max)
        {
            var lib = AssetManager.base_stats_library;
            if (lib.get(id) != null) return; 
            var a = new BaseStatAsset
            {
                id = id,
                normalize = normalize,
                normalize_min = min,
                normalize_max = max,
                show_as_percents = showAsPercents,
                used_only_for_civs = false
            };
            lib.add(a);
        }
    }
}