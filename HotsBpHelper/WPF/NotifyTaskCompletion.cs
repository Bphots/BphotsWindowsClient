using System;
using System.Threading.Tasks;

using HotsBpHelper.Pages;

namespace HotsBpHelper.WPF
{
    /// <summary>
    ///     Binding-friendly Asynchronous task wrapper
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public sealed class NotifyTaskCompletion<TResult> : ViewModelBase
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="task">The Task to run.</param>
        /// <param name="defaultResult">Optional default Result to return while the Task is running.</param>
        public NotifyTaskCompletion(Task<TResult> task, TResult defaultResult = default(TResult))
        {
            m_defaultResult = defaultResult;
            Task = task;
            if (!task.IsCompleted)
                TaskCompletion = WatchTaskAsync(task);
        }

        /// <summary>
        ///     Error message if Task resulted in Exception
        /// </summary>
        public string ErrorMessage => InnerException?.Message;

        /// <summary>
        ///     Exception produced by Task.
        /// </summary>
        public AggregateException Exception => Task.Exception;

        /// <summary>
        ///     InnerException produced by Task
        /// </summary>
        public Exception InnerException => Exception?.InnerException;

        /// <summary>
        ///     Was the Task cancelled?
        /// </summary>
        public bool IsCanceled => Task.IsCanceled;

        /// <summary>
        ///     Has the Task completed?
        /// </summary>
        public bool IsCompleted => Task.IsCompleted;

        /// <summary>
        ///     Did the Task encounter an unhandled Exception?
        /// </summary>
        public bool IsFaulted => Task.IsFaulted;

        /// <summary>
        ///     Has the Task not completed yet?
        /// </summary>
        public bool IsNotCompleted => !Task.IsCompleted && !Task.IsCanceled;

        /// <summary>
        ///     Did the task successfully complete?
        /// </summary>
        public bool IsSuccessfullyCompleted => Status == TaskStatus.RanToCompletion;

        /// <summary>
        ///     The result of the Task.
        /// </summary>
        public TResult Result => IsSuccessfullyCompleted ? Task.Result : m_defaultResult;

        /// <summary>
        ///     Current Task Status.
        /// </summary>
        public TaskStatus Status => Task.Status;

        /// <summary>
        ///     The Task.
        /// </summary>
        public Task<TResult> Task { get; }

        public Task TaskCompletion { get; private set; }

        /// <summary>
        ///     Fired when the Task has been cancelled.
        /// </summary>
        public event EventHandler TaskCancelled;

        /// <summary>
        ///     Fired when the Task has failed.
        /// </summary>
        public event EventHandler TaskFailed;

        /// <summary>
        ///     Fired when the Task has stopped.
        ///     This does not indicate that it completed successfully, it might have failed or has been cancelled.
        /// </summary>
        public event EventHandler TaskStopped;

        /// <summary>
        ///     Fired when the Task has successfully completed.
        /// </summary>
        public event EventHandler TaskSuccessfullyCompleted;

        private void OnTaskCancelled()
        {
            TaskCancelled?.Invoke(this, EventArgs.Empty);
        }

        private void OnTaskCompleted()
        {
            TaskSuccessfullyCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void OnTaskFailed()
        {
            TaskFailed?.Invoke(this, EventArgs.Empty);
        }

        private void OnTaskStopped()
        {
            TaskStopped?.Invoke(this, EventArgs.Empty);
        }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                // Handled by Task.IsFaulted
            }

            NotifyOfPropertyChange(() => Status);
            NotifyOfPropertyChange(() => IsCompleted);
            NotifyOfPropertyChange(() => IsNotCompleted);

            if (task.IsCanceled)
            {
                NotifyOfPropertyChange(() => IsCanceled);
                OnTaskCancelled();
            }
            else if (task.IsFaulted)
            {
                NotifyOfPropertyChange(() => IsFaulted);
                NotifyOfPropertyChange(() => Exception);
                NotifyOfPropertyChange(() => InnerException);
                NotifyOfPropertyChange(() => ErrorMessage);
                OnTaskFailed();
            }
            else
            {
                NotifyOfPropertyChange(() => IsSuccessfullyCompleted);
                NotifyOfPropertyChange(() => Result);
                OnTaskCompleted();
            }

            OnTaskStopped();
        }

        private readonly TResult m_defaultResult;
    }

    /// <summary>
    ///     An empty, completed Task.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class Empty<T>
    {
        public static Task<T> Task { get; } = System.Threading.Tasks.Task.FromResult(default(T));
    }
}
