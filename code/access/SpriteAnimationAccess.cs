using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class SpriteAnimationAccess
    {
        private static readonly FieldInfo PhenotypeField = AccessTools.Field(typeof(SpriteAnimation), "phenotype");
        private static bool _warnedPhenotype;

        public static void SetPhenotype(SpriteAnimation animation, PhenotypeAsset phenotype)
        {
            if (animation == null) return;
            if (PhenotypeField == null)
            {
                WarnOnce(ref _warnedPhenotype, "[XN] SpriteAnimation.phenotype field not found; phenotype was not changed.");
                return;
            }
            PhenotypeField.SetValue(animation, phenotype);
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
