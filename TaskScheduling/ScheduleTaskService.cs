using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskScheduling
{
    /// <summary>
    /// Task service
    /// </summary>
    public class ScheduleTaskService : IScheduleTaskService
    {
        #region Fields

        private List<ScheduleTaskEntry> _taskSource;

        #endregion

        #region Ctor

        public ScheduleTaskService()
        {
            _taskSource = new List<ScheduleTaskEntry>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="task">Task</param>
        public void DeleteTask(ScheduleTaskEntry task)
        {
            _taskSource.Remove(task);
        }

        /// <summary>
        /// Gets a task
        /// </summary>
        /// <param name="taskId">Task identifier</param>
        public ScheduleTaskEntry GetTaskById(int taskId)
        {
            return _taskSource.FirstOrDefault(t => t.Id == taskId);
        }

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        public virtual ScheduleTaskEntry GetTaskByType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return null;

            return _taskSource.Where(st => st.Type == type)
                .OrderByDescending(t => t.Id)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets all tasks
        /// </summary>
        public List<ScheduleTaskEntry> GetAllTasks()
        {
            return _taskSource;
        }

        /// <summary>
        /// Inserts a task
        /// </summary>
        /// <param name="task">Task</param>
        public void InsertTask(ScheduleTaskEntry task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            if (task.Enabled && !task.LastEnabledUtc.HasValue)
                task.LastEnabledUtc = DateTime.UtcNow;

            _taskSource.Add(task);
        }

        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="task">Task</param>
        public void UpdateTask(ScheduleTaskEntry task)
        {

        }

        #endregion
    }
}