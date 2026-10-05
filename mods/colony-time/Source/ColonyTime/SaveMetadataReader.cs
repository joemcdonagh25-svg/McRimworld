using System;
using System.IO;
using System.Xml;

namespace ColonyTime
{
    /// <summary>
    /// Forward-only .rws reader. Stops once both required fields are found.
    /// </summary>
    public static class SaveMetadataReader
    {
        public const string PlayTimeElement = "realPlayTimeInteracting";
        public const string TicksGameElement = "ticksGame";

        public static SaveMetadata Read(string filePath)
        {
            var info = new FileInfo(filePath);
            var meta = new SaveMetadata
            {
                FilePath = filePath,
                LastWriteTimeUtc = info.Exists ? info.LastWriteTimeUtc : DateTime.MinValue,
                FileSize = info.Exists ? info.Length : 0L
            };

            if (!info.Exists)
            {
                meta.ParseFailed = true;
                return meta;
            }

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = XmlReader.Create(stream, CreateSettings()))
                {
                    bool foundPlayTime = false;
                    bool foundTicks = false;

                    while (reader.Read())
                    {
                        if (reader.NodeType != XmlNodeType.Element)
                        {
                            continue;
                        }

                        if (!foundPlayTime && reader.Name == PlayTimeElement)
                        {
                            string text = reader.ReadElementContentAsString();
                            if (TryParseDouble(text, out double seconds))
                            {
                                meta.RealPlayTimeSeconds = seconds;
                            }

                            foundPlayTime = true;
                        }
                        else if (!foundTicks && reader.Name == TicksGameElement)
                        {
                            string text = reader.ReadElementContentAsString();
                            if (TryParseLong(text, out long ticks))
                            {
                                meta.TicksGame = ticks;
                            }

                            foundTicks = true;
                        }

                        if (foundPlayTime && foundTicks)
                        {
                            break;
                        }
                    }

                    if (meta.RealPlayTimeSeconds == null && meta.TicksGame == null)
                    {
                        meta.ParseFailed = true;
                        ColonyTimeLog.WarningOnce(
                            "parse:" + filePath,
                            "Could not read metadata from " + info.Name + ": required fields missing.");
                    }
                }
            }
            catch (Exception ex)
            {
                meta.ParseFailed = true;
                meta.RealPlayTimeSeconds = null;
                meta.TicksGame = null;
                ColonyTimeLog.WarningOnce(
                    "parse:" + filePath,
                    "Could not read metadata from " + info.Name + ": " + ex.Message);
            }

            return meta;
        }

        private static XmlReaderSettings CreateSettings()
        {
            return new XmlReaderSettings
            {
                IgnoreComments = true,
                IgnoreWhitespace = true,
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                CloseInput = true
            };
        }

        private static bool TryParseDouble(string text, out double value)
        {
            return double.TryParse(
                text,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
        }

        private static bool TryParseLong(string text, out long value)
        {
            return long.TryParse(
                text,
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
        }
    }
}
