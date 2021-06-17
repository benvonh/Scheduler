using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerServer
{
    public static class Control
    {
        public static Dictionary<string, TimeSpan[]> Data;

        public static void Init()
        {
            Data = new Dictionary<string, TimeSpan[]>();
        }

        public static Task<string> Run(int minPeople, int latestStartDiff, int earliestEndDiff)
        {
            StringBuilder results = new StringBuilder();

            // Collect user data in an array of events signifying each day
            Event[] events = new Event[31];
            for (int i = 0; i < 62; i += 2)
            {
                events[i / 2] = new Event
                {
                    Date = DateTime.Today.AddDays(i / 2)
                };
                foreach (string name in Data.Keys)
                {
                    // Only adds person if person is free
                    events[i / 2].AddPerson(name, Data[name][i], Data[name][i + 1]);
                }
            }

            // Construct appending message
            foreach (Event event_ in events)
            {
                results.Append(event_.Date.ToString("\ndd/MM "));
                if (event_.isEnoughPeople(minPeople))
                {
                    event_.AverageTimes(minPeople);
                    event_.SortAvailablePeople(latestStartDiff, earliestEndDiff);

                    if (event_.AvailablePeopleCount[0] < minPeople || event_.AvailablePeopleCount[1] < minPeople)
                    {
                        results.AppendFormat("Insufficient time allowed ({0} late start and {1} early finish; Proposed Time [{2} - {3}])",
                            event_.People.Count - event_.AvailablePeopleCount[0],
                            event_.People.Count - event_.AvailablePeopleCount[1],
                            event_.Times[0].ToString("hh':'mm"),
                            event_.Times[1].ToString("hh':'mm"));
                    }
                    else if (event_.Times[0] > event_.Times[1])
                    {
                        results.AppendFormat("Overlapping availabilities (Proposed Time [{0} - {1}])",
                            event_.Times[0].ToString("hh':'mm"),
                            event_.Times[1].ToString("hh':'mm"));
                    }
                    else
                    {
                        results.AppendFormat("[{0} - {1}] ", event_.Times[0].ToString("hh':'mm"), event_.Times[1].ToString("hh':'mm"));

                        int available = Math.Max(event_.AvailablePeopleCount[0], event_.AvailablePeopleCount[1]);
                        results.AppendFormat("{0} {1} can make it (",
                            available,
                            available > 1 ? "people" : "person");
                        results.AppendJoin(", ", event_.AvailablePeopleNames);
                        results.Append(')');

                        if (event_.ExceptionsNames.Count > 0)
                        {
                            results.Append(". NOTE: ");
                            results.AppendJoin(", ", event_.ExceptionsNames);
                        }
                    }
                }
                else
                    results.AppendFormat("Not enough people available ({0} out of {1})", event_.People.Count, minPeople.ToString());

                results.AppendLine();
            }

            return Task.FromResult(results.ToString());
        }
    }
}
