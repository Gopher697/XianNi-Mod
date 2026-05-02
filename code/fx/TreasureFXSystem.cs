using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using xn.assets;   
namespace xn.fx
{
    internal static class TreasureFXSystem
    {
        private const string ITEM_ID_PREFIX = "xn_treasure_"; 
        private const string SPRITE_PREFIX = "sutras/";
        private const float DEFAULT_SCALE = 0.20f; 
        private const float DEFAULT_OX = 0f;
        private const float DEFAULT_OY = 0f; 
        private struct OverrideEntry { public float scale, ox, oy; }
        private static readonly Dictionary<string, OverrideEntry> s_overrides = new Dictionary<string, OverrideEntry>(16);
        private struct TreasureEntry { public Sprite sprite; public string shortId; }
        private static readonly Dictionary<long, TreasureEntry> s_treasures = new Dictionary<long, TreasureEntry>(256);
        public static void SetOverride(string shortId, float scale, float offsetX, float offsetY)
        {
            OverrideEntry e; e.scale = scale; e.ox = offsetX; e.oy = offsetY;
            s_overrides[shortId] = e;
        }
        public static void RemoveOverride(string shortId) { s_overrides.Remove(shortId); }
        public static void ClearOverrides() { s_overrides.Clear(); }
        private static float GetScale(string shortId)
        {
            OverrideEntry e; return s_overrides.TryGetValue(shortId, out e) ? e.scale : DEFAULT_SCALE;
        }
        private static void GetOffset(string shortId, out float ox, out float oy)
        {
            OverrideEntry e;
            if (s_overrides.TryGetValue(shortId, out e)) { ox = e.ox; oy = e.oy; return; }
            ox = DEFAULT_OX; oy = DEFAULT_OY;
        }
        private static Sprite GetTreasureSprite(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return null;
            if (!actor.hasEquipment()) return null;
            var slot = actor.equipment.getSlot((EquipmentType)XNTreasureDefs.EQUIP_TYPE_TREASURE_INT);
            if (slot == null || slot.isEmpty()) return null;
            var item = slot.getItem();
            if (item == null) return null;
            var asset = item.getAsset();
            if (asset == null || string.IsNullOrEmpty(asset.id)) return null;
            if (!asset.id.StartsWith(ITEM_ID_PREFIX)) return null;
            string shortId = asset.id.Substring(ITEM_ID_PREFIX.Length);
            long actorId = xn.access.ActorAccess.GetData(actor).id;
            TreasureEntry entry;
            if (s_treasures.TryGetValue(actorId, out entry) && entry.shortId == shortId && entry.sprite != null)
            {
                return entry.sprite;
            }
            string spritePath = SPRITE_PREFIX + shortId;
            Sprite sprite = SpriteTextureLoader.getSprite(spritePath);
            if (sprite == null)
            {
                var frames = SpriteTextureLoader.getSpriteList(spritePath);
                if (frames != null && frames.Length > 0)
                {
                    sprite = frames[0];
                }
            }
            if (sprite != null)
            {
                s_treasures[actorId] = new TreasureEntry { sprite = sprite, shortId = shortId };
            }
            return sprite;
        }
        private static void ClearCache()
        {
            s_treasures.Clear();
        }
        [HarmonyPatch(typeof(ActorManager), "precalculateRenderDataNormal")]
        private static class Patch_ActorManager_precalculateRenderDataNormal
        {
            static void Postfix(ActorManager __instance)
            {
                if (__instance == null) return;
                ActorRenderData render_data = __instance.render_data;
                if (render_data == null) return;
                int count = __instance.visible_units.count;
                Actor[] array = __instance.visible_units.array;
                if (array == null || count <= 0) return;
                for (int i = 0; i < count; i++)
                {
                    Actor actor = array[i];
                    if (actor == null || !actor.isAlive()) continue;
                    Sprite treasureSprite = GetTreasureSprite(actor);
                    if (treasureSprite == null) continue; 
                    AnimationFrameData frameData = actor.getAnimationFrameData();
                    if (frameData == null || !frameData.show_head) continue;
                    Vector3 currentScale = actor.current_scale;
                    Vector3 actorPos = actor.updatePos();
                    Vector3 rotation = actor.updateRotation();
                    Vector2 posHead = xn.access.AnimationFrameDataAccess.GetPosHead(frameData);
                    float headX = posHead.x;
                    float headY = posHead.y;
                    float posX = actorPos.x + headX * currentScale.x;
                    float posY = actorPos.y + headY * currentScale.y;
                    float posZ = -0.01f + headY * currentScale.y; 
                    Vector3 treasurePos = new Vector3(posX, posY, posZ);
                    if (rotation.y != 0f || rotation.z != 0f)
                    {
                        Vector3 pivot = new Vector3(actorPos.x, actorPos.y, 0f);
                        treasurePos = Toolbox.RotatePointAroundPivot(ref treasurePos, ref pivot, ref rotation);
                    }
                    if (!actor.hasEquipment()) continue;
                    var slot = actor.equipment.getSlot((EquipmentType)XNTreasureDefs.EQUIP_TYPE_TREASURE_INT);
                    if (slot == null || slot.isEmpty()) continue;
                    var item = slot.getItem();
                    if (item == null) continue;
                    var asset = item.getAsset();
                    if (asset == null || string.IsNullOrEmpty(asset.id)) continue;
                    if (!asset.id.StartsWith(ITEM_ID_PREFIX)) continue;
                    string shortId = asset.id.Substring(ITEM_ID_PREFIX.Length);
                    float scale = GetScale(shortId);
                    float ox, oy;
                    GetOffset(shortId, out ox, out oy);
                    treasurePos.x += ox * currentScale.x;
                    treasurePos.y += oy * currentScale.y;
                    render_data.has_item[i] = true;
                    render_data.item_sprites[i] = treasureSprite;
                    render_data.item_pos[i] = treasurePos;
                    render_data.item_scale[i] = currentScale * scale;
                }
            }
        }
        [HarmonyPatch(typeof(MapBox), "addLoadWorldCallbacks")]
        private static class Patch_MapBox_addLoadWorldCallbacks
        {
            static void Postfix() { ClearCache(); }
        }
        [HarmonyPatch(typeof(ActorEquipment), "setItem")]
        private static class Patch_ActorEquipment_setItem
        {
            static void Postfix(ActorEquipment __instance, Item pItem, Actor pActor)
            {
                if (pActor == null) return;
                long actorId = xn.access.ActorAccess.GetData(pActor).id;
                s_treasures.Remove(actorId); 
            }
        }
    }
}
