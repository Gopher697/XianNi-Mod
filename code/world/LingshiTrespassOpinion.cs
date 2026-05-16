using HarmonyLib;
using UnityEngine;

namespace xn.world
{
    internal static class LingshiTrespassOpinion
    {
        public const string OpinionId = "xn_opinion_lingshi_trespass";
        public const string KeyTrespass = "xn.diplo.lingshi_trespass";

        private const int PenaltyPerMineAction = 1;
        private const int MaxPenalty = 100;

        private static bool _patchRegistered;

        public static void Init(Harmony harmony)
        {
            if (!_patchRegistered)
            {
                harmony.Patch(
                    AccessTools.Method(typeof(OpinionLibrary), "init"),
                    postfix: new HarmonyMethod(typeof(LingshiTrespassOpinion), nameof(PostOpinionLibraryInit)));
                _patchRegistered = true;
            }

            RegisterIfNeeded();
        }

        public static void RegisterIfNeeded()
        {
            OpinionLibrary library = AssetManager.opinion_library;
            if (library == null || library.get(OpinionId) != null)
            {
                return;
            }

            library.add(new OpinionAsset
            {
                id = OpinionId,
                translation_key = "opinion_zones",
                translation_key_negative = "opinion_zones",
                calc = CalculateOpinion
            });
        }

        public static void RecordTrespass(Kingdom offended, Kingdom offender)
        {
            if (offended == null || offender == null || offended == offender)
            {
                return;
            }
            if (offended.isRekt() || offender.isRekt() || World.world == null || World.world.diplomacy == null)
            {
                return;
            }

            DiplomacyRelation relation = World.world.diplomacy.getRelation(offended, offender);
            if (relation == null || relation.data == null)
            {
                return;
            }

            string key = DirectionalKey(offended, offender);
            int current;
            relation.data.get(key, out current, 0);
            relation.data.set(key, Mathf.Clamp(current + PenaltyPerMineAction, 0, MaxPenalty));
        }

        private static void PostOpinionLibraryInit()
        {
            RegisterIfNeeded();
        }

        private static int CalculateOpinion(Kingdom main, Kingdom target)
        {
            if (main == null || target == null || main == target)
            {
                return 0;
            }
            if (main.isRekt() || target.isRekt() || World.world == null || World.world.diplomacy == null)
            {
                return 0;
            }

            DiplomacyRelation relation = World.world.diplomacy.getRelation(main, target);
            if (relation == null || relation.data == null)
            {
                return 0;
            }

            int penalty;
            relation.data.get(DirectionalKey(main, target), out penalty, 0);
            return -Mathf.Clamp(penalty, 0, MaxPenalty);
        }

        private static string DirectionalKey(Kingdom offended, Kingdom offender)
        {
            return KeyTrespass + "." + offended.id + "." + offender.id;
        }
    }
}
