using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

// Used below example for reference
// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/start-multiple-async-tasks-and-process-them-as-they-complete

namespace FileServiceClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static Task Main() => GetFileSizes();

        private static async Task GetFileSizes()
        {
            var stopwatch = Stopwatch.StartNew();

            uint maxThreads = 4;

            var concurrentQueue = new ConcurrentQueue<string>(Directory.EnumerateFiles(@"C:\DemoFolder", "*", SearchOption.AllDirectories));

            List<Task> fileSizeTasks = new List<Task>();

            int filesCount = concurrentQueue.Count;

            for (int i = 0; i < maxThreads; i++)
            {
                fileSizeTasks.Add(Task.Run(async () =>
                {
                    while (concurrentQueue.TryDequeue(out string fileName))
                    {
                        string result = await GetFileSize(fileName);
                        Console.WriteLine($"Finished for {fileName} with {result}");
                    }
                }));
            }

            await Task.WhenAll(fileSizeTasks);

            stopwatch.Stop();
            Console.WriteLine($"\n\n\nTotal files count: {filesCount}.\nElapsed time: {stopwatch.Elapsed}\n");

            Console.ReadLine();
        }

        private static async Task<string> GetFileSize(string fileName)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            HttpResponseMessage response = await client.GetAsync(string.Concat(@"http://localhost:8080/file/size?fileName=", fileName));
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"SUCCESS\t\tFile: {fileName}, Response status: {response.StatusCode}");
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                Console.WriteLine($"FAILURE\t\tFile: {fileName}, Response status: {response.StatusCode}");
                return null;
            }
        }
    }
}
