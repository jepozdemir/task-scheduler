using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TaskScheduling
{
    class Program
    {
        private static IScheduleTaskService _scheduleTaskService;
        private static ITaskScheduler _taskScheduler;
        private static IScheduleTaskRunner _taskRunner;

        static void Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                                            .AddSingleton<IScheduleTaskService, ScheduleTaskService>()
                                            .AddSingleton<ITaskScheduler, TaskScheduler>()
                                            .AddTransient<IScheduleTaskRunner, ScheduleTaskRunner>()
                                            .BuildServiceProvider();

            _scheduleTaskService = serviceProvider.GetService<IScheduleTaskService>();
            _taskScheduler = serviceProvider.GetService<ITaskScheduler>();
            _taskRunner = serviceProvider.GetService<IScheduleTaskRunner>();

            var task = new ScheduleTaskEntry
            {
                Id = 1,
                Enabled = true,
                Seconds = 1,
                Name = "test schedule task",
                Type = typeof(SampleScheduleTask).FullName
            };

            _scheduleTaskService.InsertTask(task);

            RunOnce(1).Wait();
            
            //repeat per second..
            _taskScheduler.Initialize();
            Console.WriteLine("task scheduler started.");
            _taskScheduler.StartScheduler();
            Console.ReadLine();
            _taskScheduler.StopScheduler();
        }

        private static async Task RunOnce(int id)
        {
            var scheduleTask = _scheduleTaskService.GetTaskById(id)
                                   ?? throw new ArgumentException("Schedule task cannot be loaded", nameof(id));

            await _taskRunner.ExecuteAsync(scheduleTask, true, true, false);
        }
    }
}
