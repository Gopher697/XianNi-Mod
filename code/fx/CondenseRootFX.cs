namespace xn.fx
{
    public static class CondenseRootFX
    {
        public static void Register() => XNStatusFX.Register();
        public static void StartFor(Actor a) => XNStatusFX.StartCondenseRoot(a);
        public static void StopFor(Actor a) => XNStatusFX.StopCondenseRoot(a);
    }
}