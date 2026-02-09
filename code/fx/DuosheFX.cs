using xn.fx;
public static class DuoSheFX
{
    public static void Register() => XNStatusFX.Register();
    public static float GetDuration() => 1f;
    public static void PlayOnce(Actor a) => XNStatusFX.PlayDuoshe(a);
}