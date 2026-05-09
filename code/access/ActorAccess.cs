using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace xn.access
{
    internal static class ActorAccess
    {
        private static readonly FieldInfo LastAttackTypeField = AccessTools.Field(typeof(Actor), "_last_attack_type");
        private static readonly FieldInfo FlyingField = AccessTools.Field(typeof(Actor), "_flying");
        private static readonly FieldInfo LastColoredSpriteField = AccessTools.Field(typeof(Actor), "_last_colored_sprite");
        private static readonly FieldInfo IsVisibleField = AccessTools.Field(typeof(Actor), "is_visible");
        private static readonly FieldInfo DataField = AccessTools.Field(typeof(Actor), "data");
        private static readonly FieldInfo HasAttackTargetField = AccessTools.Field(typeof(Actor), "has_attack_target");
        private static readonly FieldInfo AIField = AccessTools.Field(typeof(Actor), "ai");
        private static readonly FieldInfo IsInsideBuildingField = AccessTools.Field(typeof(Actor), "is_inside_building");
        private static readonly FieldInfo InsideBuildingField = AccessTools.Field(typeof(Actor), "inside_building");
        private static readonly FieldInfo IsInsideBoatField = AccessTools.Field(typeof(Actor), "is_inside_boat");
        private static readonly FieldInfo InsideBoatField = AccessTools.Field(typeof(Actor), "inside_boat");
        private static readonly FieldInfo AttackedByField = AccessTools.Field(typeof(Actor), "attackedBy");
        private static readonly FieldInfo AttackTargetField = AccessTools.Field(typeof(Actor), "attack_target");
        private static readonly FieldInfo BehActorTargetField = AccessTools.Field(typeof(Actor), "beh_actor_target");
        private static readonly FieldInfo TileTargetField = AccessTools.Field(typeof(Actor), "tile_target");
        private static readonly FieldInfo ActorScaleField = AccessTools.Field(typeof(Actor), "actor_scale");
        private static readonly FieldInfo TimerActionField = AccessTools.Field(typeof(Actor), "timer_action");
        private static readonly MethodInfo IsFlyingMethod = AccessTools.Method(typeof(Actor), "isFlying");
        private static readonly MethodInfo IsUsingPathMethod = AccessTools.Method(typeof(Actor), "isUsingPath");
        private static readonly MethodInfo IsAttackPossibleMethod = AccessTools.Method(typeof(Actor), "isAttackPossible");
        private static readonly MethodInfo IsInAttackRangeMethod = AccessTools.Method(typeof(Actor), "isInAttackRange", new[] { typeof(BaseSimObject) });
        private static readonly MethodInfo IsProfessionMethod = AccessTools.Method(typeof(Actor), "isProfession", new[] { typeof(UnitProfession) });
        private static readonly MethodInfo CalculateForceMethod = AccessTools.Method(typeof(Actor), "calculateForce", new[] { typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(bool) });
        private static readonly MethodInfo SetCityMethod = AccessTools.Method(typeof(Actor), "setCity", new[] { typeof(City) });
        private static readonly MethodInfo SetKingdomMethod = AccessTools.Method(typeof(Actor), "setKingdom", new[] { typeof(Kingdom) });
        private static readonly MethodInfo SetCurrentTilePositionMethod = AccessTools.Method(typeof(Actor), "setCurrentTilePosition", new[] { typeof(WorldTile) });
        private static readonly MethodInfo SetProfessionMethod = AccessTools.Method(typeof(Actor), "setProfession", new[] { typeof(UnitProfession), typeof(bool) });
        private static readonly MethodInfo SpawnOnMethod = AccessTools.Method(typeof(Actor), "spawnOn", new[] { typeof(WorldTile), typeof(float) });
        private static bool _warnedIsVisible;
        private static bool _warnedData;
        private static bool _warnedHasAttackTarget;
        private static bool _warnedAI;
        private static bool _warnedIsInsideBuilding;
        private static bool _warnedInsideBuilding;
        private static bool _warnedIsInsideBoat;
        private static bool _warnedInsideBoat;
        private static bool _warnedAttackedBy;
        private static bool _warnedAttackTarget;
        private static bool _warnedBehActorTarget;
        private static bool _warnedTileTarget;
        private static bool _warnedActorScale;
        private static bool _warnedTimerAction;
        private static bool _warnedIsFlying;
        private static bool _warnedIsUsingPath;
        private static bool _warnedIsAttackPossible;
        private static bool _warnedIsInAttackRange;
        private static bool _warnedIsProfession;
        private static bool _warnedCalculateForce;
        private static bool _warnedSetCity;
        private static bool _warnedSetKingdom;
        private static bool _warnedSetCurrentTilePosition;
        private static bool _warnedSetProfession;
        private static bool _warnedSpawnOn;

        public static void SetLastAttackType(Actor actor, AttackType attackType)
        {
            if (actor == null) return;
            if (LastAttackTypeField == null)
            {
                Debug.LogWarning("[XN] Actor._last_attack_type field not found; attack type was not set.");
                return;
            }
            LastAttackTypeField.SetValue(actor, attackType);
        }

        public static bool IsFlyingRaw(Actor actor)
        {
            if (actor == null) return false;
            if (FlyingField == null)
            {
                WarnOnce(ref _warnedIsFlying, "[XN] Actor._flying field not found; treating actor as not flying.");
                return false;
            }
            return FlyingField.GetValue(actor) is bool value && value;
        }

        public static Sprite GetLastColoredSprite(Actor actor)
        {
            if (actor == null) return null;
            if (LastColoredSpriteField == null)
            {
                Debug.LogWarning("[XN] Actor._last_colored_sprite field not found; using asset icon.");
                return null;
            }
            return LastColoredSpriteField.GetValue(actor) as Sprite;
        }

        public static bool IsVisible(Actor actor)
        {
            if (actor == null) return false;
            if (IsVisibleField == null)
            {
                WarnOnce(ref _warnedIsVisible, "[XN] Actor.is_visible field not found; treating actor as visible.");
                return true;
            }
            return IsVisibleField.GetValue(actor) is bool value && value;
        }

        public static ActorData GetData(Actor actor)
        {
            if (actor == null) return null;
            if (DataField == null)
            {
                WarnOnce(ref _warnedData, "[XN] Actor.data field not found; actor data lookup failed.");
                return null;
            }
            return DataField.GetValue(actor) as ActorData;
        }

        public static bool HasAttackTarget(Actor actor)
        {
            if (actor == null) return false;
            if (HasAttackTargetField == null)
            {
                WarnOnce(ref _warnedHasAttackTarget, "[XN] Actor.has_attack_target field not found; treating actor as not in attack target state.");
                return false;
            }
            return HasAttackTargetField.GetValue(actor) is bool value && value;
        }

        public static AiSystemActor GetAI(Actor actor)
        {
            if (actor == null) return null;
            if (AIField == null)
            {
                WarnOnce(ref _warnedAI, "[XN] Actor.ai field not found; actor AI lookup failed.");
                return null;
            }
            return AIField.GetValue(actor) as AiSystemActor;
        }

        public static bool IsInsideBuilding(Actor actor)
        {
            if (actor == null) return false;
            if (IsInsideBuildingField == null)
            {
                WarnOnce(ref _warnedIsInsideBuilding, "[XN] Actor.is_inside_building field not found; treating actor as outside building.");
                return false;
            }
            return IsInsideBuildingField.GetValue(actor) is bool value && value;
        }

        public static Building GetInsideBuilding(Actor actor)
        {
            if (actor == null) return null;
            if (InsideBuildingField == null)
            {
                WarnOnce(ref _warnedInsideBuilding, "[XN] Actor.inside_building field not found; inside building lookup failed.");
                return null;
            }
            return InsideBuildingField.GetValue(actor) as Building;
        }

        public static void SetIsInsideBuilding(Actor actor, bool value)
        {
            if (actor == null) return;
            if (IsInsideBuildingField == null)
            {
                WarnOnce(ref _warnedIsInsideBuilding, "[XN] Actor.is_inside_building field not found; inside-building flag was not changed.");
                return;
            }
            IsInsideBuildingField.SetValue(actor, value);
        }

        public static bool IsInsideBoat(Actor actor)
        {
            if (actor == null) return false;
            if (IsInsideBoatField == null)
            {
                WarnOnce(ref _warnedIsInsideBoat, "[XN] Actor.is_inside_boat field not found; treating actor as outside boat.");
                return false;
            }
            return IsInsideBoatField.GetValue(actor) is bool value && value;
        }

        public static void SetIsInsideBoat(Actor actor, bool value)
        {
            if (actor == null) return;
            if (IsInsideBoatField == null)
            {
                WarnOnce(ref _warnedIsInsideBoat, "[XN] Actor.is_inside_boat field not found; inside-boat flag was not changed.");
                return;
            }
            IsInsideBoatField.SetValue(actor, value);
        }

        public static Boat GetInsideBoat(Actor actor)
        {
            if (actor == null) return null;
            if (InsideBoatField == null)
            {
                WarnOnce(ref _warnedInsideBoat, "[XN] Actor.inside_boat field not found; inside boat lookup failed.");
                return null;
            }
            return InsideBoatField.GetValue(actor) as Boat;
        }

        public static void SetInsideBoat(Actor actor, Boat value)
        {
            if (actor == null) return;
            if (InsideBoatField == null)
            {
                WarnOnce(ref _warnedInsideBoat, "[XN] Actor.inside_boat field not found; inside boat was not changed.");
                return;
            }
            InsideBoatField.SetValue(actor, value);
        }

        public static BaseSimObject GetAttackedBy(Actor actor)
        {
            if (actor == null) return null;
            if (AttackedByField == null)
            {
                WarnOnce(ref _warnedAttackedBy, "[XN] Actor.attackedBy field not found; attacker lookup failed.");
                return null;
            }
            return AttackedByField.GetValue(actor) as BaseSimObject;
        }

        public static void SetAttackedBy(Actor actor, BaseSimObject value)
        {
            if (actor == null) return;
            if (AttackedByField == null)
            {
                WarnOnce(ref _warnedAttackedBy, "[XN] Actor.attackedBy field not found; attacker was not changed.");
                return;
            }
            AttackedByField.SetValue(actor, value);
        }

        public static BaseSimObject GetAttackTarget(Actor actor)
        {
            if (actor == null) return null;
            if (AttackTargetField == null)
            {
                WarnOnce(ref _warnedAttackTarget, "[XN] Actor.attack_target field not found; attack target lookup failed.");
                return null;
            }
            return AttackTargetField.GetValue(actor) as BaseSimObject;
        }

        public static void SetAttackTarget(Actor actor, BaseSimObject value)
        {
            if (actor == null) return;
            if (AttackTargetField == null)
            {
                WarnOnce(ref _warnedAttackTarget, "[XN] Actor.attack_target field not found; attack target was not changed.");
                return;
            }
            AttackTargetField.SetValue(actor, value);
        }

        public static BaseSimObject GetBehActorTarget(Actor actor)
        {
            if (actor == null) return null;
            if (BehActorTargetField == null)
            {
                WarnOnce(ref _warnedBehActorTarget, "[XN] Actor.beh_actor_target field not found; behaviour target lookup failed.");
                return null;
            }
            return BehActorTargetField.GetValue(actor) as BaseSimObject;
        }

        public static void SetBehActorTarget(Actor actor, BaseSimObject value)
        {
            if (actor == null) return;
            if (BehActorTargetField == null)
            {
                WarnOnce(ref _warnedBehActorTarget, "[XN] Actor.beh_actor_target field not found; behaviour target was not changed.");
                return;
            }
            BehActorTargetField.SetValue(actor, value);
        }

        public static WorldTile GetTileTarget(Actor actor)
        {
            if (actor == null) return null;
            if (TileTargetField == null)
            {
                WarnOnce(ref _warnedTileTarget, "[XN] Actor.tile_target field not found; tile target lookup failed.");
                return null;
            }
            return TileTargetField.GetValue(actor) as WorldTile;
        }

        public static float GetActorScale(Actor actor)
        {
            if (actor == null) return 1f;
            if (ActorScaleField == null)
            {
                WarnOnce(ref _warnedActorScale, "[XN] Actor.actor_scale field not found; using scale 1.");
                return 1f;
            }
            return ActorScaleField.GetValue(actor) is float value ? value : 1f;
        }

        public static void SetTimerAction(Actor actor, float value)
        {
            if (actor == null) return;
            if (TimerActionField == null)
            {
                WarnOnce(ref _warnedTimerAction, "[XN] Actor.timer_action field not found; timer action was not changed.");
                return;
            }
            TimerActionField.SetValue(actor, value);
        }

        public static bool IsFlying(Actor actor)
        {
            if (actor == null) return false;
            if (IsFlyingMethod == null)
            {
                WarnOnce(ref _warnedIsFlying, "[XN] Actor.isFlying method not found; treating actor as not flying.");
                return false;
            }
            return IsFlyingMethod.Invoke(actor, null) is bool value && value;
        }

        public static bool IsUsingPath(Actor actor)
        {
            if (actor == null) return false;
            if (IsUsingPathMethod == null)
            {
                WarnOnce(ref _warnedIsUsingPath, "[XN] Actor.isUsingPath method not found; treating actor as not using a path.");
                return false;
            }
            return IsUsingPathMethod.Invoke(actor, null) is bool value && value;
        }

        public static bool IsAttackPossible(Actor actor)
        {
            if (actor == null) return false;
            if (IsAttackPossibleMethod == null)
            {
                WarnOnce(ref _warnedIsAttackPossible, "[XN] Actor.isAttackPossible method not found; treating attack as impossible.");
                return false;
            }
            return IsAttackPossibleMethod.Invoke(actor, null) is bool value && value;
        }

        public static bool IsInAttackRange(Actor actor, BaseSimObject target)
        {
            if (actor == null || target == null) return false;
            if (IsInAttackRangeMethod == null)
            {
                WarnOnce(ref _warnedIsInAttackRange, "[XN] Actor.isInAttackRange method not found; treating target as out of range.");
                return false;
            }
            return IsInAttackRangeMethod.Invoke(actor, new object[] { target }) is bool value && value;
        }

        public static bool IsProfession(Actor actor, UnitProfession profession)
        {
            if (actor == null) return false;
            if (IsProfessionMethod == null)
            {
                WarnOnce(ref _warnedIsProfession, "[XN] Actor.isProfession method not found; checking actor data profession.");
                var data = GetData(actor);
                return data != null && data.profession == profession;
            }
            return IsProfessionMethod.Invoke(actor, new object[] { profession }) is bool value && value;
        }

        public static void CalculateForce(Actor actor, float startX, float startY, float targetX, float targetY, float forceAmountDirection, float forceHeight, bool checkCancelJobOnLand)
        {
            if (actor == null) return;
            if (CalculateForceMethod == null)
            {
                WarnOnce(ref _warnedCalculateForce, "[XN] Actor.calculateForce method not found; force was not applied.");
                return;
            }
            CalculateForceMethod.Invoke(actor, new object[] { startX, startY, targetX, targetY, forceAmountDirection, forceHeight, checkCancelJobOnLand });
        }

        public static void SetCity(Actor actor, City city)
        {
            if (actor == null) return;
            if (SetCityMethod == null)
            {
                WarnOnce(ref _warnedSetCity, "[XN] Actor.setCity method not found; city was not changed.");
                return;
            }
            SetCityMethod.Invoke(actor, new object[] { city });
        }

        public static void SetKingdom(Actor actor, Kingdom kingdom)
        {
            if (actor == null) return;
            if (SetKingdomMethod == null)
            {
                WarnOnce(ref _warnedSetKingdom, "[XN] Actor.setKingdom method not found; kingdom was not changed.");
                return;
            }
            SetKingdomMethod.Invoke(actor, new object[] { kingdom });
        }

        public static void SetCurrentTilePosition(Actor actor, WorldTile tile)
        {
            if (actor == null || tile == null) return;
            if (SetCurrentTilePositionMethod == null)
            {
                WarnOnce(ref _warnedSetCurrentTilePosition, "[XN] Actor.setCurrentTilePosition method not found; tile position was not changed.");
                return;
            }
            SetCurrentTilePositionMethod.Invoke(actor, new object[] { tile });
        }

        public static void SetProfession(Actor actor, UnitProfession profession, bool cancelBeh = true)
        {
            if (actor == null) return;
            if (SetProfessionMethod == null)
            {
                WarnOnce(ref _warnedSetProfession, "[XN] Actor.setProfession method not found; profession was not changed.");
                return;
            }
            SetProfessionMethod.Invoke(actor, new object[] { profession, cancelBeh });
        }

        public static void SpawnOn(Actor actor, WorldTile tile, float zHeight = 0f)
        {
            if (actor == null || tile == null) return;
            if (SpawnOnMethod == null)
            {
                WarnOnce(ref _warnedSpawnOn, "[XN] Actor.spawnOn method not found; actor was not moved.");
                return;
            }
            SpawnOnMethod.Invoke(actor, new object[] { tile, zHeight });
        }

        private static void WarnOnce(ref bool warned, string message)
        {
            if (warned) return;
            warned = true;
            Debug.LogWarning(message);
        }
    }
}
