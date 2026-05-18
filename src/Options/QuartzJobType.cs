using System;

namespace Lycoris.Quartz.Options
{
    internal class QuartzJobType
    {
        public Type JobType { get; set; }

        public QuartzJobAttribute JobSettings { get; set; }
    }
}
