using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.ProjectModel;
using PROG455_FinalProject_ExerciseTracker.Models;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.Json.Nodes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    public class ExerciseDataController : Controller
    {
        private static API api;

        private async Task<string> GetDataDict()
        {
            //Get User, Exercise, and Data ID's
            var userid = HttpContext.Session.GetString("UserID")
                ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

            var exerciseid = HttpContext.Session.GetString("ExerciseID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

            var dataid = HttpContext.Session.GetString("DataID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : DataID");

            //Query API using ID's as filter and get the Data json column as a result
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

            //Parse that result into a deserializeable json
            var node = (JObject)JArray.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
            var data = node.GetValue("Data")!.ToString();
            return data;
        }

        private async void SetDataDict(string newdata)
        {
            //Get User, Exercise, and Data ID's
            var userid = HttpContext.Session.GetString("UserID")
                ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

            var exerciseid = HttpContext.Session.GetString("ExerciseID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

            var dataid = HttpContext.Session.GetString("DataID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : DataID");

            //Query API using ID's as filter and update the Data json column with the new dat
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
        }

        #region Index

        // GET: ExerciseDataController
        public async Task<ActionResult> Index()
        {
            try
            {
                api = new(HttpContext.Session.GetString("API")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : API"));

                //Retrieve User and Exercise ID's from the session data
                var userid = HttpContext.Session.GetString("UserID")
                ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                var exerciseid = HttpContext.Session.GetString("ExerciseID")
                        ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

                //Query the API Database to get the Exercise's DataType
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

                //Verify and Parse the API result from the Exercise DataType
                var dt_res = (JObject)JArray.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
                var type = dt_res.GetValue("DataType")!.ToString();
                if (type == null) throw new NullReferenceException($"{nameof(type)} : Result Null");

                //Query the API to retrieve the ExerciseData associated with the Exercise and User ID
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

                //Get the ActionResult view/Model
                Tuple<ActionResult, int> parse = ParseExerciseDataView(type);

                //If the 'int' of 'parse' is -1, then the ExerciseData already existed with the database
                if(parse.Item2 != -1)
                {
                    var newdataid = parse.Item2;
                    var emptyjson = "{}";

                    //Query the API database and insert a new ExerciseData row with an empty data column
                    await api.AsyncPOST("post.php?", new Dictionary<string, string>
                    {
                        {
                            "non-query",
                            Hasher.UTF8Encode(new APIQuery
                            {
                                Table = "PROG455_FP",
                                Query = "INSERT INTO ExerciseData (UserID, ExerciseID, ID, Data) VALUES " +
                                $"('{userid}', '{exerciseid}', '{newdataid}', '{emptyjson}')"
                            })
                        }
                    });
                }
                HttpContext.Session.SetString("DataType", type);
                TempData["DataType"] = type;
                return parse.Item1; 
            }
            catch
            {
                return View();
            }
        }

        #region Parse View 

        private Tuple<ActionResult, int> ParseExerciseDataView(string type)
        {
            ActionResult actres = null;

            //If the row does not exist, create new data row
            if (api.POSTResult == "[]")
            {
                //Generate id
                var newdataid = Hasher.CreateID();

                //Parse ActionResult based on type, set data to null
                actres = ParseExerciseDataModel(type, newdataid, null);

                //Set Data ID to Session context
                HttpContext.Session.SetString("DataID", $"{newdataid}");

                //Return ActionResult and ID, ID is a not '-1' meaing that a new row needs to be inserted
                return new Tuple<ActionResult, int>(actres, newdataid);
            }
            else
            {
                //Parse the query which returned a result
                var noderes = (JObject)JArray.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
                if (noderes == null) throw new NullReferenceException($"{nameof(noderes)} : Result Null");

                //Get ID and Data columns
                var nodeid = noderes.GetValue("ID");
                var nodedata = noderes.GetValue("Data");
                
                //Data dictionary as string/json
                var data = nodedata!.ToString();

                //Set Data ID to Session context and parse ID as int
                HttpContext.Session.SetString("DataID", nodeid?.ToString()!);
                var dataid = int.Parse(nodeid?.ToString()!);

                //Parse ActionResult based on type, pass data json to be deserialized
                actres = ParseExerciseDataModel(type, dataid, data);

                //Return ActionResult and '-1' since the column existed previously
                return new Tuple<ActionResult, int>(actres, -1);
            }
        }

        private ActionResult ParseExerciseDataModel(string type, int dataid, string? data)
        {
            //Pass DataType to view
            ViewData["DataType"] = type;
            switch (type)
            {
                case "Reps":
                    //New column
                    if(data != null) 
                    {
                        return View(new ExerciseDataWrapper<Tuple<int, int>>(new(dataid,
                        JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(data)!)));
                    }
                    //Existing column
                    else
                    {
                        return View(new ExerciseDataWrapper<Tuple<int, int>>
                            (new(dataid,new Dictionary<DateTime, Tuple<int, int>>())));
                    }

                case "Timed":
                    //New column
                    if (data != null)
                    {
                        return View(new ExerciseDataWrapper<TimeSpan>(new(dataid,
                        JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(data)!)));
                    }
                    //Existing column
                    else
                    {
                        return View(new ExerciseDataWrapper<TimeSpan>
                            (new(dataid, new Dictionary<DateTime, TimeSpan>())));
                    }

                case "Unknown":
                    //New column
                    if (data != null)
                    {
                        return View(new ExerciseDataWrapper<string>(new(dataid,
                        JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(data!)!)));
                    }
                    //Existing column
                    else
                    {
                        return View(new ExerciseDataWrapper<string>
                            (new(dataid, new Dictionary<DateTime, string>())));
                    }
                    
                default: return View();
            }
        }

        #endregion

        #endregion

        #region Create
        // GET: ExerciseDataController/Create
        public ActionResult Create(string type)
        {
            //Set the DataType in TempData/Session
            ViewData["DataType"] = TempData["DataType"] = type;

            //Choose view based on type
            switch (type)
            {
                case "Reps": return View(new ExerciseDataForm<Tuple<int, int>>());
                case "Timed": return View(new ExerciseDataForm<TimeSpan>());
                case "Unknown": return View(new ExerciseDataForm<string>());
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
                //Get type from tempdata
                var type = (string)TempData["DataType"]!
                        ?? throw new NullReferenceException($"{TempData} : DataType");

                //Pull Data from API TODO: fix this, could become demanding if the data is large enough
                var data = await GetDataDict();

                //Get new date from collection
                var date = DateTime.Parse(collection["Date"]!);

                //Parse and add the new data from the collection to the data dictionary
                //then return that updated dictionary as a string
                string newdata;
                switch (type)
                {
                    case "Reps": newdata = RepsDataToken(collection, data, date); break;
                    case "Timed": newdata = TimedDataToken(collection, data, date); break;
                    default: newdata = UnknownDataToken(collection, data, date); break;
                }

                //Update the API column with a new json value
                SetDataDict(newdata);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        #region Types
        private string RepsDataToken(IFormCollection collection, string jdict, DateTime date)
        {
            try
            {
                //Parse Data from json and Reps/Set from the collection
                var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(jdict) ?? throw new Exception();
                var sets = int.Parse(collection["Sets"]!);
                var reps = int.Parse(collection["Reps"]!);

                //Add value to dictionary
                dict.Add(date, new Tuple<int, int>(sets, reps));

                //Re-serialize dictionary and return
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
                //Parse Data from json and Min/Sec from the collection
                var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(jdict);
                var mins = int.Parse(collection["Minutes"]!);
                var secs = int.Parse(collection["Seconds"]!);
                var time = new TimeSpan(0,mins, secs);

                //Add value to dictionary
                dict!.Add(date, time);

                //Re-serialize dictionary and return
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
                //Parse Data from json and Value from the collection
                var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(jdict);
                var value = (string)collection["Value"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Value : string");

                //Add value to dictionary
                dict!.Add(date, value);

                //Re-serialize dictionary and return
                return JsonConvert.SerializeObject(dict, Formatting.None);
            }
            catch (Exception ex)
            {
                throw new Exception("Error parsing Unknown data", ex);
            }
        }

        #endregion

        #endregion

        #region Edit

        // GET: ExerciseDataController/Edit/5
        public ActionResult Edit(string token)
        {
            var type = (string)TempData["DataType"]!
                        ?? throw new NullReferenceException($"{TempData} : DataType");

            ViewData["DataType"] = TempData["DataType"] = type;
            ViewData["SessionToken"] = TempData["SessionToken"] = token;
            return View();
        }

        // POST: ExerciseDataController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(IFormCollection collection)
        {
            try
            {
                //Get tempdata variables, exercise type and data to be edited as jobject token
                var jtok = JObject.Parse((string)TempData["SessionToken"]!);

                var type = (string)TempData["DataType"]!
                        ?? throw new NullReferenceException($"{TempData} : DataType");

                //Parse API result to get dictionary value
                var jdict = await GetDataDict();

                //Get current/new data date value
                var cur_date = jtok.GetValue<DateTime>("Date");
                var new_date = DateTime.Parse(collection["Date"]!);

                //Set up hold variables for new/current data parsing based on type
                IDictionary dict;
                dynamic cur_data;
                dynamic new_data;
                switch (type)
                {
                    case "Reps":
                        //Current
                        var dtok = jtok.GetValue("Data");
                        var item1 = dtok.GetValue<int>("Item1");
                        var item2 = dtok.GetValue<int>("Item2");
                        cur_data = new Tuple<int, int>(item1, item2);

                        //New
                        var sets = int.Parse(collection["Sets"]!);
                        var reps = int.Parse(collection["Reps"]!);
                        new_data = new Tuple<int,int>(sets, reps);
                        dict = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(jdict)!;
                        break;
                    case "Timed":
                        cur_data = jtok.GetValue<TimeSpan>("Data");
                        new_data = TimeSpan.Parse(collection["Time"]!);
                        dict = JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(jdict)!; 
                        break;
                    default:
                        cur_data = jtok.GetValue<string>("Data");
                        new_data = (string)collection["Value"]!;
                        dict = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(jdict)!; 
                        break;
                }

                //If the date remains unchanged, then
                if(cur_date == new_date)
                {
                    //If the data is unchanged and the data values are different, reset the kvp
                    if (new_data != cur_data)
                    {
                        dict[cur_date] = new_data;
                    }
                }
                //If the date has changed 
                else
                {
                    //Remove the old kvp, and add the new one
                    dict.Remove(cur_date);
                    dict.Add(new_date, new_data);
                }

                //Reserialize the changed dictionary
                var newdata = JsonConvert.SerializeObject(dict);

                //Update the changed dictionary
                SetDataDict(newdata);

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        #endregion

        #region Delete

        // GET: ExerciseDataController/Delete/5
        public ActionResult Delete(string token)
        {
            var type = (string)TempData["DataType"]!
                        ?? throw new NullReferenceException($"{TempData} : DataType");
           
            TempData["SessionToken"] = ViewData["SessionToken"] = token;
            TempData["DataType"] = ViewData["DataType"] = type;
            return View();
        }

        // POST: ExerciseDataController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(IFormCollection collection)
        {
            try
            {
                //Get tempdata variables, exercise type and data to be edited as jobject token
                var jtok = JObject.Parse((string)TempData["SessionToken"]!);

                var type = (string)TempData["DataType"]!
                        ?? throw new NullReferenceException($"{TempData} : DataType");

                //Parse API result to get dictionary value
                var jdict = await GetDataDict();

                //Set up hold variables for parsing based on type
                IDictionary dict;
                switch (type)
                {
                    case "Reps": dict = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(jdict)!; break;
                    case "Timed": dict = JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(jdict)!; break;
                    default: dict = JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(jdict)!; break;
                }

                var date = jtok.GetValue<DateTime>("Date");

                dict.Remove(date);

                //Reserialize the changed dictionary
                var newdata = JsonConvert.SerializeObject(dict);

                //Update the changed dictionary
                SetDataDict(newdata);

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
