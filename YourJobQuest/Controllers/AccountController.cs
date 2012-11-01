using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using YourJobQuest.Models;
using System.Net.Mail;
namespace YourJobQuest.Controllers
{
    public class AccountController : Controller
    {
        YourJobQuestEntities1 db = new YourJobQuestEntities1();
        public static string EncodePasswordToBase64(string password)
        {
            try
            {
                byte[] encData_byte = new byte[password.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(password);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Encode" + ex.Message);
            }

        }

        public string DecodeFrom64(string encodedData)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var UserLogin = new YourJobQuestEntities1();
                var pass = EncodePasswordToBase64(model.password);
                var user = from u in UserLogin.Users
                           where u.email.Equals(model.email)
                           where u.password.Equals(pass)
                           select u;
                if (user.FirstOrDefault() != null)
                { 
                    HttpContext.Session.Add("YourJobQuestUser", user.FirstOrDefault().userid);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        Globalvariables.UserId = (from u in db.Users
                                                  where u.email == model.email
                                                  select u.userid).Single();
                        Globalvariables.UserName = (from u in db.Users
                                                    where u.userid == Globalvariables.UserId
                                                    select u.firstName).Single().ToString();

                        Globalvariables.email = (from u in db.Users
                                                 where u.userid == Globalvariables.UserId
                                                 select u.email).Single().ToString();
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }
            return View();
        }


        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            Globalvariables.UserId = 0;

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(User model)
        {
            User usr = new User();
            var uNameValidity = (from u in db.Users
                                 select u.email).ToArray();
            for (int i = 0; i < uNameValidity.Length; i++)
            {
                if (uNameValidity[i].Equals(model.email))
                {
                    Globalvariables.isValidUserName = false;
                    TempData["message"] = "An account already exist for this e-mail. please try with another e-mail";

                    break;
                }
                else
                {
                    Globalvariables.isValidUserName = true;
                }

            }
            if (Globalvariables.isValidUserName == true)
            {

                if (ModelState.IsValid)
                {
                    User p = new User();

                    p.firstName = model.firstName;
                    p.lastName = model.lastName;

                    p.email = model.email;
                    p.password = EncodePasswordToBase64(model.password);
                    p.RootUser = false;

                    p.CreatedDate = DateTime.Now;
                    p.LastModifiedDate = DateTime.Now;

                    db.Users.Add(p);
                    db.SaveChanges();
                    return RedirectToAction("LogOn", "Account");


                    //MailMessage message = new MailMessage();
                    //message.From = new MailAddress("anamika@mtxbd.com");
                    //message.To.Add(new MailAddress("anamika@mtxbd.com"));
                    //message.Subject = " Welcome to Matrix";
                    //message.Body = "Dear " + p.firstName + " " + p.lastName + "," + Environment.NewLine + "Thank you for registering with YourJObQuest" + Environment.NewLine + "Username: " + p.email + Environment.NewLine + "Password: " + p.password + Environment.NewLine + "Regards," + Environment.NewLine + "YourJobQuest Admin";

                    //SmtpClient client = new SmtpClient();
                    //client.Send(message);

                }

                return RedirectToAction("LogOn", "Account");

            }
            else
            {
                return RedirectToAction("Register", "Account");
            }
            




        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        #region Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
