using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yoakke.Lir
{
    /// <summary>
    /// A collection of metrics for the compilation.
    /// </summary>
    public class Metrics
    {
        /// <summary>
        /// The collected <see cref="TimeMetric"/>s.
        /// </summary>
        public readonly IList<TimeMetric> TimeMetrics = new List<TimeMetric>();

        private Stack<(string, Stopwatch)> timeStarts = new Stack<(string, Stopwatch)>();

        /// <summary>
        /// Starts a new time measurement.
        /// </summary>
        /// <param name="name">The name of the measurement.</param>
        public void StartTime(string name)
        {
            timeStarts.Push((name, Stopwatch.StartNew()));
        }

        /// <summary>
        /// Ends the most recently started time measurement.
        /// </summary>
        public void EndTime()
        {
            var (name, sw) = timeStarts.Pop();
            var elapsed = sw.Elapsed;
            TimeMetrics.Add(new TimeMetric(name, elapsed));
        }
    }

    /// <summary>
    /// A metric representing a time interval.
    /// </summary>
    public readonly struct TimeMetric
    {
        /// <summary>
        /// The name of the metric.
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The <see cref="TimeSpan"/> this metric represents.
        /// </summary>
        public readonly TimeSpan TimeSpan;

        /// <summary>
        /// Initializes a new <see cref="TimeMetric"/>.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        /// <param name="timeSpan">The <see cref="TimeSpan"/> this metric represents.</param>
        public TimeMetric(string name, TimeSpan timeSpan)
        {
            Name = name;
            TimeSpan = timeSpan;
        }

        public void Deconstruct(out string name, out TimeSpan timeSpan)
        {
            name = Name;
            timeSpan = TimeSpan;
        }
    }
}
