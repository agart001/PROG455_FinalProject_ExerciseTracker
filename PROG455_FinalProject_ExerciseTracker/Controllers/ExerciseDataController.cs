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

                var data = nodedata![0]!.ToString();
                var test = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(data);

                HttpContext.Session.SetString("DataID", nodeid?.ToString()!);
                var dataid = int.Parse(nodeid?.ToString()!);




                switch (type.ToString())
                {
                    case "Unknown": return View(new ExerciseDataWrapper<string>(new(dataid,
                            JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(data!)!)));
                    case "Reps": return View(new ExerciseDataWrapper<Tuple<int, int>>(new(dataid,
                            JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(data!)!)));
                    case "Timed": return View(new ExerciseDataWrapper<TimeSpan>(new(dataid,
                            JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(data!)!)));
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
            switch (type)
            {
                case "Tuple`2": return View(new ExerciseDateForm<Tuple<int, int>>());
                case "TimeSpan": return View(new ExerciseDateForm<TimeSpan>());
                case "String": return View(new ExerciseDateForm<string>());
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
                var data = JObject.Parse(node["Data"]!.ToString());

                var type = (string)collection["DataType"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : DataType : string");

                var date = DateTime.Parse(collection["Date"]!);

                JToken token = null;
                switch (type)
                {
                    case "Tuple`2": token = RepsDataToken(collection); break;
                    case "TimeSpan": token = TimedDataToken(collection); break;
                    default: token = UnknownDataToken(collection); break;
                }

                data[date.ToString("yyyy-MM-ddTHH:mm:ssK")] = token;

                node["Data"] = data;

                var newdata = node.ToString(Formatting.None);

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "query",
                        Hasher.UTF8Encode(new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = $"UPDATE ExerciseData SET Data = [{newdata}] WHERE UserID = '{userid}' " +
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

        private JToken RepsDataToken(IFormCollection collection)
        {
            try
            {
                var sets = int.Parse(collection["Sets"]!);
                var reps = int.Parse(collection["Reps"]!);

                return JToken.FromObject(new Tuple<int, int>(sets, reps));
            }
            catch
            {
                throw new Exception();
            }
        }

        private JToken TimedDataToken(IFormCollection collection)
        {
            try
            {
                var time = TimeSpan.Parse(collection["Time"]!);

                return JToken.FromObject(time.ToString());
            }
            catch
            {
                throw new Exception();
            }
        }

        private JToken UnknownDataToken(IFormCollection collection)
        {
            try
            {
                var value = (string)collection["Value"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Value : string");

                return JToken.FromObject(value);
            }
            catch
            {
                throw new Exception();
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
