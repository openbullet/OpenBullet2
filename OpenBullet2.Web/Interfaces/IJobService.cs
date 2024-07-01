using OpenBullet2.Web.Dtos.Job;

namespace OpenBullet2.Web.Interfaces;

/// <summary>
/// A service that can control jobs.
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Start a job.
    /// </summary>
    void Start(int jobId);

    /// <summary>
    /// Stop a job.
    /// </summary>
    void Stop(int jobId);

    /// <summary>
    /// Abort a job.
    /// </summary>
    void Abort(int jobId);

    /// <summary>
    /// Pause a job.
    /// </summary>
    void Pause(int jobId);

    /// <summary>
    /// Resume a paused job.
    /// </summary>
    void Resume(int jobId);

    /// <summary>
    /// Skip the waiting period of a job.
    /// </summary>
    void SkipWait(int jobId);

    /// <summary>
    /// Change the number of bots of a job.
    /// </summary>
    void ChangeBots(int jobId, ChangeBotsMessage message);

    /// <summary>
    /// Registers a new connection, a.k.a. an interactive job session started
    /// by a given client.
    /// </summary>
    void RegisterConnection(string connectionId, int jobId);

    /// <summary>
    /// Unregisters an existing connection.
    /// </summary>
    void UnregisterConnection(string connectionId, int jobId);
}
