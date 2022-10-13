using System;
using System.Threading.Tasks;

namespace TaskScheduling
{
    /// <summary>
    /// Sample scheduled task implementation
    /// </summary>
    public class SampleScheduleTask : IScheduleTask
    {
        /// <summary>
        /// Executes a task
        /// </summary>
        public async Task ExecuteAsync()
        {
            Console.WriteLine("Sample task executed!");
            await Task.CompletedTask;
        }
    }
}