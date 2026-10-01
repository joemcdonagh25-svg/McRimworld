using Verse;

namespace TheArk
{
    [StaticConstructorOnStartup]
    public static class TheArkBootstrap
    {
        static TheArkBootstrap()
        {
            Log.Message("[The Ark] Initialised successfully.");
        }
    }
}
