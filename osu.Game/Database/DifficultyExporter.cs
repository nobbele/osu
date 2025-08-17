// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Overlays.Notifications;

namespace osu.Game.Database
{
    /// <summary>
    /// Exporter for osu!lazer beatmap files.
    /// This is not for legacy purposes and works for lazer only.
    /// </summary>
    public class DifficultyExporter : LegacyExporter<BeatmapInfo>
    {
        public DifficultyExporter(Storage storage)
            : base(storage)
        {
        }

        public override void ExportToStream(BeatmapInfo model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
        {
            if (model.File == null)
                return;

            GetFileContents(model.File)?.CopyTo(outputStream);
        }

        protected Stream? GetFileContents(INamedFileUsage file)
            => UserFileStorage.GetStream(file.File.GetStoragePath());

        protected override string FileExtension => @".osu";
    }
}
