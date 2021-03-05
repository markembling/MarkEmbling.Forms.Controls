using System;

namespace MarkEmbling.Forms.Controls.Events
{
    public class ValueInRangeChangedEventArgs : EventArgs {
        /// <summary>
        /// GaugeRange affected by this event.
        /// </summary>
        public GaugeRange Range { get; private set; }

        /// <summary>
        /// Gauge value
        /// </summary>
        public float Value { get; private set; }

        /// <summary>
        /// Whether or not the value is within this range (have we entered it or left it?)
        /// </summary>
        public bool InRange { get; private set; }

        public ValueInRangeChangedEventArgs(GaugeRange range, Single value, bool inRange) {
            Range = range;
            Value = value;
            InRange = inRange;
        }
    }
}