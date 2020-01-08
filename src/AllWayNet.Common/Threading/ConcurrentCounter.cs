namespace AllWayNet.Common.Threading
{
    /// <summary>
    /// Thread safe counter.
    /// </summary>
    public class ConcurrentCounter
    {
        /// <summary>
        /// Object used when locking.
        /// </summary>
        private object lockObject = new object();

        /// <summary>
        /// Internal counter.
        /// </summary>
        private int counter = 0;

        /// <summary>
        /// Gets an object of type ProtectedCounter initialized to 0.
        /// </summary>
        public static ConcurrentCounter Zero
        {
            get
            {
                return new ConcurrentCounter();
            }
        }

        /// <summary>
        /// Gets the Value.
        /// </summary>
        public int Value
        {
            get
            {
                lock (this.lockObject)
                {
                    return this.counter;
                }
            }
        }

        /// <summary>
        /// Increases the counter.
        /// </summary>
        /// <param name="inc">Amount to increase.</param>
        public void Inc(int inc = 1)
        {
            lock (this.lockObject)
            {
                this.counter += inc;
            }
        }
    }
}
