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

        #region Index/Details
        // GET: ExerciseController
        public async Task<ActionResult> Index()
        {
            try
            {

                api = new(HttpContext.Session.GetString("API")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : API"));

                //Get User's ID from the session data
                var userid = HttpContext.Session.GetString("UserID")
                        ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                //Get all exercises in the database which are associated with the User via their ID
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

                //Verify that the API returned a result, whether that is an error code or not (TODO: further checks for errors needed)
                var res = api.POSTResult;
                if (res == null) throw new NullReferenceException($"{nameof(res)} : Result Null");

                //Parse the API result json into an JArray
                var jarr = JArray.Parse(res);

                //Parse the JArray result into the VM model
                var lis = jarr.ToObject<List<Exercise>>();
                if (lis == null) throw new NullReferenceException($"{nameof(lis)} : Deserialization Failed");

                return View(lis);
            }
            catch
            {
                return RedirectToAction("Account","User");
            }
        }

        public ActionResult Details(int id)
        {
            HttpContext.Session.SetString("ExerciseID", $"{id}");
            return RedirectToAction("Index", "ExerciseData");
        }

        #endregion

        #region Create
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
                //Get User's ID from the session data
                var userid = HttpContext.Session.GetString("UserID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                //Get the User's form input, the Exercise Name, Description, and DataType
                var name = (string)collection["Name"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Name : string");

                var desc = (string)collection["Description"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Description : string");

                var type = (Exercise.EDataType)Enum.Parse
                        (typeof(Exercise.EDataType), (string)collection["DataType"]!
                            ?? throw new InvalidCastException($"{nameof(collection)} : DataType : string"));

                //Create a new Exercise(this is mildy unecessary, but provides a new Exercise ID)
                Exercise exercise = new(name, desc, type);

                //Query the API database to insert the newly created Exercise and attach the User's ID as an identifier
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

        #endregion

        #region Edit
        // GET: ExerciseController/Edit/5
        public ActionResult Edit(string token)
        {
            //Parse the action-route parameter, a Jtoken string, of the Exercise
            var exercise = JToken.Parse(token).ToObject<Exercise>()!;

            //Set session and temp data variables to be passed
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
                //Get the User's ID from the session data
                var userid = HttpContext.Session.GetString("UserID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                //Get the Exercise's ID from the session data
                var exerciseid = HttpContext.Session.GetString("ExerciseID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

                //Retrieve the variables store in 'TempData'
                var cur_name = TempData["ExerciseName"] as string;
                var cur_desc = TempData["ExerciseDesc"] as string;

                //Parse the form inputs, the new Name and the new Description
                var new_name = (string)collection["Name"]
                    ?? throw new InvalidCastException($"{nameof(collection)} : Name : string");

                var new_desc = (string)collection["Description"]
                    ?? throw new InvalidCastException($"{nameof(collection)} : Description : string");

                //Conditionals to determine whether the new variable value is different from the original
                // if(condition) { first } else { other } == (condition) ? first : other
                var name = (cur_name != new_name) ? new_name : cur_name;
                var desc = (cur_desc != new_desc) ? new_desc : cur_desc;

                //Query the API database to apply any changes that were made(TODO: could be made more efficient if nothin is changed)
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

        #endregion

        #region Delete

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
                //Retrieve the User and Exercise ID's
                var userid = HttpContext.Session.GetString("UserID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                var exerciseid = HttpContext.Session.GetString("ExerciseID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

                //Query the API database to remove the Exercise 
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

        #endregion
    }
}
