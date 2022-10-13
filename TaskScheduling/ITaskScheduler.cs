using System.Threading.Tasks;

namespace TaskScheduling
{
    /// <summary>
    /// Task manager interface
    /// </summary>
    public interface ITaskScheduler
    {
        /// <summary>
        /// Initializes task scheduler
        /// </summary>
        void Initialize();

        /// <summary>
        /// Starts the task scheduler
        /// </summary>
        public void StartScheduler();

        /// <summary>
        /// Stops the task scheduler
        /// </summary>
        public void StopScheduler();
    }
}
