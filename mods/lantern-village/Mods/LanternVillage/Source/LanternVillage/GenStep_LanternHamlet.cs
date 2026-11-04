using Verse;

namespace LanternVillage
{
    /// <summary>
    /// Optional GenStep entry point that delegates to <see cref="LanternHamletBuilder"/>.
    /// The authoritative path is <see cref="ScenPart_LanternVillageSetup.GenerateIntoMap"/>
    /// (same pattern as Vampire Lord's playtest keep). This GenStep is retained so a
    /// future MapGeneratorDef patch can also invoke the shared builder; the builder's
    /// per-map guard prevents double construction.
    /// </summary>
    public class GenStep_LanternHamlet : GenStep
    {
        public override int SeedPart => 194_810_31;

        public override void Generate(Map map, GenStepParams parms)
        {
            LanternHamletBuilder.TryBuild(map);
        }
    }
}
