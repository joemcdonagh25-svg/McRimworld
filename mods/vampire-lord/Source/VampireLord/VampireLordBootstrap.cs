using Verse;

namespace VampireLord
{
    /// <summary>
    /// Startup canary — proves the assembly loaded under RimWorld 1.6.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class VampireLordBootstrap
    {
        static VampireLordBootstrap()
        {
            Log.Message("[VampireLord] Initialised successfully.");
        }
    }
}
