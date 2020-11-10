using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Game.Beatmaps;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bmviewer
{
    class PpWorkingBeatmap : WorkingBeatmap
    {
        private readonly Beatmap beatmap;
        public int RulesetID => beatmap.BeatmapInfo.RulesetID;
        public double Length
        {
            get
            {
                if (!beatmap.HitObjects.Any())
                    return 0;

                var hitObject = beatmap.HitObjects.Last();
                return (hitObject as IHasEndTime)?.EndTime ?? hitObject.StartTime;
            }
        }

        public string BackgroundFile => beatmap.Metadata.BackgroundFile;
        internal PpWorkingBeatmap(Beatmap beatmap, int? beatmapId = null)
            : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;

            beatmap.BeatmapInfo.Ruleset = GetRulesetFromLegacyID(beatmap.BeatmapInfo.RulesetID).RulesetInfo;

            if (beatmapId.HasValue)
                beatmap.BeatmapInfo.OnlineBeatmapID = beatmapId;
        }

        protected override IBeatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null;
        protected override VideoSprite GetVideo() => null;
        protected override Track GetTrack() => null;

        public static Ruleset GetRulesetFromLegacyID(int id)
        {
            switch (id)
            {
                case 0:
                    return new OsuRuleset();
                //case 1:
                //    return new TaikoRuleset();
                //case 2:
                //    return new CatchRuleset();
                //case 3:
                //    return new ManiaRuleset();
                default:
                    throw new ArgumentException("Invalid ruleset ID provided.");
            }
        }
    }
}
