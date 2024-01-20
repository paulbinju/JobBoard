using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Jobs4Bahrainis
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");



            routes.MapRoute(
                name: "PostAJob",
                url: "PostAJob",
                defaults: new { controller = "Home", action = "PostAJob" }
            );


            routes.MapRoute(
                name: "PaymentSuccess",
                url: "PaymentSuccess",
                defaults: new { controller = "Home", action = "PaymentSuccess" }
            );


            routes.MapRoute(
                name: "RecruiterContact",
                url: "RecruiterContact",
                defaults: new { controller = "Home", action = "RecruiterContact" }
            );

    



            routes.MapRoute(
                name: "RecruiterRegistration",
                url: "RecruiterRegistration",
                defaults: new { controller = "Home", action = "RecruiterRegistration" }
            );


            routes.MapRoute(
                name: "JobDescription",
                url: "JobDescription/{jobid}/{jobtitle}",
                defaults: new { controller = "Home", action = "JobDescription", jobid = UrlParameter.Optional, jobtitle = UrlParameter.Optional }
            );


 


            routes.MapRoute(
                  name: "AddCandidateSkill2",
                  url: "AddCandidateSkill2",
                  defaults: new { controller = "Home", action = "AddCandidateSkill2" }
              );

            routes.MapRoute(
                name: "RecruiterSectors2",
                url: "RecruiterSectors2",
                defaults: new { controller = "Home", action = "RecruiterSectors2" }
            );

            

            routes.MapRoute(
                  name: "DeleteCandidateSkill",
                  url: "DeleteCandidateSkill/{caskillid}",
                  defaults: new { controller = "Home", action = "DeleteCandidateSkill", caskillid = UrlParameter.Optional }
              );
            

            routes.MapRoute(
                name: "Search",
                url: "Search",
                defaults: new { controller = "Home", action = "Search"}
            );


            routes.MapRoute(
                name: "JobSeekerProfile",
                url: "JobSeekerProfile",
                defaults: new { controller = "Home", action = "JobSeekerProfile", loggedin = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "Login",
                url: "Login",
                defaults: new { controller = "Home", action = "Login", loggedin = UrlParameter.Optional }
            );



            routes.MapRoute(
                name: "MediaCenter",
                url: "MediaCenter",
                defaults: new { controller = "Home", action = "MediaCenter", loggedin = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "ContactUs",
                url: "ContactUs",
                defaults: new { controller = "Home", action = "ContactUs", loggedin = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "JobSeeker",
                url: "JobSeeker",
                defaults: new { controller = "Home", action = "JobSeeker", loggedin = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Recruiter",
                url: "Recruiter",
                defaults: new { controller = "Home", action = "Recruiter", loggedin = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{loggedin}",
                defaults: new { controller = "Home", action = "Index", loggedin = UrlParameter.Optional }
            );
        }
    }
}
