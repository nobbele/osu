// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Screens.Edit.Submission
{
    public class SubmissionBeatmapExporter : LegacyArchiveExporter<BeatmapSetInfo>
    {
        private readonly LegacyDifficultyExporter difficultyExporter;

        private readonly uint? beatmapSetId;
        private readonly HashSet<int>? allocatedBeatmapIds;

        public SubmissionBeatmapExporter(Storage storage, PutBeatmapSetResponse putBeatmapSetResponse)
            : base(storage)
        {
            difficultyExporter = new LegacyDifficultyExporter(storage);

            beatmapSetId = putBeatmapSetResponse.BeatmapSetId;
            allocatedBeatmapIds = putBeatmapSetResponse.BeatmapIds.Select(id => (int)id).ToHashSet();
        }

        protected override Stream? GetFileContents(BeatmapSetInfo model, INamedFileUsage file)
        {
            var beatmapInfo = model.Beatmaps.SingleOrDefault(o => o.Hash == file.File.Hash);

            if (beatmapInfo == null)
                return base.GetFileContents(model, file);

            MutateBeatmap(model, beatmapInfo);

            var stream = new MemoryStream();
            difficultyExporter.ExportToStream(beatmapInfo, stream, null);

            return stream;
        }

        protected void MutateBeatmap(BeatmapSetInfo beatmapSet, BeatmapInfo beatmapInfo)
        {
            if (beatmapSetId != null && allocatedBeatmapIds != null)
            {
                beatmapInfo.BeatmapSet = beatmapSet;
                beatmapInfo.BeatmapSet!.OnlineID = (int)beatmapSetId;

                if (allocatedBeatmapIds.Contains(beatmapInfo.OnlineID))
                {
                    allocatedBeatmapIds.Remove(beatmapInfo.OnlineID);
                    return;
                }

                if (beatmapInfo.OnlineID > 0)
                    throw new InvalidOperationException($@"Difficulty ""{beatmapInfo.DifficultyName}"" has BeatmapID {beatmapInfo.OnlineID} that has not been assigned to it by the server!");

                if (allocatedBeatmapIds.Count == 0)
                    throw new InvalidOperationException(@"Ran out of new beatmap IDs to assign to unsubmitted beatmaps!");

                int newId = allocatedBeatmapIds.First();
                allocatedBeatmapIds.Remove(newId);
                beatmapInfo.OnlineID = newId;
            }
        }

        protected override string FileExtension => @".osz";
    }
}
