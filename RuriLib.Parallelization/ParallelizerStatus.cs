namespace RuriLib.Parallelization
{
    /// <summary>
    /// The status of the parallelizer.
    /// </summary>
    public enum ParallelizerStatus
    {
        /// <summary>
        /// The parallelizer has not started yet.
        /// </summary>
        Idle,

        /// <summary>
        /// The parallelizer is starting up.
        /// </summary>
        Starting,

        /// <summary>
        /// The parallelizer is processing the workload.
        /// </summary>
        Running,

        /// <summary>
        /// The parallelizer is pausing the workload.
        /// </summary>
        Pausing,

        /// <summary>
        /// The parallelizer is paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The parallelizer is stopping the workload.
        /// </summary>
        Stopping,

        /// <summary>
        /// The parallelizer is recovering from a paused state.
        /// </summary>
        Resuming
    }
}
