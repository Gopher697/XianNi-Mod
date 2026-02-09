using HarmonyLib;
namespace xn.expand
{
    public static class FanjieKingdomTrait
    {
        public const string TraitId = "xn_fanjie";
        private static bool _inited;
        private static KingdomTrait _cachedTrait;
        public static void Init()
        {
            if (_inited) return;
            _inited = true;
            RegisterTrait();
        }
        private static void RegisterTrait()
        {
            if (AssetManager.kingdoms_traits == null) return;
            if (AssetManager.kingdoms_traits.get(TraitId) != null) return;
            var trait = new KingdomTrait
            {
                id = TraitId,
                group_id = "fate",
                path_icon = "kingdomtrair/fanjie",
                rarity = Rarity.R3_Legendary,
                spawn_random_trait_allowed = false,
                needs_to_be_explored = false,
                has_localized_id = true,
                has_description_1 = true,
                show_for_unlockables_ui = true,
                show_in_knowledge_window = true
            };
            AssetManager.kingdoms_traits.add(trait);
            trait.unlock(false);
            _cachedTrait = trait;
        }
        public static bool HasFanjieTrait(Kingdom kingdom)
        {
            if (kingdom == null || kingdom.data == null) return false;
            _cachedTrait ??= AssetManager.kingdoms_traits?.get(TraitId);
            if (_cachedTrait == null) return false;
            return kingdom.hasTrait(_cachedTrait);
        }
        public static bool CityHasFanjieTrait(City city)
        {
            if (city == null) return false;
            return HasFanjieTrait(city.kingdom);
        }
        public static bool ActorHasFanjieTrait(Actor actor)
        {
            if (actor == null) return false;
            return HasFanjieTrait(actor.kingdom);
        }
    }
}