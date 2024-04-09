using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PROG455_FinalProject_ExerciseTracker.Models;
using static PROG455_FinalProject_ExerciseTracker.Models.Hasher;

namespace PROG455_FinalProject_ExerciseTracker.Controllers
{
    /// <summary>
    /// Controller responsible for managing user-related actions.
    /// </summary>
    public class UserController : Controller
    {
        static string BaseUrl = "http://ec2-18-223-162-6.us-east-2.compute.amazonaws.com/";
        static API api = new(BaseUrl);

        /// <summary>
        /// Displays the index view.
        /// </summary>
        /// <returns>The index view.</returns>
        public ActionResult Index()
        {
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
                var name = (string)collection["Name"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Name : string");

                var password = GetStringHash((string)collection["Password"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Password : string"));

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "query",
                        new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = $"SELECT * FROM Users WHERE Name = '{name}' AND Password = '{password}'"
                        }.ToString()
                    }
                });

                var res = api.POSTResult;
                if (res == null) throw new NullReferenceException($"{nameof(res)} : Result Null");
                var lis = api.NSJsonDeserialize<List<User>>(res);

                if (lis == null) throw new NullReferenceException($"{nameof(lis)} : Deserialization Failed");

                User user = lis.FirstOrDefault()!;

                HttpContext.Session.SetString("UserID", $"{user.ID}");

                return RedirectToAction(nameof(Account));
            }
            catch
            {
                return View();
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
                var name = (string)collection["Name"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Name : string");

                var password = GetStringHash((string)collection["Password"]!
                    ?? throw new InvalidCastException($"{nameof(collection)} : Password : string"));

                User user = new(name, password);

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "non-query",
                        new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = "INSERT INTO Users (ID, Name, Password) " +
                            $"VALUES ('{user.ID}', '{user.Name}', '{user.Password}')"
                        }.ToString()
                    }
                });

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
                var id = HttpContext.Session.GetString("UserID")
                    ?? throw new NullReferenceException($"{HttpContext.Session} : UserID");

                await api.AsyncPOST("post.php?", new Dictionary<string, string>
                {
                    {
                        "query",
                        new APIQuery
                        {
                            Table = "PROG455_FP",
                            Query = $"SELECT * FROM Users WHERE ID = '{id}'"
                        }.ToString()
                    }
                });

                var res = api.POSTResult;
                if (res == null) throw new NullReferenceException($"{nameof(res)} : Result Null");
                var lis = api.NSJsonDeserialize<List<User>>(res);

                if (lis == null) throw new NullReferenceException($"{nameof(lis)} : Deserialization Failed");

                User user = lis.FirstOrDefault()!;

                return View(user);
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
