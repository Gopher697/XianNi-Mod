namespace xn.world
{
    public static class TerritoryFX
    {
        public static void Register() => fx.XNStatusFX.Register();
        public static void StartFor(Actor a) => fx.XNStatusFX.StartTerritory(a);
        public static void StopFor(Actor a) => fx.XNStatusFX.StopTerritory(a);
    }
}