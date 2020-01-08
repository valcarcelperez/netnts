namespace AllWayNet.Common.Threading
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Timers;

    /// <summary>
    /// Wraps the System.Timers.Timer class. 
    /// Raises an event when an exception occurs.
    /// Executes an action.
    /// This is a Non-reentrant timer.
    /// </summary>
    public class TimerEx : IDisposable
    {
        /// <summary>
        /// Internal timer.
        /// </summary>
        private System.Timers.Timer timer = null;
        
        /// <summary>
        /// Synchronization object.
        /// </summary>
        private object lockObj = new object();
        
        /// <summary>
        /// Indicates that the object has been disposed.
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Indicates that the action method that is executed in by the timer accepts a CancellationToken
        /// </summary>
        private bool isActionCancellable;

        /// <summary>
        /// Indicates that the action is being executing.
        /// </summary>
        private bool actionExecuting = false;

        /// <summary>
        /// Action to be executed when the timer event elapses.
        /// </summary>
        private Action action;

        /// <summary>
        /// Action to be executed when the timer event elapses.
        /// </summary>
        private Action<CancellationToken> cancelableAction;

        /// <summary>
        /// A CancellationTokenSource used when executing an action.
        /// </summary>
        private CancellationTokenSource cancellationTokenSource = null;

        /// <summary>
        /// Signal when the action completed executing.
        /// </summary>
        private ManualResetEvent idleSignal = new ManualResetEvent(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerEx" /> class.
        /// </summary>
        /// <param name="interval">The time, in milliseconds, between events.</param>
        /// <param name="action">Action executed when the interval elapses.</param>
        public TimerEx(int interval, Action action)
        {
            this.Prepare(interval, false);
            this.action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerEx" /> class.
        /// </summary>
        /// <param name="interval">The time, in milliseconds, between events.</param>
        /// <param name="cancelableAction">Cancelable Action executed when the interval elapses.</param>
        public TimerEx(int interval, Action<CancellationToken> cancelableAction)
        {
            this.Prepare(interval, true);
            this.cancelableAction = cancelableAction;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TimerEx" /> class.
        /// </summary>
        ~TimerEx()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Event raised when an exception occurs.
        /// </summary>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Stops the timer if running and release the resources used by this object.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Start executing the action passed in the constructor.
        /// </summary>
        public void Start()
        {
            lock (this.lockObj)
            {
                if (this.timer.Enabled)
                {
                    return;
                }

                this.cancellationTokenSource = new CancellationTokenSource();
                this.timer.Start();
            }
        }

        /// <summary>
        /// Stops executing the action passed in the constructor.
        /// </summary>
        /// <param name="timeout">The number of milliseconds to wait, or System.Threading.Timeout.Infinite (-1) to wait indefinitely.</param>
        /// <returns>True if the action completed or is not executing; otherwise, false.</returns>
        public bool Stop(int timeout = 1000)
        {
            lock (this.lockObj)
            {
                if (!this.timer.Enabled)
                {
                    return true;
                }
                
                this.timer.Stop();
                this.UnsafeCancelAndDisposeCancellationTokenSource();
            }

            return this.idleSignal.WaitOne(timeout);
        }

        /// <summary>
        /// Part of the disposing mechanism.
        /// </summary>
        /// <param name="disposing">Indicates that the Dispose method is being called by the user code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.DisposeManagedResources();
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Executed when an exception occurs.
        /// </summary>
        /// <param name="e">An ErrorEventArgs containing the exception information.</param>
        protected virtual void OnError(ErrorEventArgs e)
        {
            EventHandler<ErrorEventArgs> handler = this.Error;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// Handles the Elapsed event of the internal timer.
        /// Executes the action passed in the constructor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An System.Timers.ElapsedEventArgs object that contains the event data.</param>
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!this.CanExecuteAction())
            {
                return;
            }

            try
            {
                this.ExecuteAction();
            }
            finally
            {
                this.ActionCompleted();
            }
        }    

        /// <summary>
        /// Set indicators when the action completed executing.
        /// </summary>
        private void ActionCompleted()
        {
            lock (this.lockObj)
            {
                this.actionExecuting = false;
                this.idleSignal.Set();
            }
        }

        /// <summary>
        /// Verifies if the action can be executed.
        /// </summary>
        /// <returns>True if the action can be executed; otherwise, false.</returns>
        private bool CanExecuteAction()
        {
            lock (this.lockObj)
            {
                if (!this.timer.Enabled)
                {
                    return false;
                }

                if (this.actionExecuting)
                {
                    return false;
                }

                this.actionExecuting = true;
                this.idleSignal.Reset();
            }

            return true;
        }
   
        /// <summary>
        /// Executes the action passed in the constructor.
        /// </summary>
        private void ExecuteAction()
        {
            try
            {
                if (this.isActionCancellable)
                {
                    this.cancelableAction(this.cancellationTokenSource.Token);
                }
                else
                {
                    this.action();
                }
            }
            catch (Exception ex)
            {
                this.OnError(new ErrorEventArgs(ex));
            }
        }

        /// <summary>
        /// Common code to prepare this object.
        /// </summary>
        /// <param name="interval">The time, in milliseconds, between events.</param>
        /// <param name="isActionCancelable">Indicates that the action is cancelable.</param>
        private void Prepare(int interval, bool isActionCancelable)
        {
            this.timer = new System.Timers.Timer(interval);
            this.timer.Elapsed += this.TimerElapsed;
            this.isActionCancellable = isActionCancelable;
        }

        /// <summary>
        /// Disposes managed resources.
        /// </summary>
        private void DisposeManagedResources()
        {
            this.Stop();
            this.timer.Dispose();
            this.idleSignal.Dispose();
        }

        /// <summary>
        /// Cancel and dispose the cancellation token source.
        /// </summary>
        private void UnsafeCancelAndDisposeCancellationTokenSource()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource.Dispose();
                this.cancellationTokenSource = null;
            }
        }
    }
}
