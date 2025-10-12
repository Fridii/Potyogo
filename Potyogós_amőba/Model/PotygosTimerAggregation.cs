using System;
using System.Timers;

namespace Potyogós_amőba.Model
{
    /// <summary>
    /// Időzítő a <see cref="System.Timers.Timer"/> aggregálásával, az <see cref="ITimer"/> interfészt megvalósítva.
    /// </summary>
    public class PotygosTimerAggregation : ITimer, IDisposable
    {
        private readonly System.Timers.Timer _timer;

        public bool Enabled
        {
            get => _timer.Enabled;
            set => _timer.Enabled = value;
        }

        public double Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public event EventHandler? Elapsed;

        public PotygosTimerAggregation()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += (s, e) => Elapsed?.Invoke(this, e);
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Dispose();
    }
}
