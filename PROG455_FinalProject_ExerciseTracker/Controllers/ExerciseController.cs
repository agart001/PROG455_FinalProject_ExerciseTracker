using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PROG455_FinalProject_ExerciseTracker.Models;
using System.Collections.Generic;
using System.Xml.Linq;

namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    public class ExerciseController : Controller
    {
        private static API api;
        // GET: ExerciseController
        public async Task<ActionResult> Index()
        {
            api = new(HttpContext.Session.GetString("API")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : API"));

            var userid = HttpContext.Session.GetString("UserID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

            await api.AsyncPOST("post.php?", new Dictionary<string, string>
            {
                {
                    "query",
                    new APIQuery
                    {
                        Table = "PROG455_FP",
                        Query = $"SELECT * FROM Exercises WHERE UserID = '{userid}'"
                    }.ToString()
                }
            });

            var res = api.POSTResult;
            if (res == null) throw new NullReferenceException($"{nameof(res)} : Result Null");

            var lis = JsonConvert.DeserializeObject<List<Exercise>>(res);
            if (lis == null) throw new NullReferenceException($"{nameof(lis)} : Deserialization Failed");

            return View(lis);
        }


        // GET: ExerciseController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ExerciseController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateAsync(IFormCollection collection)
        {
            try
            {
                var userid = HttpContext.Session.GetString("UserID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                var name = (string)collection["Name"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Name : string");

                var desc = (string)collection["Description"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Description : string");

                var type = (Exercise.EDataType)Enum.Parse
                        (typeof(Exercise.EDataType), (string)collection["DataType"]!
                            ?? throw new InvalidCastException($"{nameof(collection)} : DataType : string"));

                Exercise exercise = new(name, desc, type);

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "non-query",
                        Hasher.UTF8Encode(new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = "INSERT INTO Exercises (UserID, ID, Name, Description, DataType) VALUES " +
                            $"('{userid}', '{exercise.ID}', '{exercise.Name}', '{exercise.Description}', '{exercise.DataType}')"
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

        // GET: ExerciseController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ExerciseController/Edit/5
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

        // GET: ExerciseController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ExerciseController/Delete/5
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
