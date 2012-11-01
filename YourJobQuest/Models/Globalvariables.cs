using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Data.SqlClient;

namespace YourJobQuest.Models
{
    public class Globalvariables
    {
        public static Boolean isValidUserName { get; set; }
        public static int UserId { get; set; }
        public static String UserName { get; set; }
        public static YourJobQuestEntities1 db { get; set; }
        public static SqlCommand conn { get; set; }
        public static String ProfileName { get; set; }
        public static String email { get; set; }
    }
}


