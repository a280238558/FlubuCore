using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FlubuCore.Context;

namespace FlubuCore.Tasks
{
    /// <summary>
    ///     A base abstract class from which tasks can be implemented.
    /// </summary>
    public abstract class TaskBase<T> : ITaskOfT<T>
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is safe to execute in dry run mode.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is safe to execute in dry run mode; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsSafeToExecuteInDryRun => false;

        public Stopwatch TaskStopwatch { get; } = new Stopwatch();

        protected virtual string DescriptionForLog => null;

        /// <summary>
        ///     Gets a value indicating whether the duration of the task should be logged after the task
        ///     has finished.
        /// </summary>
        /// <value><c>true</c> if duration should be logged; otherwise, <c>false</c>.</value>
        protected virtual bool LogDuration => false;

        public void ExecuteVoid(ITaskContext context)
        {
            Execute(context);
        }

        public async Task ExecuteVoidAsync(ITaskContext context)
        {
            await ExecuteAsync(context);
        }

        /// <summary>
        ///     Executes the task using the specified script execution environment.
        /// </summary>
        /// <remarks>
        ///     This method implements the basic reporting and error handling for
        ///     classes which inherit the <see>
        ///         <cref>TaskBase</cref>
        ///     </see>
        ///     class.
        /// </remarks>
        /// <param name="context">The script execution environment.</param>
        public T Execute(ITaskContext context)
        {
            ITaskContextInternal contextInternal = (ITaskContextInternal)context;

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            TaskStopwatch.Start();

            if (!string.IsNullOrEmpty(DescriptionForLog))
            {
                contextInternal.LogInfo(DescriptionForLog);
            }

            contextInternal.IncreaseDepth();

            try
            {
                return DoExecute(contextInternal);
            }
            finally
            {
                TaskStopwatch.Stop();
                contextInternal.DecreaseDepth();

                if (LogDuration)
                {
                    contextInternal.LogInfo($"{DescriptionForLog} finished (took {(int)TaskStopwatch.Elapsed.TotalSeconds} seconds)");
                }
            }
        }

        public async Task<T> ExecuteAsync(ITaskContext context)
        {
            ITaskContextInternal contextInternal = (ITaskContextInternal)context;

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            TaskStopwatch.Start();

            if (!string.IsNullOrEmpty(DescriptionForLog))
            {
                contextInternal.LogInfo(DescriptionForLog);
            }

            contextInternal.IncreaseDepth();

            try
            {
                return await DoExecuteAsync(contextInternal);
            }
            finally
            {
                TaskStopwatch.Stop();
                contextInternal.DecreaseDepth();

                if (LogDuration)
                {
                    contextInternal.LogInfo($"{DescriptionForLog} finished (took {(int)TaskStopwatch.Elapsed.TotalSeconds} seconds)");
                }
            }
        }

        /// <summary>
        ///     Abstract method defining the actual work for a task.
        /// </summary>
        /// <remarks>This method has to be implemented by the inheriting task.</remarks>
        /// <param name="context">The script execution environment.</param>
        protected abstract T DoExecute(ITaskContextInternal context);

        /// <summary>
        /// Virtual method defining the actual work for a task.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual async Task<T> DoExecuteAsync(ITaskContextInternal context)
        {
            return await Task.Run(() => DoExecute(context));
        }
    }
}