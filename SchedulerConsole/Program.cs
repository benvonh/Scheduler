using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchedulerConsole
{
    class Program
    {
        private static HubConnection connection;
        private static string url = "http://neuotec.com:51926/mainhub";

        static async Task Main(string[] args)
        {
            START:
            Console.Clear();

            Console.WriteLine("Establishing connection with server...");

            try
            {
                if (connection == null)
                {
                    connection = new HubConnectionBuilder().WithUrl(url).Build();
                    connection.Closed += (error) => { Console.WriteLine("Connection lost..."); return Task.CompletedTask; };
                    connection.Reconnected += (id) => { Console.WriteLine("Reconnected"); return Task.CompletedTask; };
                    connection.On<string>("Recieve", async (msg) =>
                    {
                        Console.WriteLine("\n" + msg + "\n");
                        //await Temp(msg);
                    });
                    await connection.StartAsync();
                }
                    
                Console.WriteLine("Connection established\n");

                while (true)
                {
                    string input = Console.ReadLine();

                    if (input == "help")
                    {
                        Console.WriteLine("");
                        Console.WriteLine("get commands:");
                        Console.WriteLine(" -> name (retrieves all names stored on server)");
                        Console.WriteLine(" -> availability (retrieves all availability stored on server)");
                        Console.WriteLine(" -> all (retrieves both name and their availability on server)");
                        Console.WriteLine("");
                        Console.WriteLine("del commands:");
                        Console.WriteLine(" -> [name] (deletes the user data matching the given name)");
                        Console.WriteLine(" -> all (deletes all server data)");
                        Console.WriteLine("");
                        Console.WriteLine("more commands:");
                        Console.WriteLine(" -> url (modify the url address to which the hub connection is established with)");
                        Console.WriteLine(" -> run (runs the server algorithm)");
                        Console.WriteLine("");
                    }

                    else if (input.Length > 4 && input[0..4] == "get ")
                    {
                        await connection.SendAsync("Get", input[4..]);
                    }

                    else if (input.Length > 4 && input[0..4] == "del ")
                        await connection.SendAsync("Del", input[4..]);

                    else if (input == "run")
                    {
                        string query1 = "Minimum number of people to attend:";
                        string query2 = "Latest start difference (latest someone can join an event in hours):";
                        string query3 = "Earliest end difference (earliest someone can leave an event in hours):";
                        Console.WriteLine(query1);
                        int minPeople = int.Parse(Console.ReadLine());
                        Console.WriteLine(query2);
                        int latestStartDiff = int.Parse(Console.ReadLine());
                        Console.WriteLine(query3);
                        int earliestEndDiff = int.Parse(Console.ReadLine());
                        await connection.SendAsync("Run", minPeople, latestStartDiff, earliestEndDiff);
                    }
                    else if (input == "url")
                    {
                        Console.WriteLine("\nOld url: {0}", url);
                        Console.WriteLine("\nEnter new url:");
                        url = Console.ReadLine();
                        await connection.DisposeAsync();
                        connection = null;
                        goto START;
                    }
                    else
                        Console.WriteLine("\nInvalid command\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nERROR: {0}\n", ex.Message);
                Console.WriteLine("\nSTACK TRACE: {0}\n", ex.StackTrace);
                ConsoleKey key = Console.ReadKey().Key;
                if (key is ConsoleKey.C)
                {
                    Console.WriteLine("\nOld url: {0}", url);
                    Console.WriteLine("\nEnter new url:");
                    url = Console.ReadLine();
                    await connection.DisposeAsync();
                    connection = null;
                }
                goto START;
            }
        }

        static Task Temp(string msg)
        {
            Console.WriteLine("Run intercept?");
            string k = Console.ReadLine();
            if (k != "y")
                return Task.CompletedTask;

            Dictionary<string, TimeSpan[]> dict = new Dictionary<string, TimeSpan[]>();
            List<string> names = new List<string>();
            List<TimeSpan> times = new List<TimeSpan>();
            foreach (string line in msg.Split("\n"))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.Contains('-'))
                    names.Add(line.Substring(0, line.IndexOf('-')));

                else
                {
                    foreach (string time in line.Split('|'))
                    {
                        if (!string.IsNullOrEmpty(time))
                            times.Add(new TimeSpan(int.Parse(time.Substring(0, 2)), int.Parse(time.Substring(3, 2)), 0));
                    }
                }
            }

            return Task.CompletedTask;
        } 
    }
}
