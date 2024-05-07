using ChartJSCore.Helpers;
using ChartJSCore.Models;
using ChartJSCore.Plugins.Zoom;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Common;


namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    public class GraphController : Controller
    {
        private Dictionary<string, ChartColor> ColorByName = new Dictionary<string, ChartColor>()
        {
            { "Red", ChartColor.FromRgba(255, 99, 132, 0.2) },
            { "Blue", ChartColor.FromRgba(54, 162, 235, 0.2) },
            { "Yellow", ChartColor.FromRgba(255, 206, 86, 0.2) },
            { "Green", ChartColor.FromRgba(75, 192, 192, 0.2) },
            { "Purple", ChartColor.FromRgba(153, 102, 255, 0.2) },
            { "Orange", ChartColor.FromRgba(255, 159, 64, 0.2) }
        };

        // GET: GraphController
        public IActionResult Index(string token)
        {
            var type = HttpContext.Session.GetString("DataType")
                ?? throw new NullReferenceException($"{HttpContext.Session} : DataType");

            Chart chart = null;

            switch(type)
            {
                case "Reps":
                    chart = GenerateVerticalBarChart(token);
                    break;
                case "Timed":
                    chart = GenerateLineChart(token);
                    break;
                default:
                    return RedirectToAction("Index", "ExerciseData");
            }
            

            ViewData["chart"] = chart;

            return View();
        }


        private Chart GenerateLineChart(string token)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(token);

            var datestrings = new List<string>();
            var times = new List<double?>();
            foreach (var kvp in dict)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                datestrings.Add(key.ToString());
                times.Add(value.TotalSeconds);
            }

            Chart chart = new Chart();

            chart.Type = Enums.ChartType.Line;
            /*chart.Options.Scales = new Dictionary<string, Scale>();
            CartesianScale xAxis = new CartesianScale();
            xAxis.Display = true;
            xAxis.Title = new Title
            {
                Text = new List<string> { "Month" },
                Display = true
            };
            chart.Options.Scales.Add("x", xAxis);*/


            Data data = new Data
            {
                Labels = datestrings
            };

            LineDataset dataset = new LineDataset()
            {
                Label = "Time",
                Data = times,
                Fill = "true",
                Tension = .01,
                BackgroundColor = [ColorByName["Green"]],
                BorderColor = [ColorByName["Green"]],
                BorderCapStyle = "butt",
                BorderDash = new List<int>(),
                BorderDashOffset = 0.0,
                BorderJoinStyle = "miter",
                PointBorderColor = [ColorByName["Green"]],
                PointBackgroundColor = new List<ChartColor> { ChartColor.FromHexString("#ffffff") },
                PointBorderWidth = new List<int> { 1 },
                PointHoverRadius = new List<int> { 5 },
                PointHoverBackgroundColor = [ColorByName["Green"]],
                PointHoverBorderColor = new List<ChartColor> { ChartColor.FromRgb(220, 220, 220) },
                PointHoverBorderWidth = new List<int> { 2 },
                PointRadius = new List<int> { 1 },
                PointHitRadius = new List<int> { 10 },
                SpanGaps = false
            };

            data.Datasets = new List<Dataset>
            {
                dataset
            };

            chart.Data = data;

            ZoomOptions zoomOptions = new ZoomOptions
            {
                Zoom = new Zoom
                {
                    Wheel = new Wheel
                    {
                        Enabled = true
                    },
                    Pinch = new Pinch
                    {
                        Enabled = true
                    },
                    Drag = new Drag
                    {
                        Enabled = true,
                        ModifierKey = Enums.ModifierKey.alt
                    }
                },
                Pan = new Pan
                {
                    Enabled = true,
                    Mode = "xy"
                }
            };

            chart.Options.Plugins = new Plugins
            {
                PluginDynamic = new Dictionary<string, object> { { "zoom", zoomOptions } }
            };

            return chart;
        }

        private Chart GenerateVerticalBarChart(string token)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(token);

            var datestrings = new List<string>();
            var sets = new List<double?>();
            var reps = new List<double?>();
            foreach (var kvp in dict!)
            {
                var key = kvp.Key;
                var value = kvp.Value;
                datestrings.Add(key.ToString());
                sets.Add(value.Item1);
                reps.Add(value.Item2);
            }

            Chart chart = new Chart
            {
                Type = Enums.ChartType.Bar
            };


            Data data = new Data
            {
                Labels = datestrings
            };

            BarDataset set_data = new BarDataset()
            {
                Label = "Sets",
                Data = sets,
                BackgroundColor = [ColorByName["Red"]],
                BorderColor = [ColorByName["Red"]],
                BorderWidth = new List<int>() { 1 },
                BarPercentage = 0.5,
                BarThickness = 10,
                MaxBarThickness = 8,
                MinBarLength = 2
            };

            BarDataset rep_data = new BarDataset()
            {
                Label = "Reps",
                Data = reps,
                BackgroundColor = [ColorByName["Blue"]],
                BorderColor = [ColorByName["Blue"]],
                BorderWidth = new List<int>() { 1 },
                BarPercentage = 0.5,
                BarThickness = 10,
                MaxBarThickness = 8,
                MinBarLength = 2
            };

            data.Datasets = [set_data, rep_data];

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

    }
}
