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

            // It's important to always pass a cancellation token so the user can cancel the work
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
