namespace LanternVillage
{
    /// <summary>
    /// Legacy launch-mode marker (Founding vs Hamlet). The prebuilt hamlet now always
    /// generates for The Lantern Village scenario via ScenPart.GenerateIntoMap; this
    /// flag is retained only so older launcher marker files remain harmless.
    /// </summary>
    public static class HamletModeState
    {
        public static bool HamletModeRequested = true;
    }
}
