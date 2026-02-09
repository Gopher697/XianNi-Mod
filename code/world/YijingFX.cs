using xn.fx;
public static class YijingFX
{
    public static void Register() => XNStatusFX.Register();
    public static void PlayExtremeOnce(Actor target) => XNStatusFX.PlayJijing(target);
    public static void StartLoop(Actor a) => XNStatusFX.StartYijingLoop(a);
    public static void StopLoop(Actor a) => XNStatusFX.StopYijingLoop(a);
}