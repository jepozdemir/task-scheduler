namespace TaskScheduling
{
    /// <summary>
    /// Represents default values related to task services
    /// </summary>
    public static partial class TaskDefaults
    {
        /// <summary>
        /// Gets a running schedule task path
        /// </summary>
        public static string ScheduleTaskPath => "scheduletask/runtask";

        /// <summary>
        /// The length of time, in milliseconds, before the running schedule task times out. Set null to use default value
        /// </summary>
        public static int? ScheduleTaskRunTimeout => null;

        public static string RemoteHost => "/";
    }
}