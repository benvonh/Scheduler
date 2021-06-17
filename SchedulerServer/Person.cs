using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerServer
{
    public class Person
    {
        public string Name { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class Event
    {
        public List<Person> People { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan[] Times { get; private set; }
        public int[] AvailablePeopleCount { get; private set; }
        public List<string> AvailablePeopleNames { get; private set; }
        public List<string> ExceptionsNames { get; private set; }

        public Event()
        {
            People = new List<Person>();
            AvailablePeopleCount = new int[2];
            AvailablePeopleNames = new List<string>();
            ExceptionsNames = new List<string>();
        }

        public void AddPerson(string name, TimeSpan startTime, TimeSpan endTime)
        {
            if (startTime < endTime)
                People.Add(new Person { Name = name, StartTime = startTime, EndTime = endTime } );
        }
        
        public bool isEnoughPeople(int requirement)
        {
            return People.Count >= requirement;
        }

        public void AverageTimes(int minPeople)
        {
            //Times = new TimeSpan[2];
            //foreach (Person person in People)
            //{
            //    Times[0] = Times[0].Add(person.StartTime);
            //    Times[1] = Times[1].Add(person.EndTime);
            //}
            //Times[0] = Times[0].Divide(People.Count);
            //Times[1] = Times[1].Divide(People.Count);

            List<TimeSpan> startTimes = new List<TimeSpan>();
            List<TimeSpan> endTimes = new List<TimeSpan>();
            foreach (Person person in People)
            {
                startTimes.Add(person.StartTime);
                endTimes.Add(person.EndTime);
            }
            startTimes.Sort();
            endTimes.Sort();

            int indexer = minPeople;
            if (minPeople > startTimes.Count)
                indexer = startTimes.Count;
            Times = new TimeSpan[2]
            {
                startTimes[indexer - 2],
                endTimes[indexer - 2]
            };
        }

        public void SortAvailablePeople(int latestStartDiff, int earliestEndDiff)
        {
            foreach (Person person in People)
            {
                int flexibility = 0;
                if (person.StartTime <= Times[0].Add(new TimeSpan(latestStartDiff, 0, 0)))
                {
                    AvailablePeopleCount[0]++;
                    flexibility += 1;
                }
                if (person.EndTime >= Times[1].Subtract(new TimeSpan(earliestEndDiff, 0, 0)))
                {
                    AvailablePeopleCount[1]++;
                    flexibility += 2;
                }
                switch (flexibility)
                {
                    case 1:
                        int hours = (Times[1] - person.EndTime).Hours;
                        int mins = (Times[1] - person.EndTime).Minutes;
                        ExceptionsNames.Add(string.Format("{0} will finish {1} {2} {3} early",
                            person.Name,
                            hours,
                            hours > 1 ? "hrs" : "hr",
                            mins > 0 ? string.Format("and {0} {1}", mins, mins > 1 ? "mins" : "min") : string.Empty));
                        break;
                    case 2:
                        hours = (person.StartTime - Times[0]).Hours;
                        mins = (person.StartTime - Times[0]).Minutes;
                        ExceptionsNames.Add(string.Format("{0} will start {1} {2} {3} late.",
                            person.Name,
                            hours,
                            hours > 1 ? "hrs" : "hr",
                            mins > 0 ? string.Format("and {0} {1}", mins, mins > 1 ? "mins" : "min") : string.Empty));
                        break;
                }
                if (flexibility != 0)
                    AvailablePeopleNames.Add(person.Name);
            }
        }
    }
}
