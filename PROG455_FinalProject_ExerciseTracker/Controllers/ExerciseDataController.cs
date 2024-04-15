using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PROG455_FinalProject_ExerciseTracker.Models;
using System.Text.Json.Nodes;

namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    public class ExerciseDataController : Controller
    {
        private static API api;
        // GET: ExerciseDataController
        public async Task<ActionResult> Index()
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

            JsonObject dt_res = (JsonObject)JsonNode.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
            var type = dt_res.TryGetPropertyValue("DataType", out var val) ? val : null;
            var test = type.ToString();
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

            var res = (JsonObject)JsonNode.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
            if (res == null) throw new NullReferenceException($"{nameof(res)} : Result Null");

            var jdataid = res.TryGetPropertyValue("ID", out var _dataid) ? _dataid : null;
            var jdata = res.TryGetPropertyValue("Data", out var _data) ? _data : null;

            var dataid = int.Parse(jdataid?.ToString()!);
            var data = jdata?.ToString();


            dynamic exercisedata = null;
            switch(type.ToString())
            {
                case "Unknown":
                    exercisedata = new ExerciseData<string>(dataid, 
                        JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(data!)!);
                    break;
                case "Reps":
                    exercisedata = new ExerciseData<Tuple<int, int>>(dataid,
                        JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(data!)!);
                    break;
                case "Timed":
                    exercisedata = new ExerciseData<TimeSpan>(dataid, 
                        JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(data!)!);
                    break;
            }

            return View();
        }

        // GET: ExerciseDataController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ExerciseDataController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExerciseDataController/Create
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
