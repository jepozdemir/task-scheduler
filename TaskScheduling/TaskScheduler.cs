using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TaskScheduling
{
    /// <summary>
    /// Represents task manager
    /// </summary>
    public class TaskScheduler : ITaskScheduler
    {
        #region Fields

        private static readonly List<TaskThread> _taskThreads = new List<TaskThread>();
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IScheduleTaskRunner _taskRunner;
        #endregion

        #region Ctor

        public TaskScheduler(IScheduleTaskService scheduleTaskService,
            IScheduleTaskRunner taskRunner)
        {
            _scheduleTaskService = scheduleTaskService;
            _taskRunner = taskRunner;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the task manager
        /// </summary>
        public void Initialize()
        {

            if (_taskThreads.Any())
                return;

            //initialize and start schedule tasks
            var scheduleTasks = (_scheduleTaskService.GetAllTasks())
                .OrderBy(x => x.Seconds)
                .ToList();

            var timeout = TaskDefaults.ScheduleTaskRunTimeout;

            foreach (var scheduleTask in scheduleTasks)
            {
                var taskThread = new TaskThread(_taskRunner, scheduleTask, timeout)
                {
                    Seconds = scheduleTask.Seconds
                };

                //sometimes a task period could be set to several hours (or even days)
                //in this case a probability that it'll be run is quite small (an application could be restarted)
                //calculate time before start an interrupted task
                if (scheduleTask.LastStartUtc.HasValue)
                {
                    //seconds left since the last start
                    var secondsLeft = (DateTime.UtcNow - scheduleTask.LastStartUtc).Value.TotalSeconds;

                    if (secondsLeft >= scheduleTask.Seconds)
                        //run now (immediately)
                        taskThread.InitSeconds = 0;
                    else
                        //calculate start time
                        //and round it (so "ensureRunOncePerPeriod" parameter was fine)
                        taskThread.InitSeconds = (int)(scheduleTask.Seconds - secondsLeft) + 1;
                }
                else if (scheduleTask.LastEnabledUtc.HasValue)
                {
                    //seconds left since the last enable
                    var secondsLeft = (DateTime.UtcNow - scheduleTask.LastEnabledUtc).Value.TotalSeconds;

                    if (secondsLeft >= scheduleTask.Seconds)
                        //run now (immediately)
                        taskThread.InitSeconds = 0;
                    else
                        //calculate start time
                        //and round it (so "ensureRunOncePerPeriod" parameter was fine)
                        taskThread.InitSeconds = (int)(scheduleTask.Seconds - secondsLeft) + 1;
                }
                else
                    //first start of a task
                    taskThread.InitSeconds = scheduleTask.Seconds;

                _taskThreads.Add(taskThread);
            }
        }

        /// <summary>
        /// Starts the task scheduler
        /// </summary>
        public void StartScheduler()
        {
            foreach (var taskThread in _taskThreads)
                taskThread.InitTimer();
        }

        /// <summary>
        /// Stops the task scheduler
        /// </summary>
        public void StopScheduler()
        {
            foreach (var taskThread in _taskThreads)
                taskThread.Dispose();
        }

        #endregion

        #region Nested class

        /// <summary>
        /// Represents task thread
        /// </summary>
        protected partial class TaskThread : IDisposable
        {
            #region Fields
            private readonly IScheduleTaskRunner _taskRunner;
            protected readonly ScheduleTaskEntry _scheduleTask;
            protected readonly int? _timeout;

            protected Timer _timer;
            protected bool _disposed;

            #endregion

            #region Ctor

            public TaskThread(IScheduleTaskRunner taskRunner, ScheduleTaskEntry task, int? timeout)
            {
                _taskRunner = taskRunner;
                _scheduleTask = task;
                _timeout = timeout;

                Seconds = 10 * 60;
            }

            #endregion

            #region Utilities

            private async Task RunAsync()
            {
                if (Seconds <= 0)
                    return;

                StartedUtc = DateTime.UtcNow;
                IsRunning = true;

                try
                {
                    await _taskRunner.ExecuteAsync(_scheduleTask);
                }
                catch { }

                IsRunning = false;
            }

            private void TimerHandler(object state)
            {
                try
                {
                    _timer.Change(-1, -1);

                    RunAsync().Wait();
                }
                catch
                {
                    // ignore
                }
                finally
                {
                    if (!_disposed && _timer != null)
                    {
                        if (RunOnlyOnce)
                            Dispose();
                        else
                            _timer.Change(Interval, Interval);
                    }
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Disposes the instance
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            // Protected implementation of Dispose pattern.
            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                    lock (this)
                        _timer?.Dispose();

                _disposed = true;
            }

            /// <summary>
            /// Inits a timer
            /// </summary>
            public void InitTimer()
            {
                _timer ??= new Timer(TimerHandler, null, InitInterval, Interval);
            }

            #endregion

            #region Properties

            /// <summary>
            /// Gets or sets the interval in seconds at which to run the tasks
            /// </summary>
            public int Seconds { get; set; }

            /// <summary>
            /// Get or set the interval before timer first start 
            /// </summary>
            public int InitSeconds { get; set; }

            /// <summary>
            /// Get or sets a datetime when thread has been started
            /// </summary>
            public DateTime StartedUtc { get; private set; }

            /// <summary>
            /// Get or sets a value indicating whether thread is running
            /// </summary>
            public bool IsRunning { get; private set; }

            /// <summary>
            /// Gets the interval (in milliseconds) at which to run the task
            /// </summary>
            public int Interval
            {
                get
                {
                    //if somebody entered more than "2147483" seconds, then an exception could be thrown (exceeds int.MaxValue)
                    var interval = Seconds * 1000;
                    if (interval <= 0)
                        interval = int.MaxValue;
                    return interval;
                }
            }

            /// <summary>
            /// Gets the due time interval (in milliseconds) at which to begin start the task
            /// </summary>
            public int InitInterval
            {
                get
                {
                    //if somebody entered less than "0" seconds, then an exception could be thrown
                    var interval = InitSeconds * 1000;
                    if (interval <= 0)
                        interval = 0;
                    return interval;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the thread would be run only once (on application start)
            /// </summary>
            public bool RunOnlyOnce { get; set; }

            /// <summary>
            /// Gets a value indicating whether the timer is started
            /// </summary>
            public bool IsStarted => _timer != null;

            /// <summary>
            /// Gets a value indicating whether the timer is disposed
            /// </summary>
            public bool IsDisposed => _disposed;

            #endregion
        }

        #endregion
    }
}
