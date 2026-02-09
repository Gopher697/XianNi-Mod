using xn.fx;
namespace xn.world
{
    public static class ShentongFX
    {
        public static void Register() => XNStatusFX.Register();
        public static BaseEffect PlayOnce_Sanmei(Actor target)
        {
            XNStatusFX.PlaySanmei(target);
            return null;
        }
        public static BaseEffect PlayOnce_Wanjian(WorldTile center, float scale)
        {
            XNStatusFX.PlayWanjian(center, scale);
            return null;
        }
        public static BaseEffect PlayOnce_Xuankongpo(Actor target)
        {
            XNStatusFX.PlayXuankongpo(target);
            return null;
        }
        public static BaseEffect PlayOnce_Zhenkongquan(Actor target)
        {
            XNStatusFX.PlayZhenkongquan(target);
            return null;
        }
        public static BaseEffect PlayOnce_Jiuyin(Actor target)
        {
            XNStatusFX.PlayJiuyin(target);
            return null;
        }
        public static BaseEffect PlayOnce_Duqi(WorldTile center, float scale = 1f)
        {
            XNStatusFX.PlayDuqi(center, scale);
            return null;
        }
        public static BaseEffect PlayOnce_Jianzhan(Actor target)
        {
            XNStatusFX.PlayJianzhan(target);
            return null;
        }
        public static BaseEffect PlayOnce_XS_Slash(Actor target)
        {
            XNStatusFX.PlayXsSlash(target);
            return null;
        }
        public static BaseEffect PlayOnce_XS_Quake(WorldTile center, float scale = 1f)
        {
            XNStatusFX.PlayXsQuake(center, scale);
            return null;
        }
        public static BaseEffect PlayOnce_XS_Waves(Actor caster)
        {
            XNStatusFX.PlayXsWaves(caster);
            return null;
        }
        public static BaseEffect PlayOnce_XS_Convert(WorldTile center, float scale = 1f)
        {
            XNStatusFX.PlayXsConvert(center, scale);
            return null;
        }
        public static BaseEffect PlayOnce_XS_Missile(Actor target)
        {
            XNStatusFX.PlayXsMissile(target);
            return null;
        }
        public static BaseEffect PlayOnce_XS_Palm(WorldTile center, float scale = 1f)
        {
            XNStatusFX.PlayXsPalm(center, scale);
            return null;
        }
        public static BaseEffect PlayOnce_XS_Breaker(Actor target)
        {
            XNStatusFX.PlayXsBreaker(target);
            return null;
        }
        public static BaseEffect PlayOnce_XS_Link(Actor unit)
        {
            XNStatusFX.PlayXsLink(unit);
            return null;
        }
        public static void StartLoop_Weiya(Actor target) => XNStatusFX.StartWeiya(target);
        public static void StopLoop_Weiya(Actor target) => XNStatusFX.StopWeiya(target);
        public static void StartLoop_Baonu(Actor caster) => XNStatusFX.StartBaonu(caster);
        public static void StopLoop_Baonu(Actor caster) => XNStatusFX.StopBaonu(caster);
        public static void StartLoop_XS_Ascension(Actor caster) => XNStatusFX.StartXsAscension(caster);
        public static void StopLoop_XS_Ascension(Actor caster) => XNStatusFX.StopXsAscension(caster);
        public static void StartLoop_XS_Shield(Actor caster) => XNStatusFX.StartXsShield(caster);
        public static void StopLoop_XS_Shield(Actor caster) => XNStatusFX.StopXsShield(caster);
    }
}