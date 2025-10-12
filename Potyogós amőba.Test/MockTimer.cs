using System;
using Potyogós_amőba.Model;

namespace Potyogós_amőba.Test
{
    /// <summary>
    /// Időzítő mockolása.
    /// </summary>
    public class MockTimer : Model.ITimer
    {
        public bool Enabled { get; set; }
        public double Interval { get; set; }
        public event EventHandler? Elapsed;

        public void Start()
        {
            Enabled = true;
        }

        public void Stop()
        {
            Enabled = false;
        }

        /// <summary>
        /// Időzítő eseményének explicit kiváltása.
        /// </summary>
        public void RaiseElapsed()
        {
            Elapsed?.Invoke(this, EventArgs.Empty);
        }
    }
}
