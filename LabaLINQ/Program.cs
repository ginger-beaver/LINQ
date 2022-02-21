using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabaLINQ
{
    delegate string PrintResult();

    internal static class Program
    {
        private static List<WeatherEvent> _parsedDataEvents;
        private static void ParsingData()
        {
            var parsedData = new List<WeatherEvent>();
            using (var sr = new StreamReader(@"BIGdata.csv"))
            {
                while (!sr.EndOfStream)
                {
                    var weatherEvent = new WeatherEvent();
                    var line = sr.ReadLine();
                    if (line ==
                        "EventId,Type,Severity,StartTime(UTC),EndTime(UTC),TimeZone,AirportCode,LocationLat,LocationLng,City,County,State,ZipCode")
                        continue;
                    var splitLine = line?.Split(",");

                    weatherEvent.EventId = splitLine![0];
                    weatherEvent.Type = (Type) Enum.Parse(typeof(Type), splitLine[1]);
                    weatherEvent.Severity = (Severity) Enum.Parse(typeof(Severity), splitLine[2]);
                    weatherEvent.StartTime = DateTimeOffset.Parse(splitLine[3]);
                    weatherEvent.EndTime = DateTimeOffset.Parse(splitLine[4]);
                    weatherEvent.TimeZone = splitLine[5];
                    weatherEvent.AirportCode = splitLine[6];
                    weatherEvent.LocationLat = splitLine[7];
                    weatherEvent.LocationLng = splitLine[8];
                    weatherEvent.City = splitLine[9];
                    weatherEvent.County = splitLine[10];
                    weatherEvent.State = splitLine[11];
                    weatherEvent.ZipCode = splitLine[12];

                    parsedData.Add(weatherEvent);
                }
            }

            _parsedDataEvents = parsedData;
        }
        
        
        static void Main()
        {
            ParsingData();
            
            var list = new List<PrintResult>
            {
                Request0, Request1, Request2, Request3,
                Request4, Request5, Request6
            };
            
            foreach (var printResult in list)
            {
                Console.WriteLine(printResult.Method.Name);
                Console.WriteLine(printResult.Invoke());
            }
            
            CheckTime(list);
        }

        private static void CheckTime(List<PrintResult> list)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var printResult in list)
            {
                printResult.Invoke();
            }

            stopWatch.Stop();
            Console.WriteLine($"Foreach: {stopWatch.Elapsed.TotalSeconds:F3} seconds in total.\n");
            var sumTimeSpanForEach = stopWatch.Elapsed;
            stopWatch.Reset();


            stopWatch.Start();
            Parallel.ForEach(Enumerable.Range(0, 7), i => { list[i].Invoke(); });
            stopWatch.Stop();
            var sumTimeSpanParallel = stopWatch.Elapsed;
            Console.WriteLine($"Parallel: {stopWatch.Elapsed.TotalSeconds:F3} seconds in total.\n");
            stopWatch.Reset();

            stopWatch.Start();
            var tasks = new Task[7];
            for (var i = 0; i < tasks.Length; i++)
            {
                var i1 = i;
                tasks[i] = new Task(() => { list[i1].Invoke(); });
                tasks[i].Start();
            }
            
            Task.WaitAll(tasks);
            stopWatch.Stop();
            var sumTimeSpanTask = stopWatch.Elapsed;
            Console.WriteLine($"Task: {stopWatch.Elapsed.TotalSeconds:F3} seconds in total.");
            stopWatch.Reset();

            Console.WriteLine("Parallel быстрее foreach на " +
                              $"{(sumTimeSpanForEach - sumTimeSpanParallel) / sumTimeSpanForEach * 100:F3} %");
            
            Console.WriteLine("Task быстрее foreach на " +
                              $"{(sumTimeSpanForEach - sumTimeSpanTask) / sumTimeSpanForEach * 100:F3} %");
        }

        private static string Request0()
        {
            var count = _parsedDataEvents.Count(x => x.StartTime.Year == 2018);
            return $"Количество зафиксированных природных явлений в Америке в 2018 году: {count}. \n";
        }

        private static string Request1()
        {
            var numberOfStates = _parsedDataEvents.Select(x => x.State).Distinct().Count();
            var numberOfCities = _parsedDataEvents.Select(x => x.City).Distinct().Count();

            return $"Количество штатов: {numberOfStates}. Количество городов: {numberOfCities}. \n";
        }

        private static string Request2()
        {
            var builder = new StringBuilder();
            var enumerable = _parsedDataEvents.Where(x => x.StartTime.Year == 2019 && x.Type == Type.Rain)
                .GroupBy(arg => arg.City)
                .Select(x => new
            {
                Name = x.Key,
                Count = x.Count(weatherEvent => weatherEvent.Type == Type.Rain)
            })
                .OrderByDescending(x => x.Count)
                .Take(3);
            
            var i = 0;
            foreach (var city in enumerable)
            {
                builder.AppendLine($"{++i}. {city.Name} - {city.Count}.");
            }

            return builder.ToString();
        }

        private static string Request3()
        {
            var builder = new StringBuilder();
            var distinct = _parsedDataEvents.Where(x => x.Type == Type.Snow)
                .Select(y => new
                {
                    y.StartTime.Year,
                    y.StartTime,
                    y.EndTime,
                    Time = y.EndTime - y.StartTime,
                    y.City
                }).OrderByDescending(z => z.Year)
                .ThenByDescending(z => z.Time)
                .GroupBy(element => element.Year)
                .Select(groups => groups.OrderByDescending(p => p.Time).First());

            foreach (var year in distinct)
            {
                builder.AppendLine($"{year.Year}: в {year.City} c {year.StartTime} по {year.EndTime}.");
            }

            return builder.ToString();
        }

        private static string Request4()
        {
            var builder = new StringBuilder();
            var count = _parsedDataEvents.Where(x => x.StartTime.Year == 2019)
                .GroupBy(arg => arg.State)
                .Select(most => new
                {
                    State = most.Key,
                    Number = most.TakeWhile(x => (x.EndTime - x.StartTime).Hours <= 2).Count()
                }).OrderByDescending(x => x.Number)
                .ToList();

            foreach (var state in count)
            {
                builder.AppendLine($"Количество событий в штате {state.State}: {state.Number}.");
            }

            return builder.ToString();
        }
        
        private static string Request5()
        {
            var builder = new StringBuilder();
            var most = _parsedDataEvents
                .Where(arg => arg.StartTime.Year == 2017 && arg.Severity == Severity.Severe)
                .GroupBy(arg => arg.State);

            foreach (var list in most)
            {
                var weatherEvent = list.OrderByDescending(x => x.EndTime - x.StartTime).First();
                builder.AppendLine($"В штате {list.Key} город {weatherEvent.City} " +
                                   "с максимальной суммарной длительностью" +
                                   $" сильных погодных явлений {(weatherEvent.EndTime - weatherEvent.StartTime).Hours} часов.");
            }
            
            return builder.ToString();
        }
        
        private static string Request6()
        {
            
                var groupedByYear = _parsedDataEvents
                    .GroupBy(x => x.StartTime.Year);
                var builder = new StringBuilder();
    
                foreach (var group in groupedByYear)
                {
                    var groupedByCount = group
                        .GroupBy(x => x.Type).
                        OrderBy(x => x.Count())
                        .ToList();

                    var least = groupedByCount.First();
                    var timeLeast = new TimeSpan();
                    foreach (var weatherEvent in least)
                    {
                        timeLeast += (weatherEvent.EndTime - weatherEvent.StartTime);
                    }
                    var averageLeast = (timeLeast / least.Count()).TotalHours;

                    var most = groupedByCount.Last();
                    var timeMost = new TimeSpan();
                    foreach (var weatherEvent in most)
                    {
                        timeMost += (weatherEvent.EndTime - weatherEvent.StartTime);
                    }
                    var averageMost = (timeMost / most.Count()).TotalHours;


                    builder.AppendLine(
                        $"{group.Key}: самый частый тип события - {most.Key}, среднее время - {averageMost:F3} часов; " +
                        $"самый редкий тип события - {least.Key}, среднее время - {averageLeast:F3} часов.");
                }

                return builder.ToString();
        }
    }
}