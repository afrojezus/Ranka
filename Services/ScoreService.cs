using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ranka.Services
{
    public class ScoreObject : IComparable
    {
        public SocketGuildUser User { get; set; }
        public int Score { get; set; }

        public int CompareTo(object obj)
        {
            if (obj is ScoreObject)
                return (obj as ScoreObject).Score.CompareTo(Score);

            return Score.CompareTo(obj);
        }

        public override int GetHashCode() => base.GetHashCode();
    }

    public class ScoreService : RankaService
    {
        public SortedSet<ScoreObject> Leaderboard { get; set; }

        // TODO: Instead of butching it in here, how about incorporating this across event handlers to improve performance?
        public void UpdateLeaderboard(SocketCommandContext ctx)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx), "No context");

            Leaderboard.Clear();
            if (File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}log.{ctx.Guild.Id}.txt"))
            {
                var scoreObjectsRaw = File.ReadAllLines($"{AppDomain.CurrentDomain.BaseDirectory}log.{ctx.Guild.Id}.txt");

                foreach (var item in scoreObjectsRaw)
                {
                    // TODO: Really ugly as fuck, find a better solution.
                    string raw = item.Replace("[", " ").Replace("]", " ");
                    var idRaw = raw.Split()[5];
                    idRaw = idRaw.Substring(idRaw.LastIndexOf('|') + 1);

                    ulong id = ulong.Parse(idRaw);
                    int score = scoreObjectsRaw.Where(x => x.Contains($"|{idRaw}]")).Count();

                    var user = ctx.Guild.GetUser(id);

                    ScoreObject scoreObject = new ScoreObject
                    {
                        User = user,
                        Score = score
                    };
                    if (!(Leaderboard.Where(x => x.User.Id == user.Id).Any()))
                        Leaderboard.Add(scoreObject);
                }

                Leaderboard.OrderBy(x => x.Score);
            }
        }

        public void SetupLeaderboard() => Leaderboard = new SortedSet<ScoreObject>();
    }
}