namespace LanternVillage
{
    /// <summary>
    /// Whether the player picked Hamlet Mode or Founding Mode. Set by the launcher
    /// (via a marker file read at startup -- see ResumeHelper's sibling check in
    /// LanternVillageMod) or left false for Founding Mode's undeveloped clearing.
    /// </summary>
    public static class HamletModeState
    {
        public static bool HamletModeRequested = false;
    }
}
