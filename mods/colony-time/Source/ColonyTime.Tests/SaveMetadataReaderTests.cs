using System.IO;
using Xunit;

namespace ColonyTime.Tests
{
    public class SaveMetadataReaderTests
    {
        public SaveMetadataReaderTests()
        {
            ColonyTimeLog.Reset();
            SaveMetadataCache.Instance.Clear();
        }

        [Fact]
        public void Read_extracts_both_fields_and_stops()
        {
            string path = WriteTempSave(@"<?xml version=""1.0"" encoding=""utf-8""?>
<savegame>
  <meta><gameVersion>1.6.0</gameVersion></meta>
  <game>
    <info>
      <realPlayTimeInteracting>6612.5</realPlayTimeInteracting>
    </info>
    <tickManager>
      <ticksGame>123456789</ticksGame>
    </tickManager>
    <hugePadding>" + new string('x', 5000) + @"</hugePadding>
  </game>
</savegame>");

            SaveMetadata meta = SaveMetadataReader.Read(path);
            Assert.False(meta.ParseFailed);
            Assert.Equal(6612.5d, meta.RealPlayTimeSeconds.Value, 3);
            Assert.Equal(123456789L, meta.TicksGame.Value);
        }

        [Fact]
        public void Read_malformed_fails_safely()
        {
            string path = WriteTempSave("<not-valid-xml");
            SaveMetadata meta = SaveMetadataReader.Read(path);
            Assert.True(meta.ParseFailed);
            Assert.Null(meta.RealPlayTimeSeconds);
            Assert.Null(meta.TicksGame);
            Assert.NotEmpty(ColonyTimeLog.Warnings);
        }

        [Fact]
        public void Cache_reuses_unchanged_file()
        {
            string path = WriteTempSave(@"<?xml version=""1.0"" encoding=""utf-8""?>
<savegame>
  <realPlayTimeInteracting>120</realPlayTimeInteracting>
  <ticksGame>60000</ticksGame>
</savegame>");

            var a = SaveMetadataCache.Instance.GetOrRead(path);
            int messagesAfterFirst = ColonyTimeLog.Messages.Count;
            var b = SaveMetadataCache.Instance.GetOrRead(path);
            Assert.Same(a, b);
            Assert.Equal(messagesAfterFirst, ColonyTimeLog.Messages.Count);
        }

        [Fact]
        public void Cache_invalidates_when_size_changes()
        {
            string path = WriteTempSave(@"<?xml version=""1.0"" encoding=""utf-8""?>
<savegame>
  <realPlayTimeInteracting>120</realPlayTimeInteracting>
  <ticksGame>60000</ticksGame>
</savegame>");

            var a = SaveMetadataCache.Instance.GetOrRead(path);
            File.AppendAllText(path, "<!-- changed -->");
            var b = SaveMetadataCache.Instance.GetOrRead(path);
            Assert.NotSame(a, b);
            Assert.Equal(120d, b.RealPlayTimeSeconds.Value);
        }

        private static string WriteTempSave(string contents)
        {
            string path = Path.Combine(Path.GetTempPath(), "colonytime-test-" + Path.GetRandomFileName() + ".rws");
            File.WriteAllText(path, contents);
            return path;
        }
    }
}
