using Xunit;

namespace ColonyTime.Tests
{
    public class SaveTimeFormatterTests
    {
        [Theory]
        [InlineData(null, "—")]
        [InlineData(-1d, "—")]
        [InlineData(0d, "0m")]
        [InlineData(59d, "0m")]
        [InlineData(60d, "1m")]
        [InlineData(47 * 60d, "47m")]
        [InlineData(3 * 3600d + 12 * 60d, "3h 12m")]
        [InlineData(183 * 3600d + 42 * 60d, "183h 42m")]
        public void FormatPlayTime_examples(double? seconds, string expected)
        {
            Assert.Equal(expected, SaveTimeFormatter.FormatPlayTime(seconds));
        }

        [Theory]
        [InlineData(null, "—")]
        [InlineData(-1L, "—")]
        [InlineData(0L, "0d")]
        [InlineData(59999L, "0d")]
        [InlineData(60000L, "1d")]
        [InlineData(38L * 60000L, "38d")]
        [InlineData(3L * 60 * 60000L + 17L * 60000L, "3y 17d")]
        [InlineData(6L * 60 * 60000L + 31L * 60000L, "6y 31d")]
        public void FormatColonyAge_examples(long? ticks, string expected)
        {
            Assert.Equal(expected, SaveTimeFormatter.FormatColonyAge(ticks));
        }

        [Fact]
        public void FormatLines_prefix()
        {
            Assert.Equal("Played: 3h 12m", SaveTimeFormatter.FormatPlayedLine(3 * 3600d + 12 * 60d));
            Assert.Equal("Colony: 6y 31d", SaveTimeFormatter.FormatColonyLine(6L * 60 * 60000L + 31L * 60000L));
        }
    }
}
