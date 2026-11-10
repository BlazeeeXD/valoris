using System.Globalization;
using Valoris.Core.Contracts;

namespace Valoris.Core.Cli.Reporting
{
    public static class CsvExporter
    {
        public static void Export(IReadOnlyList<EpisodeMetric> metrics, string path)
        {
            if (metrics == null) throw new ArgumentNullException(nameof(metrics));
            string directory = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            using var writer = new StreamWriter(path, false);
            writer.WriteLine("episode,totalReward,steps,reachedGoal,epsilon,terminationReason");
            foreach (EpisodeMetric m in metrics)
                writer.WriteLine(string.Join(",", m.EpisodeNumber,
                    m.TotalReward.ToString(CultureInfo.InvariantCulture), m.Steps,
                    m.ReachedGoal, m.Epsilon.ToString(CultureInfo.InvariantCulture), m.TerminationReason));
        }
    }
}
