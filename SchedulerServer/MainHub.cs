using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerServer
{
    public class MainHub : Hub
    {
        public async void Send(string name, string json)
        {
            try
            {
                List<DateTime> availability = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DateTime>>(json);
                TimeSpan[] times = new TimeSpan[62];
                for (int i = 0; i < 62; i++)
                {
                    DateTime current = availability[i];
                    times[i] = new TimeSpan(current.Hour, current.Minute, current.Second);
                }
                Control.Data.Add(name, times);
                await Clients.Caller.SendAsync("Recieve", "Your availability has been successfully uploaded to the server.");
            }
            catch (ArgumentException)
            {
                await Clients.Caller.SendAsync("Update", "This name already exists on the server. Would you like to update your availability?");
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Recieve", ex.Message);
            }
        }

        public async void Update(string name, string json)
        {
            try
            {
                List<DateTime> availability = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DateTime>>(json);
                TimeSpan[] times = new TimeSpan[62];
                for (int i = 0; i < 62; i++)
                {
                    DateTime current = availability[i];
                    times[i] = new TimeSpan(current.Hour, current.Minute, current.Second);
                }

                if (Control.Data.Remove(name))
                {
                    Control.Data.Add(name, times);
                    await Clients.Caller.SendAsync("Recieve", "Your availability has been successfully updated.");
                }
                else
                    throw new Exception("Error locating data under " + name);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Recieve", ex.Message);
            }
        }

        public async void Run(int minPeople, int latestStartDiff, int earliestEndDiff)
        {
            try
            {
                await Clients.All.SendAsync("Recieve", "Server algorithm initiaited by admin console...");
                string results = await Control.Run(minPeople, latestStartDiff, earliestEndDiff);
                await Clients.All.SendAsync("Recieve", results);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Recieve", ex.StackTrace);
                await Clients.All.SendAsync("Recieve", ex.Message);
            }
        }

        public async void Get(string mode)
        {
            if (Control.Data.Count < 1)
            {
                await Clients.Caller.SendAsync("Recieve", "Server data empty.");
                return;
            }
            switch (mode)
            {
                case "name":
                    await Clients.Caller.SendAsync("Recieve", string.Join('\n', Control.Data.Keys.ToArray()));
                    break;
                case "availability":
                    string msg = string.Empty;
                    foreach (TimeSpan[] times in Control.Data.Values)
                    {
                        msg += "\n";
                        msg += new string('-', 31 * 6);
                        msg += "\n";
                        for (int i = 0; i < 62; i += 2)
                        {
                            msg += "|" + times[i].ToString("hh':'mm");
                        }
                        msg += "\n";
                        for (int i = 1; i < 62; i += 2)
                        {
                            msg += "|" + times[i].ToString("hh':'mm");
                        }
                        msg += "\n";
                    }
                    await Clients.Caller.SendAsync("Recieve", msg);
                    break;
                case "all":
                    msg = string.Empty;
                    int count = 0;
                    foreach (TimeSpan[] times in Control.Data.Values)
                    {
                        msg += "\n";
                        msg += Control.Data.Keys.ToArray()[count];
                        msg += new string('-', 31 * 6);
                        msg = msg.Substring(0, msg.Length - Control.Data.Keys.ToArray()[count++].Length);
                        msg += "\n";
                        for (int i = 0; i < 62; i += 2)
                        {
                            msg += '|' + times[i].ToString("hh':'mm");
                        }
                        msg += "\n";
                        for (int i = 1; i < 62; i += 2)
                        {
                            msg += '|' + times[i].ToString("hh':'mm");
                        }
                        msg += "\n";
                    }
                    await Clients.Caller.SendAsync("Recieve", msg);
                    break;
            }
        }

        public async void Del(string mode)
        {
            if (Control.Data.Count < 1)
            {
                await Clients.Caller.SendAsync("Recieve", "Server data empty.");
                return;
            }
            if (mode == "all")
            {
                Control.Data.Clear();
                await Clients.All.SendAsync("Recieve", "Server data has been cleared.");
            }
            else
            {
                string msg;
                if (Control.Data.Remove(mode))
                    msg = mode + " has been deleted.";
                else
                    msg = mode + " could not be found.";

                await Clients.Caller.SendAsync("Recieve", msg);
            }
        }
    }
}
