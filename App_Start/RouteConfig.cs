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
            name: "CanRegSuccess",
            url: "CanRegSuccess",
            defaults: new { controller = "Home", action = "CanRegSuccess" }
            );


            routes.MapRoute(
            name: "JobFair",
            url: "JobFair",
            defaults: new { controller = "Home", action = "JobFair" }
            );

            routes.MapRoute(
            name: "ResendCandidateActivationMail",
            url: "ResendCandidateActivationMail/{Ca_ID}",
            defaults: new { controller = "Home", action = "ResendCandidateActivationMail", Ca_ID = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "PrivacyPolicy",
            url: "PrivacyPolicy",
            defaults: new { controller = "Home", action = "PrivacyPolicy" }
            );
            routes.MapRoute(
            name: "TermsandConditionsJobSeeker",
            url: "TermsandConditionsJobSeeker",
            defaults: new { controller = "Home", action = "TermsandConditionsJobSeeker" }
            );


            routes.MapRoute(
            name: "TermsandConditionsUsers",
            url: "TermsandConditionsUsers",
            defaults: new { controller = "Home", action = "TermsandConditionsUsers" }
            );


            routes.MapRoute(
            name: "TermsandConditions",
            url: "TermsandConditions",
            defaults: new { controller = "Home", action = "TermsandConditions"}
            );


            routes.MapRoute(
            name: "ActivateEmployer",
            url: "ActivateEmployer/{co_Guid}/{welcome}",
            defaults: new { controller = "Home", action = "ActivateEmployer", co_Guid = UrlParameter.Optional, welcome = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "BOEmpActivationResend",
            url: "BOEmpActivationResend/{co_Guid}",
            defaults: new { controller = "Home", action = "BOEmpActivationResend", co_Guid = UrlParameter.Optional }
            );


            routes.MapRoute(
            name: "BOEmailActivateCompany",
            url: "BOEmailActivateCompany/{companyid}",
            defaults: new { controller = "Home", action = "BOEmailActivateCompany", companyid = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "BOCompanyDelete",
            url: "BOCompanyDelete/{companyid}",
            defaults: new { controller = "Home", action = "BOCompanyDelete", companyid = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "BOCandiateDelete",
            url: "BOCandiateDelete/{candidateid}",
            defaults: new { controller = "Home", action = "BOCandiateDelete", candidateid = UrlParameter.Optional }
            );
            
            routes.MapRoute(
            name: "BackOfficeVacancies",
            url: "BackOfficeVacancies",
            defaults: new { controller = "Home", action = "BackOfficeVacancies" }
            );

            routes.MapRoute(
            name: "BackOfficeJobFair",
            url: "BackOfficeJobFair",
            defaults: new { controller = "Home", action = "BackOfficeJobFair" }
            );


            routes.MapRoute(
            name: "BackOfficeEmployersActivated",
            url: "BackOfficeEmployersActivated",
            defaults: new { controller = "Home", action = "BackOfficeEmployersActivated" }
            );

            routes.MapRoute(
            name: "BackOfficeEmployersActivate",
            url: "BackOfficeEmployersActivate",
            defaults: new { controller = "Home", action = "BackOfficeEmployersActivate" }
            );




            routes.MapRoute(
            name: "BackOfficeEmployersEmailNonActivated",
            url: "BackOfficeEmployersEmailNonActivated",
            defaults: new { controller = "Home", action = "BackOfficeEmployersEmailNonActivated" }
            );
            



            routes.MapRoute(
            name: "SuccessfullPayment",
            url: "SuccessfullPayment",
            defaults: new { controller = "Home", action = "SuccessfullPayment" }
            );

            routes.MapRoute(
            name: "Reports",
            url: "Reports",
            defaults: new { controller = "Home", action = "Reports" }
            );



            routes.MapRoute(
            name: "getCompanyContacts",
            url: "getCompanyContacts/{CompanyID}",
            defaults: new { controller = "Home", action = "getCompanyContacts", CompanyID = UrlParameter.Optional }
            );

            
 
            routes.MapRoute(
            name: "BackOfficeEmployersAssignedPackage",
            url: "BackOfficeEmployersAssignedPackage",
            defaults: new { controller = "Home", action = "BackOfficeEmployersAssignedPackage" }
            );


            routes.MapRoute(
            name: "BackOfficeEmployersAssignPackage",
            url: "BackOfficeEmployersAssignPackage",
            defaults: new { controller = "Home", action = "BackOfficeEmployersAssignPackage" }
            );

            routes.MapRoute(
            name: "CreditCardPayment",
            url: "CreditCardPayment",
            defaults: new { controller = "Home", action = "CreditCardPayment" }
            );
            
            routes.MapRoute(
            name: "SubscriptionThanks",
            url: "SubscriptionThanks",
            defaults: new { controller = "Home", action = "SubscriptionThanks" }
            );

            routes.MapRoute(
            name: "NoPermission",
            url: "NoPermission",
            defaults: new { controller = "Home", action = "NoPermission" }
            );


            



            routes.MapRoute(
            name: "FollowCompany",
            url: "FollowCompany/{CompanyID}",
            defaults: new { controller = "Home", action = "FollowCompany", CompanyID = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "SaveJob",
            url: "SaveJob/{VacancyID}",
            defaults: new { controller = "Home", action = "SaveJob", VacancyID = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "Updateappstatus",
            url: "Updateappstatus/{app_id}/{appstatusid}",
            defaults: new { controller = "Home", action = "Updateappstatus", app_id = UrlParameter.Optional, appstatusid = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "PublishMedia",
            url: "PublishMedia/{yesno}",
            defaults: new { controller = "Home", action = "PublishMedia", yesno = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "PublishGallery",
            url: "PublishGallery/{yesno}",
            defaults: new { controller = "Home", action = "PublishGallery", yesno = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "PublishVideos",
            url: "PublishVideos/{yesno}",
            defaults: new { controller = "Home", action = "PublishVideos", yesno = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "PublishProfile",
            url: "PublishProfile/{yesno}",
            defaults: new { controller = "Home", action = "PublishProfile", yesno = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "PublishTestimonials",
            url: "PublishTestimonials/{yesno}",
            defaults: new { controller = "Home", action = "PublishTestimonials", yesno = UrlParameter.Optional }
            );


            routes.MapRoute(
            name: "BackOfficeEmployersTemp",
            url: "BackOfficeEmployersTemp",
            defaults: new { controller = "Home", action = "BackOfficeEmployersTemp" }
            );


            routes.MapRoute(
            name: "BackOfficeRegistrationProgressCandiates",
            url: "BackOfficeRegistrationProgressCandiates",
            defaults: new { controller = "Home", action = "BackOfficeRegistrationProgressCandiates" }
            );

            routes.MapRoute(
            name: "UploadProfilePicRegistration",
            url: "UploadProfilePicRegistration",
            defaults: new { controller = "Home", action = "UploadProfilePicRegistration" }
            );

            


            routes.MapRoute(
            name: "UploadCVregistration",
            url: "UploadCVregistration",
            defaults: new { controller = "Home", action = "UploadCVregistration" }
            );

            routes.MapRoute(
            name: "BackOfficeActivatedCandiates",
            url: "BackOfficeActivatedCandiates",
            defaults: new { controller = "Home", action = "BackOfficeActivatedCandiates" }
            );

            routes.MapRoute(
            name: "BackOfficeNonActivatedCandiates",
            url: "BackOfficeNonActivatedCandiates",
            defaults: new { controller = "Home", action = "BackOfficeNonActivatedCandiates" }
            );


            routes.MapRoute(
            name: "BackOfficeLogin",
            url: "BackOfficeLogin",
            defaults: new { controller = "Home", action = "BackOfficeLogin" }
            );


            routes.MapRoute(
            name: "getCandiCount",
            url: "getCandiCount",
            defaults: new { controller = "Home", action = "getCandiCount" }
            );

            routes.MapRoute(
            name: "getAuthority",
            url: "getAuthority",
            defaults: new { controller = "Home", action = "getAuthority" }
            );

            routes.MapRoute(
            name: "NewsletterSubscription",
            url: "NewsletterSubscription",
            defaults: new { controller = "Home", action = "NewsletterSubscription" }
            );

            routes.MapRoute(
            name: "EmployerCSThanks",
            url: "EmployerCSThanks",
            defaults: new { controller = "Home", action = "EmployerCSThanks" }
            );

            routes.MapRoute(
            name: "EmployerMeetingRequest",
            url: "EmployerMeetingRequest",
            defaults: new { controller = "Home", action = "EmployerComingSoon" }
            );

            routes.MapRoute(
            name: "JobSeekerOSkillUpdate",
            url: "JobSeekerOSkillUpdate",
            defaults: new { controller = "Home", action = "JobSeekerOSkillUpdate" }
            );

            routes.MapRoute(
            name: "JobSeekerITSkillUpdate",
            url: "JobSeekerITSkillUpdate",
            defaults: new { controller = "Home", action = "JobSeekerITSkillUpdate" }
            );

            routes.MapRoute(
            name: "JobSeekerLanguageUpdate",
            url: "JobSeekerLanguageUpdate",
            defaults: new { controller = "Home", action = "JobSeekerLanguageUpdate" }
            );
            routes.MapRoute(
            name: "NoOtherSkillAdd",
            url: "NoOtherSkillAdd",
            defaults: new { controller = "Home", action = "NoOtherSkillAdd" }
            );



            routes.MapRoute(
            name: "RecruiterBA",
            url: "RecruiterBA",
            defaults: new { controller = "Home", action = "RecruiterBA" }
            );

            



            routes.MapRoute(
            name: "ProfileParaDelete",
            url: "ProfileParaDelete/{profileno}",
            defaults: new { controller = "Home", action = "ProfileParaDelete", profileno = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "getActivationStatus",
            url: "getActivationStatus/{Ca_ID}",
            defaults: new { controller = "Home", action = "getActivationStatus", Ca_ID = UrlParameter.Optional }
            );


            routes.MapRoute(
            name: "BackOfficeJobSeekerProfile",
            url: "BackOfficeJobSeekerProfile/{guid}",
            defaults: new { controller = "Home", action = "BackOfficeJobSeekerProfile", guid = UrlParameter.Optional }
            );
            routes.MapRoute(
            name: "ForgotPasswordChecker",
            url: "ForgotPasswordChecker",
            defaults: new { controller = "Home", action = "ForgotPasswordChecker" }
            );

            routes.MapRoute(
            name: "ForgotPassword",
            url: "ForgotPassword",
            defaults: new { controller = "Home", action = "ForgotPassword" }
            );

            routes.MapRoute(
            name: "PasswordReset2",
            url: "PasswordReset2",
            defaults: new { controller = "Home", action = "PasswordReset2" }
            );

            routes.MapRoute(
            name: "PasswordReset",
            url: "PasswordReset/{guidlink}",
            defaults: new { controller = "Home", action = "PasswordReset" , guidlink = UrlParameter.Optional }
            );
            routes.MapRoute(
            name: "JobSeekerCertificationsUpdate",
            url: "JobSeekerCertificationsUpdate",
            defaults: new { controller = "Home", action = "JobSeekerCertificationsUpdate" }
            );


            


            routes.MapRoute(
            name: "CanRegContinue",
            url: "CanRegContinue/{Ca_Guid}/{welcome}",
            defaults: new { controller = "Home", action = "CanRegContinue", Ca_Guid = UrlParameter.Optional, welcome = UrlParameter.Optional }
            );
            routes.MapRoute(
            name: "ActivateCandidate",
            url: "ActivateCandidate/{Ca_Guid}/{welcome}",
            defaults: new { controller = "Home", action = "ActivateCandidate", Ca_Guid = UrlParameter.Optional, welcome = UrlParameter.Optional }
            );


            routes.MapRoute(
            name: "AlreadyApplied",
            url: "AlreadyApplied/{VacancyID}",
            defaults: new { controller = "Home", action = "AlreadyApplied", VacancyID = UrlParameter.Optional }
            );


            routes.MapRoute(
            name: "CVViewer",
            url: "CVViewer/{ca_GUID}",
            defaults: new { controller = "Home", action = "CVViewer", ca_GUID = UrlParameter.Optional }
            );

            



            routes.MapRoute(
            name: "RecruiterProfileUpdate",
            url: "RecruiterProfileUpdate",
            defaults: new { controller = "Home", action = "RecruiterProfileUpdate" }
            );


            routes.MapRoute(
            name: "EmployerRegSuccess",
            url: "EmployerRegSuccess",
            defaults: new { controller = "Home", action = "EmployerRegSuccess" }
            );

            routes.MapRoute(
            name: "RecruiterUpdate",
            url: "RecruiterUpdate",
            defaults: new { controller = "Home", action = "RecruiterUpdate" }
            );

            routes.MapRoute(
            name: "JobApplication",
            url: "JobApplication/{VacancyID}",
            defaults: new { controller = "Home", action = "JobApplication", VacancyID = UrlParameter.Optional }
            );

            

            routes.MapRoute(
            name: "CompanyChecker",
            url: "CompanyChecker/{companyname}",
            defaults: new { controller = "Home", action = "CompanyChecker", companyname = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "EmailCheckerEMP",
            url: "EmailCheckerEMP/{EmailID}",
            defaults: new { controller = "Home", action = "EmailCheckerEMP", EmailID = UrlParameter.Optional }
            );



            routes.MapRoute(
            name: "EmailChecker",
            url: "EmailChecker/{EmailID}",
            defaults: new { controller = "Home", action = "EmailChecker", EmailID = UrlParameter.Optional }
            );




            routes.MapRoute(
            name: "ECareerServicesDetails",
            url: "ECareerServicesDetails/{articleid}",
            defaults: new { controller = "Home", action = "ECareerServicesDetails", articleid = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "CCareerServicesDetails",
            url: "CCareerServicesDetails/{articleid}",
            defaults: new { controller = "Home", action = "CCareerServicesDetails", articleid = UrlParameter.Optional }
            );



            routes.MapRoute(
            name: "CareerServices",
            url: "CareerServices",
            defaults: new { controller = "Home", action = "CareerServices" }
            );


            routes.MapRoute(
            name: "EmployerCareerServices",
            url: "EmployerCareerServices",
            defaults: new { controller = "Home", action = "EmployerCareerServices" }
            );

            routes.MapRoute(
             name: "CandidateCareerServices",
             url: "CandidateCareerServices",
             defaults: new { controller = "Home", action = "CandidateCareerServices" }
             );

            routes.MapRoute(
            name: "NonActivated",
            url: "NonActivated",
            defaults: new { controller = "Home", action = "NonActivated" }
            );

            routes.MapRoute(
            name: "Employer",
            url: "Employer/{companyid}/{companyname}",
            defaults: new { controller = "Home", action = "Employer", companyid = UrlParameter.Optional, companyname = UrlParameter.Optional }
            );


            routes.MapRoute(
            name: "RecruitmentActivityReport",
            url: "RecruitmentActivityReport",
            defaults: new { controller = "Home", action = "RecruitmentActivityReport" }
            );




            routes.MapRoute(
            name: "UserActivityReport",
            url: "UserActivityReport",
            defaults: new { controller = "Home", action = "UserActivityReport" }
            );





            routes.MapRoute(
            name: "UpdateJobPreview",
            url: "UpdateJobPreview",
            defaults: new { controller = "Home", action = "UpdateJobPreview" }
            );

            routes.MapRoute(
            name: "UpdateJobDraft",
            url: "UpdateJobDraft",
            defaults: new { controller = "Home", action = "UpdateJobDraft" }
            );

            routes.MapRoute(
            name: "UpdateJob",
            url: "UpdateJob",
            defaults: new { controller = "Home", action = "UpdateJob" }
            );

            routes.MapRoute(
            name: "JobEdit",
            url: "JobEdit/{jobid}/{preview}",
            defaults: new { controller = "Home", action = "JobEdit", jobid = UrlParameter.Optional, preview = UrlParameter.Optional }
            );

            

            routes.MapRoute(
            name: "PublishVacancyy",
            url: "PublishVacancyy/{jobid}",
            defaults: new { controller = "Home", action = "PublishVacancyy", jobid = UrlParameter.Optional }
            );


            routes.MapRoute(
            name: "DeleteVacancy",
            url: "DeleteVacancy",
            defaults: new { controller = "Home", action = "DeleteVacancy" }
            );


            routes.MapRoute(
            name: "NoValidSubscription",
            url: "NoValidSubscription",
            defaults: new { controller = "Home", action = "NoValidSubscription" }
            );



            



            routes.MapRoute(
            name: "CompanyContactDelete",
            url: "CompanyContactDelete/{contactid}",
            defaults: new { controller = "Home", action = "CompanyContactDelete" , contactid = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "CompanyContactEdit",
            url: "CompanyContactEdit",
            defaults: new { controller = "Home", action = "CompanyContactEdit" }
            );


            routes.MapRoute(
            name: "companycontact",
            url: "companycontact",
            defaults: new { controller = "Home", action = "companycontact" }
            );




            



            routes.MapRoute(
            name: "DeleteMedia",
            url: "DeleteMedia/{CMID}",
            defaults: new { controller = "Home", action = "DeleteMedia", CMID = UrlParameter.Optional }
            );


            routes.MapRoute(
            name: "DeleteTestimonials",
            url: "DeleteTestimonials/{CTID}",
            defaults: new { controller = "Home", action = "DeleteTestimonials", CTID = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "DeletePhotoGallery",
            url: "DeletePhotoGallery/{PGID}",
            defaults: new { controller = "Home", action = "DeletePhotoGallery", PGID = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "DeleteVideo",
            url: "DeleteVideo/{CoVideo_ID}",
            defaults: new { controller = "Home", action = "DeleteVideo", CoVideo_ID = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "DeleteCompanyManagement",
            url: "DeleteCompanyManagement/{CompanyManagementID}/{itemno}",
            defaults: new { controller = "Home", action = "DeleteCompanyManagement", CompanyManagementID = UrlParameter.Optional, itemno = UrlParameter.Optional }
            );



            routes.MapRoute(
            name: "CompanyManagement",
            url: "CompanyManagement",
            defaults: new { controller = "Home", action = "CompanyManagement" }
            );





            routes.MapRoute(
            name: "UploadCV",
            url: "UploadCV",
            defaults: new { controller = "Home", action = "UploadCV" }
            );


            routes.MapRoute(
            name: "UpdateCandidateAccount",
            url: "UpdateCandidateAccount",
            defaults: new { controller = "Home", action = "UpdateCandidateAccount" }
            );

            routes.MapRoute(
            name: "Packages",
            url: "Packages",
            defaults: new { controller = "Home", action = "Packages" }
            );




            routes.MapRoute(
            name: "CandidateActivate",
            url: "CandidateActivate/{guid}",
            defaults: new { controller = "Home", action = "CandidateActivate", guid = UrlParameter.Optional }
            );



            routes.MapRoute(
            name: "CandidateReject",
            url: "CandidateReject/{guid}",
            defaults: new { controller = "Home", action = "CandidateReject", guid = UrlParameter.Optional }
            );



            routes.MapRoute(
      name: "getJobSeekerOSkills",
      url: "getJobSeekerOSkills/{Ca_ID}",
      defaults: new { controller = "Home", action = "getJobSeekerOSkills", Ca_ID = UrlParameter.Optional }
      );

            routes.MapRoute(
            name: "getJobSeekerSkills",
            url: "getJobSeekerSkills/{Ca_ID}",
            defaults: new { controller = "Home", action = "getJobSeekerSkills", Ca_ID = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "getJobSeekerLanguages",
            url: "getJobSeekerLanguages/{Ca_ID}",
            defaults: new { controller = "Home", action = "getJobSeekerLanguages", Ca_ID = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "getABCcompanies",
            url: "getABCcompanies/{companyname}",
            defaults: new { controller = "Home", action = "getABCcompanies", companyname = UrlParameter.Optional }
            );
            routes.MapRoute(
          name: "getWorkExperinceJobTitle",
          url: "getWorkExperinceJobTitle/{jobtitle}",
          defaults: new { controller = "Home", action = "getWorkExperinceJobTitle", jobtitle = UrlParameter.Optional }
          );

            routes.MapRoute(
            name: "RegisterEmployerLogin",
            url: "RegisterEmployerLogin/{e}",
            defaults: new { controller = "Home", action = "RegisterEmployerLogin", e = UrlParameter.Optional }
            );

            routes.MapRoute(
             name: "RegisterJobseekerLogin",
             url: "RegisterJobseekerLogin",
             defaults: new { controller = "Home", action = "RegisterJobseekerLogin" }
             );

            routes.MapRoute(
             name: "JobSearch",
             url: "JobSearch",
             defaults: new { controller = "Home", action = "JobSearch" }
             );
            routes.MapRoute(
            name: "JobSearch2",
            url: "JobSearch2",
            defaults: new { controller = "Home", action = "JobSearch2" }
            );
            routes.MapRoute(
            name: "JobSearch3",
            url: "JobSearch3",
            defaults: new { controller = "Home", action = "JobSearch3" }
            );

            routes.MapRoute(
            name: "JobSearchResult",
            url: "JobSearchResult",
            defaults: new { controller = "Home", action = "JobSearchResult" }
            );

            routes.MapRoute(
              name: "CandidateSearchResult",
              url: "CandidateSearchResult",
              defaults: new { controller = "Home", action = "CandidateSearchResult" }
             );


            routes.MapRoute(
                          name: "JobSeekerCertification",
                          url: "JobSeekerCertification",
                          defaults: new { controller = "Home", action = "JobSeekerCertification" }
                         );
            routes.MapRoute(
                          name: "JobSeekerHonours",
                          url: "JobSeekerHonours",
                          defaults: new { controller = "Home", action = "JobSeekerHonours" }
                         );

            routes.MapRoute(
                          name: "JobSeekerLanguage",
                          url: "JobSeekerLanguage",
                          defaults: new { controller = "Home", action = "JobSeekerLanguage" }
                         );





            routes.MapRoute(
                          name: "JobSeekerWorkHistoryUpdate",
                          url: "JobSeekerWorkHistoryUpdate",
                          defaults: new { controller = "Home", action = "JobSeekerWorkHistoryUpdate" }
                         );


            routes.MapRoute(
                          name: "JobSeekerWorkHistory",
                          url: "JobSeekerWorkHistory",
                          defaults: new { controller = "Home", action = "JobSeekerWorkHistory" }
                         );

            routes.MapRoute(
                          name: "JobSeekerWorkHistoryProfile",
                          url: "JobSeekerWorkHistoryProfile",
                          defaults: new { controller = "Home", action = "JobSeekerWorkHistoryProfile" }
                         );

            




            routes.MapRoute(name: "CandidateEducationUpdate",
                                 url: "CandidateEducationUpdate",
                                 defaults: new { controller = "Home", action = "CandidateEducationUpdate" }
                                );




            routes.MapRoute(name: "JobSeekerEducationProfile",
                    url: "JobSeekerEducationProfile",
                    defaults: new { controller = "Home", action = "JobSeekerEducationProfile" }
                   );
            routes.MapRoute(name: "JobSeekerEducation",
                                 url: "JobSeekerEducation",
                                 defaults: new { controller = "Home", action = "JobSeekerEducation" }
                                );


            routes.MapRoute(
             name: "JobseekerSkills",
             url: "JobseekerSkills",
             defaults: new { controller = "Home", action = "JobseekerSkills" }
            );


            routes.MapRoute(
         name: "JobseekerOSkills",
         url: "JobseekerOSkills",
         defaults: new { controller = "Home", action = "JobseekerOSkills" }
        );

            

            routes.MapRoute(
             name: "JobseekerCareerMoveUpdate",
             url: "JobseekerCareerMoveUpdate",
             defaults: new { controller = "Home", action = "JobseekerCareerMoveUpdate" }
            );

            routes.MapRoute(
             name: "JobseekerCareerMove",
             url: "JobseekerCareerMove",
             defaults: new { controller = "Home", action = "JobseekerCareerMove" }
            );

            routes.MapRoute(
             name: "SectorJobs",
             url: "SectorJobs/{sectorid}/{sector}/{page}",
             defaults: new { controller = "Home", action = "SectorJobs", sectorid = UrlParameter.Optional, sector = UrlParameter.Optional, page = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "CompanyJobs",
               url: "CompanyJobs/{companyid}/{page}",
               defaults: new { controller = "Home", action = "CompanyJobs", companyid = UrlParameter.Optional, page = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "CanRegIdealCareerMove",
            url: "CanRegIdealCareerMove/{tabupdate}",
            defaults: new { controller = "Home", action = "CanRegIdealCareerMove", tabupdate = UrlParameter.Optional }
            );


            routes.MapRoute(
          name: "CanRegAccomplishments",
          url: "CanRegAccomplishments/{tabupdate}/{addmore}",
          defaults: new { controller = "Home", action = "CanRegAccomplishments", tabupdate = UrlParameter.Optional, addmore = UrlParameter.Optional }
          );

            routes.MapRoute(
          name: "CanRegITSkills",
          url: "CanRegITSkills/{tabupdate}",
          defaults: new { controller = "Home", action = "CanRegITSkills", tabupdate = UrlParameter.Optional }
          );


            routes.MapRoute(
            name: "CanRegWorkHistory",
            url: "CanRegWorkHistory/{tabupdate}/{addmore}",
            defaults: new { controller = "Home", action = "CanRegWorkHistory", tabupdate = UrlParameter.Optional, addmore = UrlParameter.Optional }
            );

            routes.MapRoute(
            name: "CanRegEducation",
            url: "CanRegEducation/{tabupdate}/{addmore}",
            defaults: new { controller = "Home", action = "CanRegEducation", tabupdate = UrlParameter.Optional, addmore = UrlParameter.Optional }
            );

            routes.MapRoute(
          name: "CanRegProfileSummary",
          url: "CanRegProfileSummary/{tabupdate}",
          defaults: new { controller = "Home", action = "CanRegProfileSummary", tabupdate = UrlParameter.Optional }
          );
            routes.MapRoute(
          name: "Registration",
          url: "Registration",
          defaults: new { controller = "Home", action = "JobseekerRegistration" }
          );


            routes.MapRoute(
                 name: "LatestJobs",
                 url: "LatestJobs/{page}",
                 defaults: new { controller = "Home", action = "LatestJobs", page = UrlParameter.Optional }
            );


            routes.MapRoute(
                 name: "AboutUs",
                 url: "AboutUs",
                 defaults: new { controller = "Home", action = "AboutUs" }
            );


            routes.MapRoute(
                 name: "CVWriting",
                 url: "CVWriting",
                 defaults: new { controller = "Home", action = "CVWriting" }
            );

            routes.MapRoute(
                 name: "CVTips",
                 url: "CVTips",
                 defaults: new { controller = "Home", action = "CVTips" }
            );


            routes.MapRoute(
                name: "CVChecklist",
                url: "CVChecklist",
                defaults: new { controller = "Home", action = "CVChecklist" }
            );


            routes.MapRoute(
                name: "CandidateSearch",
                url: "CandidateSearch",
                defaults: new { controller = "Home", action = "CandidateSearch" }
            );
            routes.MapRoute(
             name: "CandidateSearch2",
             url: "CandidateSearch2",
             defaults: new { controller = "Home", action = "CandidateSearch2" }
         );

            routes.MapRoute(
            name: "CandidateSearch3",
            url: "CandidateSearch3",
            defaults: new { controller = "Home", action = "CandidateSearch3" }
        );
            routes.MapRoute(
              name: "SearchDashboard",
              url: "SearchDashboard",
              defaults: new { controller = "Home", action = "SearchDashboard" }
          );
            routes.MapRoute(
           name: "RegisterCandidate",
           url: "RegisterCandidate",
           defaults: new { controller = "Home", action = "RegisterCandidate" }
       );

            routes.MapRoute(
                name: "ManageVacancies",
                url: "ManageVacancies",
                defaults: new { controller = "Home", action = "ManageVacancies" }
            );




            routes.MapRoute(
                name: "PostedJobPreview",
                url: "PostedJobPreview",
                defaults: new { controller = "Home", action = "PostedJobPreview" }
            );


            routes.MapRoute(
                name: "PostedJobDraft",
                url: "PostedJobDraft",
                defaults: new { controller = "Home", action = "PostedJobDraft" }
            );

            routes.MapRoute(
                name: "PostedJob",
                url: "PostedJob",
                defaults: new { controller = "Home", action = "PostedJob" }
            );

            routes.MapRoute(
                name: "PostAJob",
                url: "PostAJob",
                defaults: new { controller = "Home", action = "PostAJob" }
            );



            



            routes.MapRoute(
                name: "PaymentModeSelected",
                url: "PaymentModeSelected",
                defaults: new { controller = "Home", action = "PaymentModeSelected" }
            );

            routes.MapRoute(
                name: "PaymentMode",
                url: "PaymentMode",
                defaults: new { controller = "Home", action = "PaymentMode" }
            );

            


            routes.MapRoute(
                name: "PaymentFailed",
                url: "PaymentFailed",
                defaults: new { controller = "Home", action = "PaymentFailed" }
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
            name: "RecruiterAlert",
            url: "RecruiterAlert",
            defaults: new { controller = "Home", action = "RecruiterAlert" }
            );
            routes.MapRoute(
             name: "Signout",
             url: "Signout",
             defaults: new { controller = "Home", action = "Signout" }
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
                      name: "JobDescEmployer",
                      url: "JobDescEmployer/{jobid}/{jobtitle}",
                      defaults: new { controller = "Home", action = "JobDescEmployer", jobid = UrlParameter.Optional, jobtitle = UrlParameter.Optional }
                  );


            routes.MapRoute(
                  name: "AddCandidateSkill2",
                  url: "AddCandidateSkill2",
                  defaults: new { controller = "Home", action = "AddCandidateSkill2" }
              );

            routes.MapRoute(
              name: "AddCandidateWorkHistory",
              url: "AddCandidateWorkHistory",
              defaults: new { controller = "Home", action = "AddCandidateWorkHistory" }
          );

            routes.MapRoute(
                name: "AddCandidateDegree",
                url: "AddCandidateDegree",
                defaults: new { controller = "Home", action = "AddCandidateDegree" }
            );
            routes.MapRoute(
               name: "UpdateCandidateProfileSummary",
               url: "UpdateCandidateProfileSummary",
               defaults: new { controller = "Home", action = "UpdateCandidateProfileSummary" }
           );

            routes.MapRoute(
               name: "DeleteCandidateExperince",
               url: "DeleteCandidateExperince/{caExperince}",
               defaults: new { controller = "Home", action = "DeleteCandidateExperince", caExperince = UrlParameter.Optional }
           );

            routes.MapRoute(
                 name: "DeleteCandidateDegree",
                 url: "DeleteCandidateDegree/{CaEdu_ID}",
                 defaults: new { controller = "Home", action = "DeleteCandidateDegree", caExperince = UrlParameter.Optional }
             );
            routes.MapRoute(
                name: "RecruiterSectors2",
                url: "RecruiterSectors2",
                defaults: new { controller = "Home", action = "RecruiterSectors2" }
            );





            routes.MapRoute(
                 name: "DeleteEDU",
                 url: "DeleteEDU/{CaEduID}",
                 defaults: new { controller = "Home", action = "DeleteEDU", CaEduID = UrlParameter.Optional }
             );

            routes.MapRoute(
                 name: "DeleteWH",
                 url: "DeleteWH/{caWHid}",
                 defaults: new { controller = "Home", action = "DeleteWH", caWHid = UrlParameter.Optional }
             );


            routes.MapRoute(
                  name: "DeleteCER",
                  url: "DeleteCER/{caCERid}",
                  defaults: new { controller = "Home", action = "DeleteCER", caCERid = UrlParameter.Optional }
              );


            routes.MapRoute(
                  name: "DeleteICM",
                  url: "DeleteICM/{caICMid}",
                  defaults: new { controller = "Home", action = "DeleteICM", caICMid = UrlParameter.Optional }
              );



            routes.MapRoute(
               name: "DeleteCandidateLanguage",
               url: "DeleteCandidateLanguage/{Caln_ID}",
               defaults: new { controller = "Home", action = "DeleteCandidateLanguage", Caln_ID = UrlParameter.Optional }
            );

            routes.MapRoute(
                  name: "DeleteCandidateSkill",
                  url: "DeleteCandidateSkill/{caskillid}",
                  defaults: new { controller = "Home", action = "DeleteCandidateSkill", caskillid = UrlParameter.Optional }
              );


            routes.MapRoute(
                  name: "DeleteCandidateOSkill",
                  url: "DeleteCandidateOSkill/{ocaskillid}",
                  defaults: new { controller = "Home", action = "DeleteCandidateOSkill", ocaskillid = UrlParameter.Optional }
              );


            routes.MapRoute(
                name: "Search",
                url: "Search",
                defaults: new { controller = "Home", action = "Search" }
            );

            routes.MapRoute(
               name: "Search2",
               url: "Search2/{keywords}/{sector}/{page}",
               defaults: new { controller = "Home", action = "Search2", keywords = UrlParameter.Optional, sector = UrlParameter.Optional, page = UrlParameter.Optional }
           );


            routes.MapRoute(
                name: "JobSeekerProfile",
                url: "JobSeekerProfile",
                defaults: new { controller = "Home", action = "JobSeekerProfile", loggedin = UrlParameter.Optional }
            );


            routes.MapRoute(
                name: "MyCompanies",
                url: "MyCompanies",
                defaults: new { controller = "Home", action = "MyCompanies"}
            );

            

            routes.MapRoute(
                name: "Login",
                url: "Login/{loggedin}/{returnurl}",
                defaults: new { controller = "Home", action = "Login", loggedin = UrlParameter.Optional, returnurl = UrlParameter.Optional }
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
               name: "QuickSearch",
               url: "QuickSearch",
               defaults: new { controller = "Home", action = "QuickSearch", loggedin = UrlParameter.Optional }
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
