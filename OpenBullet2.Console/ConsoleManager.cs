using RuriLib.Models.Jobs;
using Spectre.Console;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OpenBullet2.Console
{
    class ConsoleManager
    {
        private readonly MultiRunJob _multiRunJob;

        public ConsoleManager(MultiRunJob multiRunJob)
        {
            _multiRunJob = multiRunJob;
        }

        public async Task StartUpdatingTitleAsync()
        {
            var title = new StringBuilder();

            while (true)
            {
                title
                    .Append("OpenBullet 2 (Console POC) - ")
                    .Append(_multiRunJob.Status)
                    .Append(" | Config: ")
                    .Append(_multiRunJob.Config.Metadata.Name)
                    .Append($" ({_multiRunJob.Progress * 100:0.00}%) | Hits: ")
                    .Append(_multiRunJob.DataHits)
                    .Append(" Custom: ")
                    .Append(_multiRunJob.DataCustom)
                    .Append(" ToCheck: ")
                    .Append(_multiRunJob.DataToCheck)
                    .Append(" Fails: ")
                    .Append(_multiRunJob.DataFails)
                    .Append(" Retries: ")
                    .Append(_multiRunJob.DataRetried + _multiRunJob.DataBanned)
                    .Append(" | Bots: ")
                    .Append(_multiRunJob.Bots)
                    .Append(" | CPM: ")
                    .Append(_multiRunJob.CPM)
                    .Append(" | Progress: ")
                    .Append(_multiRunJob.DataTested)
                    .Append(" / ")
                    .Append(_multiRunJob.DataPool.Size)
                    .Append(" | Proxies: ")
                    .Append(_multiRunJob.ProxiesAlive)
                    .Append(" / ")
                    .Append(_multiRunJob.ProxiesTotal);

                System.Console.Title = title.ToString();

                title.Clear();

                await Task.Delay(1000);
            }
        }

        public async Task StartListeningKeysAsync()
        {
            while (true)
            {
                var key = System.Console.ReadKey(true).Key;

                switch (key)
                {
                    case ConsoleKey.P:
                        await _multiRunJob.Pause();
                        AnsiConsole.MarkupLine($"[darkorange]pause at {DateTime.Now}[/]");
                        break;
                    case ConsoleKey.R:
                        await _multiRunJob.Resume();
                        AnsiConsole.MarkupLine($"[greenyellow]resume at {DateTime.Now}[/]");
                        break;
                    case ConsoleKey.S:
                        await _multiRunJob.Stop();
                        break;
                    case ConsoleKey.A:
                        await _multiRunJob.Abort();
                        break;
                    case ConsoleKey.UpArrow:
                        // don't working, please fix MultiRunJob code
                        //await _multiRunJob.ChangeBots(_multiRunJob.Bots + 1);
                        break;
                    case ConsoleKey.DownArrow:
                        // don't working, please fix MultiRunJob code
                        //await _multiRunJob.ChangeBots(_multiRunJob.Bots - 1);
                        break;
                }
            }
        }
    }
}
