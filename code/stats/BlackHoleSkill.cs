using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.config;
using xn.world;
namespace cultivation
{
    internal static class BlackHoleSkill
    {
        private const string SPRITE_PATH = "tiandao";
        private const string FX_BLACKHOLE = "fx_blackhole_skill";
        private const string KEY_NIELI = "xn.stat.nieli";
        private const string KEY_SUPPRESSED = "xn.tatian.suppressed";
        private const string KEY_BLACKHOLE_CD = "xn.blackhole.cooldown";
        private const string KEY_BLACKHOLE_ACTIVE = "xn.blackhole.active";
        private const float COOLDOWN = 30f;           
        private const int MIN_SUPPRESSED = 5;         
        private const int MIN_NIELI = 6000;           
        private const int TOTAL_FRAMES = 32;          
        private const float FRAME_TIME = 0.15f;       
        private const float PULL_RANGE = 10f;         
        private const float EFFECT_SCALE = 1.2f;      
        private const float EFFECT_Y_OFFSET = -2f;   
        private const int DAMAGE_FRAME = 28;          
        private static bool s_registered;
        private class BlackHoleData
        {
            public Actor caster;
            public Actor targetActor;                 
            public WorldTile centerTile;
            public int casterNieli;               
            public BaseEffect effect;
            public List<Actor> targets = new List<Actor>();
            public int currentFrame;
            public float nextFrameTime;
            public bool damageDealt;              
        }
        private static readonly List<BlackHoleData> s_activeBlackHoles = new List<BlackHoleData>();
        public static void Register()
        {
            if (s_registered) return;
            var lib = AssetManager.effects_library;
            if (!lib.has(FX_BLACKHOLE))
            {
                lib.add(new EffectAsset
                {
                    id = FX_BLACKHOLE,
                    use_basic_prefab = true,
                    sorting_layer_id = "EffectsTop",
                    sprite_path = SPRITE_PATH,
                    time_between_frames = FRAME_TIME,
                    limit = 32
                });
            }
            s_registered = true;
        }
        public static bool TryTrigger(Actor suppressor, List<Actor> suppressedTargets)
        {
            if (suppressor == null || !suppressor.isAlive()) return false;
            if (suppressedTargets == null || suppressedTargets.Count < MIN_SUPPRESSED)
            {
                return false;
            }
            bool isTatian = suppressor.hasTrait("realm_16_tatian");
            bool isTianzun = suppressor.hasTrait("realm_14_gtianzun");
            if (!isTatian && !isTianzun)
            {
                return false;
            }
            int nieli;
            xn.access.ActorAccess.GetData(suppressor).get(KEY_NIELI, out nieli, 0);
            if (nieli < MIN_NIELI)
            {
                return false;
            }
            float lastCd;
            xn.access.ActorAccess.GetData(suppressor).get(KEY_BLACKHOLE_CD, out lastCd, 0f);
            if (Time.time - lastCd < COOLDOWN) return false;
            int active;
            xn.access.ActorAccess.GetData(suppressor).get(KEY_BLACKHOLE_ACTIVE, out active, 0);
            if (active == 1) return false;
            Actor targetActor = FindTargetActor(suppressedTargets);
            if (targetActor == null) return false;
            WorldTile centerTile = targetActor.current_tile;
            if (centerTile == null) return false;
            BroadcastSystem.PostActor(suppressor, suppressor.getName() + " 发动了天劫之力绞杀余孽");
            StartBlackHole(suppressor, targetActor, suppressedTargets, nieli);
            return true;
        }
        private static Actor FindTargetActor(List<Actor> targets)
        {
            if (targets == null || targets.Count == 0) return null;
            foreach (var t in targets)
            {
                if (t != null && t.isAlive() && t.current_tile != null)
                {
                    return t;
                }
            }
            return null;
        }
        private static void StartBlackHole(Actor caster, Actor targetActor, List<Actor> targets, int casterNieli)
        {
            Register();
            xn.access.ActorAccess.GetData(caster).set(KEY_BLACKHOLE_CD, Time.time);
            xn.access.ActorAccess.GetData(caster).set(KEY_BLACKHOLE_ACTIVE, 1);
            xn.access.ActorAccess.GetData(caster).set(KEY_NIELI, casterNieli / 2);
            if (caster.is_moving) caster.stopMovement();
            if (xn.access.ActorAccess.HasAttackTarget(caster)) caster.clearAttackTarget();
            caster.cancelAllBeh();
            WorldTile centerTile = targetActor.current_tile;
            float effectScale = xn.access.ActorAccess.GetActorScale(targetActor);
            Vector2 pos = new Vector2(centerTile.pos.x, centerTile.pos.y + EFFECT_Y_OFFSET);
            var effect = EffectsLibrary.spawnAt(FX_BLACKHOLE, pos, effectScale);
            if (effect != null)
            {
                var anim = effect.sprite_animation;
                var sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = null; 
                if (anim != null)
                {
                    var asset = AssetManager.effects_library.get(FX_BLACKHOLE);
                    if (asset != null && !string.IsNullOrEmpty(asset.sprite_path))
                    {
                        Sprite[] frames = SpriteTextureLoader.getSpriteList(asset.sprite_path);
                        if (frames != null && frames.Length > 0)
                        {
                            anim.setFrames(frames);
                            anim.timeBetweenFrames = asset.time_between_frames;
                        }
                    }
                    anim.looped = false;
                }
            }
            else
            {
                Debug.LogWarning("[BlackHoleSkill] Failed to spawn effect!");
            }
            var data = new BlackHoleData
            {
                caster = caster,
                targetActor = targetActor,
                centerTile = centerTile,
                casterNieli = casterNieli,
                effect = effect,
                currentFrame = 0,
                nextFrameTime = Time.time + FRAME_TIME,
                damageDealt = false
            };
            data.targets.AddRange(targets);
            s_activeBlackHoles.Add(data);
        }
        public static void Update()
        {
            if (s_activeBlackHoles.Count == 0) return;
            float now = Time.time;
            List<BlackHoleData> toRemove = null;
            for (int i = 0; i < s_activeBlackHoles.Count; i++)
            {
                var data = s_activeBlackHoles[i];
                if (data == null) continue;
                if (data.caster == null || !data.caster.isAlive())
                {
                    EndBlackHole(data);
                    if (toRemove == null) toRemove = new List<BlackHoleData>();
                    toRemove.Add(data);
                    continue;
                }
                if (data.caster.is_moving) data.caster.stopMovement();
                if (xn.access.ActorAccess.HasAttackTarget(data.caster)) data.caster.clearAttackTarget();
                PullTargets(data);
                if (data.currentFrame >= DAMAGE_FRAME && !data.damageDealt)
                {
                    DealFinalDamage(data);
                    data.damageDealt = true;
                }
                if (data.currentFrame >= TOTAL_FRAMES)
                {
                    EndBlackHole(data);
                    if (toRemove == null) toRemove = new List<BlackHoleData>();
                    toRemove.Add(data);
                    continue;
                }
                if (now >= data.nextFrameTime)
                {
                    data.currentFrame++;
                    data.nextFrameTime = now + FRAME_TIME;
                }
            }
            if (toRemove != null)
            {
                foreach (var d in toRemove)
                    s_activeBlackHoles.Remove(d);
            }
        }
        private static void PullTargets(BlackHoleData data)
        {
            if (data == null || data.centerTile == null) return;
            Vector2 center = new Vector2(data.centerTile.pos.x, data.centerTile.pos.y);
            var nearbyUnits = Finder.getUnitsFromChunk(data.centerTile, 2, PULL_RANGE);
            foreach (var unit in nearbyUnits)
            {
                if (unit == null || !unit.isAlive()) continue;
                if (unit == data.caster) continue;
                int suppressed;
                xn.access.ActorAccess.GetData(unit).get(KEY_SUPPRESSED, out suppressed, 0);
                if (suppressed != 1) continue;
                if (!data.targets.Contains(unit))
                    data.targets.Add(unit);
            }
            for (int i = data.targets.Count - 1; i >= 0; i--)
            {
                var target = data.targets[i];
                if (target == null || !target.isAlive())
                {
                    data.targets.RemoveAt(i);
                    continue;
                }
                Vector2 targetPos = target.current_position;
                Vector2 dir = center - targetPos;
                float dist = dir.magnitude;
                if (dist > 0.3f)
                {
                    float progress = (float)data.currentFrame / TOTAL_FRAMES;
                    float pullStrength = Mathf.Lerp(0.5f, 3f, progress);
                    dir.Normalize();
                    target.calculateForce(
                        target.current_tile.x, target.current_tile.y,
                        target.current_tile.x - (int)(dir.x * pullStrength),
                        target.current_tile.y - (int)(dir.y * pullStrength),
                        pullStrength, 0f, false);
                }
            }
        }
        private static readonly string[] REALM_IDS = {
            "realm_01_qi", "realm_02_foundation", "realm_03_core", "realm_04_nascent",
            "realm_05_deity", "realm_06_infantchg", "realm_07_wending", "realm_08_kuinie",
            "realm_09_jingnie", "realm_10_suinie", "realm_11_kongnie", "realm_12_kongling",
            "realm_13_kongxuan", "realm_14_gtianzun", "realm_15_half_tatian", "realm_16_tatian"
        };
        private static readonly string[] ANC_STAR_IDS = {
            "ancient_01_star", "ancient_02_star", "ancient_03_star", "ancient_04_star", "ancient_05_star",
            "ancient_06_star", "ancient_07_star", "ancient_08_star", "ancient_09_star", "ancient_10_star"
        };
        private static readonly string[] BEAST_STAGE_IDS = {
            "beast_01_stage", "beast_02_stage", "beast_03_stage", "beast_04_stage", "beast_05_stage",
            "beast_06_stage", "beast_07_stage", "beast_08_stage", "beast_09_stage", "beast_10_stage"
        };
        private static bool HasAnyRealm(Actor a)
        {
            if (a == null) return false;
            foreach (var id in REALM_IDS) if (a.hasTrait(id)) return true;
            foreach (var id in ANC_STAR_IDS) if (a.hasTrait(id)) return true;
            foreach (var id in BEAST_STAGE_IDS) if (a.hasTrait(id)) return true;
            return false;
        }
        private static void DealFinalDamage(BlackHoleData data)
        {
            if (data == null || data.caster == null) return;
            int multiplier = UnityEngine.Random.Range(1, 21);
            int damage = data.casterNieli * multiplier;
            for (int i = data.targets.Count - 1; i >= 0; i--)
            {
                var target = data.targets[i];
                if (target == null || !target.isAlive()) continue;
                if (!HasAnyRealm(target))
                {
                    if (data.caster != null && data.caster.isAlive())
                    {
                        data.caster.data.kills++;
                    }
                    target.startColorEffect(ActorColorEffect.Red);
                    World.world.units.scheduleDestroyOnPlay(target);
                    continue;
                }
                xn.access.ActorAccess.GetData(target).health = Mathf.Max(0, xn.access.ActorAccess.GetData(target).health - damage);
                target.startColorEffect(ActorColorEffect.Red);
                if (!target.hasHealth())
                {
                    if (target.batch != null)
                    {
                        target.batch.c_check_deaths.Add(target);
                        if (data.caster != null && data.caster.isAlive())
                        {
                            data.caster.data.kills++;
                        }
                    }
                }
            }
        }
        private static void EndBlackHole(BlackHoleData data)
        {
            if (data == null) return;
            if (data.effect != null)
            {
                data.effect.kill();
            }
            if (data.caster != null)
            {
                data.caster.data.set(KEY_BLACKHOLE_ACTIVE, 0);
            }
        }
        public static bool IsCasting(Actor actor)
        {
            if (actor == null) return false;
            int active;
            xn.access.ActorAccess.GetData(actor).get(KEY_BLACKHOLE_ACTIVE, out active, 0);
            return active == 1;
        }
    }
}
