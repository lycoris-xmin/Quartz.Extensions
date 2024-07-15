using System;

namespace Lycoris.Quartz.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    internal class QuartzOptionException : Exception
    {
        public QuartzOptionException(string prop) : base($"{prop} property not configured")
        {

        }

        public QuartzOptionException(Type jobType, QuartzTriggerEnum trigger)
             : base(trigger == QuartzTriggerEnum.SIMPLE
                   ? $"job {jobType.FullName} intervalSecond must be configured in {trigger} trigger mode"
                   : $"job {jobType.FullName} cron expression must be configured in {trigger} trigger mode")
        {

        }
    }
}
