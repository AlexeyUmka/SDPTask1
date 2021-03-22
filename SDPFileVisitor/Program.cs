using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SDPFileVisitor.Core.Interfaces;
using SDPFileVisitor.Core.Services;

namespace SDPFileVisitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var startPath = "G:\\temporary location";

            var visitor = new FileSystemVisitorService(startPath, x => x.Name.Contains("Output") || x.Name.Contains("Source") || x.Name.Contains("exclude"));
            SubscribeHandlers(visitor);

            try
            {
                var searchResult = visitor.Search().ToList();
                Console.WriteLine($"Search result (found {searchResult.Count} occurrences) :");
                foreach (var foundItem in searchResult)
                {
                    Console.WriteLine(foundItem.FullName);
                }
            }
            catch (DirectoryNotFoundException exception)
            {
                Console.WriteLine("Matched directory wasn't found(");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private static void SubscribeHandlers(IFileSystemVisitorService visitor)
        {
            visitor.SearchStarted += (sender, eventArgs) =>
            {
                eventArgs.Stopwatch = Stopwatch.StartNew();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Search is started");
                Console.ForegroundColor = ConsoleColor.White;
            };
            visitor.SearchFinished += (sender, eventArgs) =>
            {
                eventArgs.Stopwatch.Stop();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Search is ended. Time Elapsed: {eventArgs.Stopwatch.Elapsed}");
                Console.ForegroundColor = ConsoleColor.White;
            };
            visitor.DirectoryFound += (sender, eventArgs) =>
            {
                Console.WriteLine($"Directory has been found: {eventArgs.Name}");
            };
            visitor.FileFound += (sender, eventArgs) =>
            {
                Console.WriteLine($"File has been found: {eventArgs.Name}");
            };
            visitor.DirectoryFiltered += (sender, eventArgs) =>
            {
                Console.WriteLine($"Directory has been filtered: {eventArgs.Name}");
            };
            visitor.FileFiltered += (sender, eventArgs) =>
            {
                Console.WriteLine($"File has been filtered: {eventArgs.Name}");
            };
        }
    }
}
