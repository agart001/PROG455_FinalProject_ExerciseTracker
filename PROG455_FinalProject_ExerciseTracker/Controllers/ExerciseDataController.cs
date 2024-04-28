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

                TempData["DataType"] = type;
                return parse.Item1; 
            }
            catch
            {
                return View();
            }
        }

        private Tuple<ActionResult, int> ParseExerciseDataView(string type)
        {
            ActionResult actres = null;
            if (api.POSTResult == "[]")
            {
                var newdataid = Hasher.CreateID();
                actres = ParseExerciseDataModel(type, newdataid, null);
                HttpContext.Session.SetString("DataID", $"{newdataid}");
                return new Tuple<ActionResult, int>(actres, newdataid);
            }
            else
            {
                var noderes = (JObject)JArray.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
                if (noderes == null) throw new NullReferenceException($"{nameof(noderes)} : Result Null");

                var nodeid = noderes.GetValue("ID");
                var nodedata = noderes.GetValue("Data");

                var data = nodedata!.ToString();

                HttpContext.Session.SetString("DataID", nodeid?.ToString()!);
                var dataid = int.Parse(nodeid?.ToString()!);

                actres = ParseExerciseDataModel(type, dataid, data);

                return new Tuple<ActionResult, int>(actres, -1);
            }
        }

        private ActionResult ParseExerciseDataModel(string type, int dataid, string? data)
        {
            ViewData["DataType"] = type;
            switch (type)
            {
                case "Reps":

                    if(data != null) 
                    {
                        return View(new ExerciseDataWrapper<Tuple<int, int>>(new(dataid,
                        JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(data)!)));
                    }
                    else
                    {
                        return View(new ExerciseDataWrapper<Tuple<int, int>>
                            (new(dataid,new Dictionary<DateTime, Tuple<int, int>>())));
                    }

                case "Timed":

                    if (data != null)
                    {
                        return View(new ExerciseDataWrapper<TimeSpan>(new(dataid,
                        JsonConvert.DeserializeObject<Dictionary<DateTime, TimeSpan>>(data)!)));
                    }
                    else
                    {
                        return View(new ExerciseDataWrapper<TimeSpan>
                            (new(dataid, new Dictionary<DateTime, TimeSpan>())));
                    }

                case "Unknown":

                    if (data != null)
                    {
                        return View(new ExerciseDataWrapper<string>(new(dataid,
                        JsonConvert.DeserializeObject<Dictionary<DateTime, string>>(data!)!)));
                    }
                    else
                    {
                        return View(new ExerciseDataWrapper<string>
                            (new(dataid, new Dictionary<DateTime, string>())));
                    }
                    
                default: return View();
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
                return RedirectToAction(nameof(Index));
            }
        }

        private string RepsDataToken(IFormCollection collection, string jdict, DateTime date)
        {
            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<DateTime, Tuple<int, int>>>(jdict) ?? throw new Exception();
                var sets = int.Parse(collection["Sets"]!);
                var reps = int.Parse(collection["Reps"]!);

                dict.Add(date, new Tuple<int, int>(sets, reps));

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
        public ActionResult Edit(string token)
        {
            var type = (string)TempData["DataType"]!
                        ?? throw new NullReferenceException($"{TempData} : DataType");

            var jtok = JObject.Parse(token);
            var date = jtok.GetValue<DateTime>("Date");
            dynamic data;
            ViewData["DataType"] = type;
            TempData["DataType"] = type;
            TempData["SessionToken"] = token;
            TempData["SessionDate"] = date;
            switch (type)
            {
                case "Reps":
                    var dtok = jtok.GetValue("Data");
                    var item1 = dtok.GetValue<int>("Item1");
                    var item2 = dtok.GetValue<int>("Item2");
                    data = new Tuple<int,int>(item1, item2);
                    TempData["SessionData"] = data;
                    return View();
                case "Timed":
                    data = jtok.GetValue<TimeSpan>("Data");
                    TempData["SessionData"] = data;
                    return View();
                default:
                    data = jtok.GetValue<string>("Data");
                    TempData["SessionData"] = data;
                    return View();
            }
        }

        // POST: ExerciseDataController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(IFormCollection collection)
        {
            try
            {
                //Get session ID variables
                var userid = HttpContext.Session.GetString("UserID")
                ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                var exerciseid = HttpContext.Session.GetString("ExerciseID")
                        ?? throw new NullReferenceException($"{HttpContext.Session} : ExerciseID");

                var dataid = HttpContext.Session.GetString("DataID")
                        ?? throw new NullReferenceException($"{HttpContext.Session} : DataID");

                //Get tempdata variables, exercise type and data to be edited as jobject token
                var jtok = JObject.Parse((string)TempData["SessionToken"]!);

                var type = (string)TempData["DataType"]!
                        ?? throw new NullReferenceException($"{TempData} : DataType");

                //Query API to get data dictionary
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

                //Parse API result to get dictionary value
                var node = (JObject)JArray.Parse(api.POSTResult!)![0]! ?? throw new InvalidCastException();
                var jdict = node.GetValue("Data")!.ToString();

                //Get current/new data date value
                var cur_date = jtok.GetValue<DateTime>("Date");
                var new_date = DateTime.Parse(collection["Date"]!);

                //Set up hold variables for parsing based on type
                IDictionary dict;
                dynamic cur_data;
                dynamic new_data;
                switch (type)
                {
                    case "Reps":
                        var dtok = jtok.GetValue("Data");
                        var item1 = dtok.GetValue<int>("Item1");
                        var item2 = dtok.GetValue<int>("Item2");
                        cur_data = new Tuple<int, int>(item1, item2);


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
                var new_Data = JsonConvert.SerializeObject(dict);

                //Query the API to with the updated dictionary
                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "non-query",
                        Hasher.UTF8Encode(new
                        {
                            Table = "PROG455_FP",
                            Query = $"UPDATE ExerciseData SET Data = '{new_Data}' WHERE UserID = '{userid}' " +
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
