using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            try
            {
                api = new(HttpContext.Session.GetString("API")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : API"));

                var userid = HttpContext.Session.GetString("UserID")
                        ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
            {
                {
                    "query",
                    Hasher.UTF8Encode(new APIQuery
                    {
                        Table = "PROG455_FP",
                        Query = $"SELECT * FROM Exercises WHERE UserID = '{userid}'"
                    })
                }
            });

                var res = api.POSTResult;
                if (res == null) throw new NullReferenceException($"{nameof(res)} : Result Null");

                var jarr = JArray.Parse(res);

                var lis = jarr.ToObject<List<Exercise>>();
                if (lis == null) throw new NullReferenceException($"{nameof(lis)} : Deserialization Failed");

                return View(lis);
            }
            catch
            {
                return RedirectToAction("Account","User");
            }
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

        public ActionResult Details(int id)
        {
            HttpContext.Session.SetString("ExerciseID", $"{id}");
            return RedirectToAction("Index", "ExerciseData");
        }

        // GET: ExerciseController/Edit/5
        public ActionResult Edit(string token)
        {
            var exercise = JToken.Parse(token).ToObject<Exercise>()!;
            HttpContext.Session.SetString("ExerciseID", $"{exercise.ID}");
            TempData["ExerciseName"] = exercise.Name;
            TempData["ExerciseDesc"] = exercise.Description;
            return View(exercise);
        }

        // POST: ExerciseController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(IFormCollection collection)
        {
            try
            {
                var userid = HttpContext.Session.GetString("UserID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                var exerciseid = HttpContext.Session.GetString("ExerciseID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");


                var cur_name = TempData["ExerciseName"] as string;
                var cur_desc = TempData["ExerciseDesc"] as string;

                var new_name = (string)collection["Name"]
                    ?? throw new InvalidCastException($"{nameof(collection)} : Name : string");

                var new_desc = (string)collection["Description"]
                    ?? throw new InvalidCastException($"{nameof(collection)} : Description : string");


                var name = (cur_name != new_name) ? new_name : cur_name;
                var desc = (cur_desc != new_desc) ? new_desc : cur_desc;

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "non-query",
                        Hasher.UTF8Encode(new
                        {
                            Table = "PROG455_FP",
                            Query = $"UPDATE Exercises SET Name = '{name}', Description = '{desc}' " +
                            $"WHERE UserID = '{userid}' AND ID = '{exerciseid}'"
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

        // GET: ExerciseController/Delete/5
        public ActionResult Delete(string token)
        {
            var exercise = JToken.Parse(token).ToObject<Exercise>()!;
            HttpContext.Session.SetString("ExerciseID", $"{exercise.ID}");
            return View(exercise);
        }

        // POST: ExerciseController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(IFormCollection collection)
        {
            try
            {
                var userid = HttpContext.Session.GetString("UserID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                var exerciseid = HttpContext.Session.GetString("ExerciseID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "non-query",
                        Hasher.UTF8Encode(new
                        {
                            Table = "PROG455_FP",
                            Query = $"DELETE FROM Exercises WHERE UserID = '{userid}' AND ID = '{exerciseid}'"
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
    }
}
