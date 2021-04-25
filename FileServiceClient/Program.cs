using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            IEnumerable<Task<string>> fileSizesQuery =
                from fileName in Directory.EnumerateFiles(@"C:\DemoFolder", "*", SearchOption.AllDirectories)
                select GetFileSize(fileName);

            List<Task<string>> fileSizeTasks = fileSizesQuery.ToList();

            int filesCount = 0;
            while (fileSizeTasks.Any())
            {
                Task<string> finishedTask = await Task.WhenAny(fileSizeTasks);
                fileSizeTasks.Remove(finishedTask);
                string result = await finishedTask;
                Console.WriteLine($"Finished for {result}");
                filesCount += 1;
            }

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
