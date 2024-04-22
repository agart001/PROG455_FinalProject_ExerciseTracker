using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PROG455_FinalProject_ExerciseTracker.Models;
using System.Reflection;
using System.Text.Json.Nodes;

namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    public class ExerciseDataController : Controller
    {
        private static API api;
        // GET: ExerciseDataController
        public async Task<ActionResult> Index()
        {
            try
            {
                api = new(HttpContext.Session.GetString("API")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : API"));

                var userid = HttpContext.Session.GetString("UserID")
                ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                var exerciseid = HttpContext.Session.GetString("ExerciseID")
                        ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "query",
                        Hasher.UTF8Encode(new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = $"SELECT DataType FROM Exercises WHERE UserID = '{userid}' AND ID = '{exerciseid}'"
                        })
                    }
                });

                var dt_res = (JObject)JArray.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
                var type = dt_res.GetValue("DataType");
                if (type == null) throw new NullReferenceException($"{nameof(type)} : Result Null");


                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "query",
                        Hasher.UTF8Encode(new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = $"SELECT ID, Data FROM ExerciseData WHERE UserID = '{userid}' AND ExerciseID = '{exerciseid}'"
                        })
                    }
                });

                //parse is messed up, not deserializing, may have to change default value(prev update may have messed it up)
                var noderes = (JObject)JArray.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
                if (noderes == null) throw new NullReferenceException($"{nameof(noderes)} : Result Null");

                var nodeid = noderes.GetValue("ID");
                var nodedata = noderes.GetValue("Data");

                var data = nodedata!.ToString();
                var test = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(data);

                HttpContext.Session.SetString("DataID", nodeid?.ToString()!);
                var dataid = int.Parse(nodeid?.ToString()!);



                ViewData["DataType"] = type.ToString();
                switch (type.ToString())
                {
                    case "Reps":
                        var settings = new JsonSerializerSettings
                        {
                            Converters = { new TupleConverter() }
                        };
                        return View(new ExerciseDataWrapper<Tuple<int, int>>(new(dataid,
                            JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(data!)!)));
                    case "Timed": return View(new ExerciseDataWrapper<TimeSpan>(new(dataid,
                            JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(data!)!)));
                    case "Unknown": return View(new ExerciseDataWrapper<string>(new(dataid,
                            JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(data!)!)));
                    default: return View();
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: ExerciseDataController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ExerciseDataController/Create
        public ActionResult Create(string type)
        {
            TempData["DataType"] = type;
            ViewData["DataType"] = type;
            switch (type)
            {
                case "Reps": return View(new ExerciseDateForm<Tuple<int, int>>());
                case "Timed": return View(new ExerciseDateForm<TimeSpan>());
                case "Unknown": return View(new ExerciseDateForm<string>());
                default:
                    return View();
            }
        }

        // POST: ExerciseDataController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                var userid = HttpContext.Session.GetString("UserID")
                ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                var exerciseid = HttpContext.Session.GetString("ExerciseID")
                        ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

                var dataid = HttpContext.Session.GetString("DataID")
                        ?? throw new NullReferenceException($"{HttpContext.Session} : DataID");

                var type = (string)TempData["DataType"]!
                        ?? throw new NullReferenceException($"{TempData} : DataType");

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "query",
                        Hasher.UTF8Encode(new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = $"SELECT Data FROM ExerciseData WHERE UserID = '{userid}' " +
                            $"AND ExerciseID = '{exerciseid}' AND ID = '{dataid}'"
                        })
                    }
                });

                var node = (JObject)JArray.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
                var data = node.GetValue("Data")!.ToString();

                var date = DateTime.Parse(collection["Date"]!);

                string newdata;
                switch (type)
                {
                    case "Reps": newdata = RepsDataToken(collection, data, date); break;
                    case "Timed": newdata = TimedDataToken(collection, data, date); break;
                    default: newdata = UnknownDataToken(collection, data, date); break;
                }



                /*data[date.ToString("yyyy-MM-ddTHH:mm:ssK")] = token;

                node["Data"] = data;

                var newdata = node.ToString(Formatting.None);*/
                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "non-query",
                        Hasher.UTF8Encode(new
                        {
                            Table = "PROG455_FP",
                            Query = $"UPDATE ExerciseData SET Data = '{newdata}' WHERE UserID = '{userid}' " +
                            $"AND ExerciseID = '{exerciseid}' AND ID = '{dataid}'"
                        })
                    }
                });

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private string RepsDataToken(IFormCollection collection, string jdict, DateTime date)
        {
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(jdict);
                var sets = int.Parse(collection["Sets"]!);
                var reps = int.Parse(collection["Reps"]!);

                dict!.Add(date, new Tuple<int, int>(sets, reps));

                return JsonConvert.SerializeObject(dict, Formatting.None);
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing Reps data", ex);
            }
        }

        private string TimedDataToken(IFormCollection collection, string jdict, DateTime date)
        {
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(jdict);
                var time = TimeSpan.Parse(collection["Time"]!);

                dict!.Add(date, time);

                return JsonConvert.SerializeObject(dict, Formatting.None);
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing Timed data", ex);
            }
        }

        private string UnknownDataToken(IFormCollection collection, string jdict, DateTime date)
        {
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(jdict);
                var value = (string)collection["Value"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Value : string");

                dict!.Add(date, value);

                return JsonConvert.SerializeObject(dict, Formatting.None);
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing Unknown data", ex);
            }
        }

        // GET: ExerciseDataController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ExerciseDataController/Edit/5
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

        // GET: ExerciseDataController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ExerciseDataController/Delete/5
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
