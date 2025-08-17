// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Beatmaps;

namespace osu.Game.Database
{
    /// <summary>
    /// Exporter for osu!stable legacy beatmap archives.
    /// Converts all beatmaps in the set to legacy format and exports it as a legacy package.
    /// </summary>
    public class LegacyBeatmapExporter : LegacyArchiveExporter<BeatmapSetInfo>
    {
        private readonly LegacyDifficultyExporter difficultyExporter;

        public LegacyBeatmapExporter(Storage storage)
            : base(storage)
        {
            difficultyExporter = new LegacyDifficultyExporter(storage);
        }

        protected override Stream? GetFileContents(BeatmapSetInfo model, INamedFileUsage file)
        {
            var beatmapInfo = model.Beatmaps.SingleOrDefault(o => o.Hash == file.File.Hash);

            if (beatmapInfo == null)
                return base.GetFileContents(model, file);

            var stream = new MemoryStream();
            difficultyExporter.ExportToStream(beatmapInfo, stream, null);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        protected override string FileExtension => @".osz";
    }
}
