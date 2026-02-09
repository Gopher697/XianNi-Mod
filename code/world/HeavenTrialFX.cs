namespace xn.world
{
    public static class HeavenTrialFX
    {
        public static void Register() => fx.XNStatusFX.Register();
        public static void StartFor(Actor a) => fx.XNStatusFX.StartTiandao(a);
        public static void StopFor(Actor a) => fx.XNStatusFX.StopTiandao(a);
        public static void Tick(Actor _) {  }
    }
}