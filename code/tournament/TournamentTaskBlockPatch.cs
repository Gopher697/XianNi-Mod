using System;
using HarmonyLib;
namespace xn.tournament
{
    [HarmonyPatch(typeof(Actor), "setTask", typeof(string), typeof(bool), typeof(bool), typeof(bool))]
    internal static class TournamentTaskBlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance, string pTaskId)
        {
            if (__instance == null || string.IsNullOrEmpty(pTaskId)) return true;
            if (!TournamentManager.IsRunning) return true;
            if (!TournamentManager.IsParticipant(__instance)) return true;
            switch (pTaskId)
            {
                case "task_xn_breakthrough_stay":      
                case "task_xn_condense_root_stay":     
                case "task_xn_intent_comprehend_stay": 
                case "task_xn_demonic_hunt":           
                case "task_xn_tianyunzi_hunt":         
                case "force_into_a_boat":             
                case "random_teleport":               
                case "teleport_back_home":            
                    return false; 
                default:
                    return true;
            }
        }
    }
    [HarmonyPatch(typeof(Actor), "getNextJob")]
    internal static class TournamentJobBlockPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Actor __instance, ref string __result)
        {
            if (__instance == null) return;
            if (string.IsNullOrEmpty(__result)) return;
            if (!TournamentManager.IsRunning) return;
            if (!TournamentManager.IsParticipant(__instance)) return;
            switch (__result)
            {
                case "job_xn_breakthrough":        
                case "job_xn_condense_root":       
                case "job_xn_intent_comprehend":   
                case "job_xn_demonic_hunt":        
                case "job_xn_tianyunzi":           
                    __result = null; 
                    break;
            }
        }
    }
    [HarmonyPatch(typeof(BaseSystemData), "set", typeof(string), typeof(int))]
    internal static class TournamentTrialBlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(BaseSystemData __instance, string pKey, int pData)
        {
            if (__instance == null) return true;
            if (pKey != "xn.trial.active" || pData != 1) return true;
            if (!TournamentManager.IsRunning) return true;
            var actorData = __instance as ActorData;
            if (actorData == null) return true;
            var actor = TournamentManager.GetActorById(actorData.id.ToString());
            if (actor == null) return true;
            if (!TournamentManager.IsParticipant(actor)) return true;
            return false;
        }
    }
    [HarmonyPatch(typeof(Actor), "die", typeof(bool), typeof(AttackType), typeof(bool), typeof(bool))]
    internal static class TournamentDeathBlockPatch
    {
        [HarmonyPriority(Priority.First)] 
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance)
        {
            if (__instance == null) return true;
            if (!TournamentManager.IsRunning) return true;
            if (!TournamentManager.IsParticipant(__instance)) return true;
            var currentMatch = TournamentManager.GetCurrentMatch(__instance);
            if (currentMatch != null && currentMatch.IsDeathMatch)
            {
                return true; 
            }
            TournamentManager.RecordDeathTrigger(__instance);
            xn.access.ActorAccess.GetData(__instance).health = (int)(__instance.getMaxHealth() * 0.2f);
            if (xn.access.ActorAccess.GetData(__instance).health < 1) xn.access.ActorAccess.GetData(__instance).health = 1;
            return false; 
        }
    }
    [HarmonyPatch(typeof(Boat), "addPassenger")]
    internal static class TournamentBoatBlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor)
        {
            if (pActor == null) return true;
            if (!TournamentManager.IsRunning) return true;
            if (!TournamentManager.IsParticipant(pActor)) return true;
            return false; 
        }
    }
    [HarmonyPatch(typeof(Actor), "embarkInto")]
    internal static class TournamentEmbarkBlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor __instance)
        {
            if (__instance == null) return true;
            if (!TournamentManager.IsRunning) return true;
            if (!TournamentManager.IsParticipant(__instance)) return true;
            return false; 
        }
    }
    [HarmonyPatch(typeof(BaseSimObject), "addStatusEffect", typeof(string), typeof(float), typeof(bool))]
    internal static class TournamentSleepStatusBlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(BaseSimObject __instance, string pID)
        {
            if (pID != "sleeping") return true;
            if (!TournamentManager.IsRunning) return true;
            var actor = __instance as Actor;
            if (actor == null) return true;
            if (!TournamentManager.IsParticipant(actor)) return true;
            return false; 
        }
    }
    [HarmonyPatch(typeof(ActionLibrary), "teleportRandom", new Type[] {
        typeof(BaseSimObject), typeof(BaseSimObject), typeof(WorldTile)
    })]
    internal static class TournamentTeleportRandomBlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(BaseSimObject pTarget)
        {
            if (pTarget == null) return true;
            if (!TournamentManager.IsRunning) return true;
            var actor = pTarget as Actor;
            if (actor == null) return true;
            if (!TournamentManager.IsParticipant(actor)) return true;
            return false; 
        }
    }
    [HarmonyPatch(typeof(ActionLibrary), "singularityTeleportation", new Type[] {
        typeof(WorldTile), typeof(Actor)
    })]
    internal static class TournamentSingularityTeleportBlockPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Actor pActor)
        {
            if (pActor == null) return true;
            if (!TournamentManager.IsRunning) return true;
            if (!TournamentManager.IsParticipant(pActor)) return true;
            return false; 
        }
    }
}