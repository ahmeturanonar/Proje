using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proje.Models
{
    public class LoginControl: AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            ProjeContext db = new ProjeContext();

            string rolid = httpContext.User.Identity.Name;

            var user = db.User.Where(x => x.Username == rolid).FirstOrDefault();

            
            var roles = Roles.Split(',');

            if (user.Role == "Admin" )
            {
                if (roles.Contains("Admin"))
                    return true;
            }

            else if(user.Role == "Company")
            {
                if (roles.Contains("Company"))
                    return true;
            }

            else if(user.Role == "Member")
            {
                if (roles.Contains("Member"))
                    return true;
            }

            return base.AuthorizeCore(httpContext);
        }
    }
}