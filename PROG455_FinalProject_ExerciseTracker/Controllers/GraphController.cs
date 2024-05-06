using ChartJSCore.Helpers;
using ChartJSCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;


namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    public class GraphController : Controller
    {
        // GET: GraphController
        public IActionResult Index(string token)
        {
            var type = HttpContext.Session.GetString("DataType")
                ?? throw new NullReferenceException($"{HttpContext.Session} : DataType");

            Chart chart = null;

            switch(type)
            {
                case "Reps":
                    chart = RepsChart(token);
                    break;
                case "Timed":
                    chart = GenerateVerticalBarChart(token);
                    break;
                default:
                    break;
            }
            

            ViewData["chart"] = chart;

            return View();
        }

        private Chart RepsChart(string token)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(token);

            var datestrings = new List<string>();
            var sets = new List<double?>();
            var reps = new List<double?>();
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                datestrings.Add(key.ToString());
                sets.Add(value.Item1);
                reps.Add(value.Item2);
            }


            Chart chart = new Chart();

            chart.Type = Enums.ChartType.Line;

            Data data = new Data();
            data.Labels = datestrings;

            LineDataset set_dataset = new LineDataset()
            {
                Label = "Sets",
                Data = sets,
                Fill = "false",
                Tension = 0.1,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(75, 100, 100, 0.4) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 100, 100) },
                BorderCapStyle = "butt",
                BorderDash = new List<int> { },
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 100, 100) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 100, 100) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            LineDataset rep_dataset = new LineDataset()
            {
                Label = "Reps",
                Data = reps,
                Fill = "false",
                Tension = 0.1,
                BackgroundColor = new List<ChartColor> { ChartColor.FromRgba(75, 192, 192, 0.4) },
                BorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                BorderCapStyle = "butt",
                BorderDash = new List<int> { },
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = new List<ChartColor> { ChartColor.FromRgb(75, 192, 192) },
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = [set_dataset, rep_dataset];

            chart.Data = data;

            return chart;
        }

        private static Chart GenerateVerticalBarChart(string token)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(token);

            var datestrings = new List<string>();
            var times = new List<double?>();
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                datestrings.Add(key.ToString());
                
                var dt = Convert.ToDouble(value);
                times.Add(dt);
            }

            Chart chart = new Chart();
            chart.Type = Enums.ChartType.Bar;


            Data data = new Data();
            data.Labels = datestrings;

            BarDataset dataset = new BarDataset()
            {
                Label = "# of Votes",
                Data = times,
                BackgroundColor = new List<ChartColor>
                {
                    ChartColor.FromRgba(255, 99, 132, 0.2),
                    ChartColor.FromRgba(54, 162, 235, 0.2),
                    ChartColor.FromRgba(255, 206, 86, 0.2),
                    ChartColor.FromRgba(75, 192, 192, 0.2),
                    ChartColor.FromRgba(153, 102, 255, 0.2),
                    ChartColor.FromRgba(255, 159, 64, 0.2)
                },
                BorderColor = new List<ChartColor>
                {
                    ChartColor.FromRgb(255, 99, 132),
                    ChartColor.FromRgb(54, 162, 235),
                    ChartColor.FromRgb(255, 206, 86),
                    ChartColor.FromRgb(75, 192, 192),
                    ChartColor.FromRgb(153, 102, 255),
                    ChartColor.FromRgb(255, 159, 64)
                },
                BorderWidth = new List<int>() { 1 },
                BarPercentage = 0.5,
                BarThickness = 6,
                MaxBarThickness = 8,
                MinBarLength = 2
            };

            data.Datasets = new List<Dataset>();
            data.Datasets.Add(dataset);

            chart.Data = data;

            var options = new Options
            {
                Scales = new Dictionary<string, Scale>()
                {
                    { "y", new CartesianLinearScale()
                        {
                            BeginAtZero = true
                        }
                    },
                    { "x", new Scale()
                        {
                            Grid = new Grid()
                            {
                                Offset = true
                            }
                        }
                    },
                }
            };

            chart.Options = options;

            chart.Options.Layout = new Layout
            {
                Padding = new Padding
                {
                    PaddingObject = new PaddingObject
                    {
                        Left = 10,
                        Right = 12
                    }
                }
            };

            return chart;
        }

        // GET: GraphController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: GraphController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: GraphController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: GraphController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: GraphController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: GraphController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: GraphController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
