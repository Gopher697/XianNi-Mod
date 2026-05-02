using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class SubspeciesAccess
    {
        private static readonly FieldInfo PhenotypeIndexesField = AccessTools.Field(typeof(Subspecies), "_phenotypes_set_indexes");
        private static readonly MethodInfo CachePhenotypeMethod = AccessTools.Method(typeof(Subspecies), "cachePhenotype", new[] { typeof(PhenotypeAsset) });
        private static bool _warnedCachePhenotype;

        public static bool ContainsPhenotypeIndex(Subspecies subspecies, int index)
        {
            if (subspecies == null) return false;
            if (PhenotypeIndexesField == null)
            {
                Debug.LogWarning("[XN] Subspecies._phenotypes_set_indexes field not found.");
                return false;
            }
            var indexes = PhenotypeIndexesField.GetValue(subspecies) as ICollection<int>;
            return indexes != null && indexes.Contains(index);
        }

        public static void CachePhenotype(Subspecies subspecies, PhenotypeAsset phenotype)
        {
            if (subspecies == null || phenotype == null) return;
            if (CachePhenotypeMethod == null)
            {
                WarnOnce(ref _warnedCachePhenotype, "[XN] Subspecies.cachePhenotype method not found; phenotype cache was not updated.");
                return;
            }
            CachePhenotypeMethod.Invoke(subspecies, new object[] { phenotype });
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
