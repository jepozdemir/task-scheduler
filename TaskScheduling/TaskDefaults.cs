namespace TaskScheduling
{
    /// <summary>
    /// Represents default values related to task services
    /// </summary>
    public static class TaskDefaults
    {
        /// <summary>
        /// The length of time, in milliseconds, before the running schedule task times out. Set null to use default value
        /// </summary>
        public static int? ScheduleTaskRunTimeout => null;
    }
}