using Microsoft.Extensions.DependencyInjection;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Jobs;
using System;
using System.Linq;
using System.Reflection;

namespace OpenBullet2.Native.Tests;

[Collection("WPF")]
public sealed class JobsViewModelTests(WpfAppFixture fixture)
{
    [Fact]
    public async Task EditJobAsync_WhenJobIsNotIdle_Throws()
    {
        await fixture.InvokeAsync(services =>
        {
            var jobsViewModel = services.GetRequiredService<JobsViewModel>();
            var jobManager = services.GetRequiredService<JobManagerService>();
            var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();

            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IJobRepository>();

            repo.Purge();
            jobManager.Clear();

            try
            {
                var options = (ProxyCheckJobOptions)JobOptionsFactory.CreateNew(JobType.ProxyCheck);
                var jobViewModel = jobsViewModel.CreateJobAsync(options).GetAwaiter().GetResult();
                var entity = repo.GetAll().First(e => e.Id == jobViewModel.Id);

                SetJobStatus(jobViewModel.Job, JobStatus.Running);

                var exception = Assert.Throws<InvalidOperationException>(
                    () => jobsViewModel.EditJobAsync(entity, options).GetAwaiter().GetResult());

                Assert.Equal("Stop or abort the job before editing it.", exception.Message);
            }
            finally
            {
                repo.Purge();
                jobManager.Clear();
            }
        });
    }

    private static void SetJobStatus(Job job, JobStatus status)
    {
        var setter = typeof(Job).GetProperty(nameof(Job.Status))?.GetSetMethod(true)
            ?? throw new InvalidOperationException("Could not access the job status setter");

        setter.Invoke(job, [status]);
    }
}
