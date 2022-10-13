using System.Threading.Tasks;

namespace TaskScheduling
{
    /// <summary>
    /// Interface that should be implemented by each task
    /// </summary>
    public partial interface IScheduleTask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        Task ExecuteAsync();
    }
}
