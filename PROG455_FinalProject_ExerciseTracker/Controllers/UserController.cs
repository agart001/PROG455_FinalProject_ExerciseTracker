using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PROG455_FinalProject_ExerciseTracker.Models;

namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    /// <summary>
    /// Controller responsible for managing user-related actions.
    /// </summary>
    public class UserController : Controller
    {
        private static API api = new();

        /// <summary>
        /// Displays the index view.
        /// </summary>
        /// <returns>The index view.</returns>
        public ActionResult Index()
        {
            api = new(HttpContext.Session.GetString("API")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : API"));

            return View();
        }

        #region SignIn

        /// <summary>
        /// Displays the sign-in view.
        /// </summary>
        /// <returns>The sign-in view.</returns>
        // GET: SignInController/Create
        public ActionResult SignIn()
        {
            return View();
        }

        /// <summary>
        /// Handles the sign-in form submission.
        /// </summary>
        /// <param name="collection">The form collection containing user inputs.</param>
        /// <returns>An action result.</returns>
        // POST: SignInController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignIn(IFormCollection collection)
        {
            try
            {
                // Parse form collection variable, the User's Name and Password
                var name = (string)collection["Name"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Name : string");

                var password = Hasher.SHA256Hash((string)collection["Password"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Password : string"));

                //Query the database through the API, Select the User's ID based on their Name and Password
                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "query",
                        Hasher.UTF8Encode(new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = $"SELECT ID FROM Users WHERE Name = '{name}' AND Password = '{password}'"
                        })
                    }
                });

                //Verify that the API returned a result, whether that is an error code or not (TODO: further checks for errors needed)
                var res = api.POSTResult;
                if (res == null) throw new NullReferenceException($"{nameof(res)} : Result Null");

                //Parse API json result(which is returned in an array)
                var jobj = (JObject)JArray.Parse(res)![0]!;

                //Get Jtoken of the ID variable
                var userid = jobj.GetValue("ID")!.ToString();

                //Set the User ID in the session for further use
                HttpContext.Session.SetString("UserID", $"{userid}");

                return RedirectToAction(nameof(Account));
            }
            catch
            {
                return RedirectToAction(nameof(SignUp));
            }
        }

        #endregion

        #region SignUp

        /// <summary>
        /// Displays the sign-up view.
        /// </summary>
        /// <returns>The sign-up view.</returns>
        public ActionResult SignUp()
        {
            return View();
        }

        /// <summary>
        /// Handles the sign-up form submission.
        /// </summary>
        /// <param name="collection">The form collection containing user inputs.</param>
        /// <returns>An action result.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignUp(IFormCollection collection)
        {
            try
            {
                // Parse form collection variable, the User's Name and Password
                var name = (string)collection["Name"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Name : string");

                var password = Hasher.SHA256Hash((string)collection["Password"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Password : string"));

                //Create a new User(this is mildy unecessary, but provides a new User ID)
                User user = new(name, password);

                //Query the API to insert the new User into the database
                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "non-query",
                        Hasher.UTF8Encode(new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = "INSERT INTO Users (ID, Name, Password) " +
                            $"VALUES ('{user.ID}', '{user.Name}', '{user.Password}')"
                        })
                    }
                });

                //Set the User ID in the session for further use
                HttpContext.Session.SetString("UserID", $"{user.ID}");

                return RedirectToAction(nameof(Account));
            }
            catch
            {
                return View();
            }
        }

        #endregion

        /// <summary>
        /// Displays the user account view.
        /// </summary>
        /// <returns>The user account view.</returns>
        public async Task<ActionResult> Account()
        {
            try
            {
                //Get the User's ID from the session data
                var id = HttpContext.Session.GetString("UserID");
                if (id == null)
                {
                    //If the user has not signed in/up, the session 'UserID' is null
                    //Make the user sign in/up
                    return RedirectToAction(nameof(SignIn));
                }

                //Query the API database to gather the User's info
                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "query",
                        Hasher.UTF8Encode(new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = $"SELECT * FROM Users WHERE ID = '{id}'"
                        })
                    }
                });

                //Verify that the API returned a result, whether that is an error code or not (TODO: further checks for errors needed)
                var res = api.POSTResult;
                if (res == null) throw new NullReferenceException($"{nameof(res)} : Result Null");

                //Parse API json result(which is returned in an array)
                var jobj = (JObject)JArray.Parse(res)![0]!;

                //Parse the JObject into the VM model
                User user = jobj.ToObject<User>() ?? throw new InvalidCastException($"{nameof(jobj)} : Cast to User");

                return View(user);
            }
            catch
            {
                return RedirectToAction(nameof(SignIn));
            }
        }
    }
}
