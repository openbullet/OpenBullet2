# RuriLib.Parallelization
This is a library that can perform multiple tasks (yes, a lot, even infinitely many) that act on some input and return a certain output.

Features:
- Fully asynchronous
- Dynamic degree of parallelism (change it while it runs)
- Pausing and resuming
- Soft stop and hard abort
- Automatic CPM calculation
- Events

# Installation
[NuGet](https://nuget.org/packages/RuriLib.Parallelization): `dotnet add package RuriLib.Parallelization`

# Example
```csharp
using RuriLib.Parallelization;
using RuriLib.Parallelization.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelizationDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            _ = MainAsync(args);
            Console.ReadLine();
        }

        static async Task MainAsync(string[] args)
        {
            // This func takes an input type of 'int', a cancellation token, and an output type of `Task` of `bool`
            Func<int, CancellationToken, Task<bool>> parityCheck = new(async (number, token) => 
            {
                // This is the body of your work function
                await Task.Delay(50, token);
                return number % 2 == 0;
            });

            var parallelizer = ParallelizerFactory<int, bool>.Create(
                type: ParallelizerType.TaskBased, // Use task-based (it's better)
                workItems: Enumerable.Range(1, 100), // The work items are all integers from 1 to 100
                workFunction: parityCheck, // Use the work function we defined above
                degreeOfParallelism: 5, // Use 5 concurrent tasks at most
                totalAmount: 100, // The total amount of tasks you expect to have, used for calculating progress
                skip: 0); // How many items to skip from the start of the provided enumerable

            // Hook the events
            parallelizer.NewResult += OnResult;
            parallelizer.Completed += OnCompleted;
            parallelizer.Error += OnException;
            parallelizer.TaskError += OnTaskError;

            await parallelizer.Start();

            // It's important to always pass a cancellation token to avoid waiting forever if something goes wrong!
            var cts = new CancellationTokenSource();
            cts.CancelAfter(10000);

            await parallelizer.WaitCompletion(cts.Token);
        }

        private static void OnResult(object sender, ResultDetails<int, bool> value)
            => Console.WriteLine($"Got result {value.Result} from the parity check of {value.Item}");
        private static void OnCompleted(object sender, EventArgs e) => Console.WriteLine("All work completed!");
        private static void OnTaskError(object sender, ErrorDetails<int> details)
            => Console.WriteLine($"Got error {details.Exception.Message} while processing the item {details.Item}");
        private static void OnException(object sender, Exception ex) => Console.WriteLine($"Exception: {ex.Message}");
    }
}

```
To change the degree of parallelism while it's running, for example to speed up or slow down the work, you can write
```cs
await parallelizer.ChangeDegreeOfParallelism(10);
```
You can also pause and resume work. Notice that pausing will wait until all the tasks that are being worked on will end, so it will not have immediate action since there is no support for pausing tasks half-way.
```cs
await parallelizer.Pause();
// Do something
await parallelizer.Resume();
```
Finally, there are two ways to stop the parallelizer. You can either stop it (which, like pause, waits until all current tasks have ended) or abort it, which will cancel the cancellation token passed to the tasks. You should constantly check if the cancellation token has been cancelled inside your work function.
```cs
await parallelizer.Stop();
await parallelizer.Abort();
```
You can check how fast the parallelizer is processing items or how much time is remaining by accessing the corresponding properties
```cs
Console.WriteLine($"Doing {parallelizer.CPM} checks per minute and the remaining time is {parallelizer.Remaining}");
```
