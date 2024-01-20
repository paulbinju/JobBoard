using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using Jobs4Bahrainis.Models;

/// <summary>
/// Summary description for dbOperations
/// </summary>
public class dbOperations
{


    public void CompanyVideoOld(int co_ID, string co_Video)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "UPDATE Companies set co_Video=@co_Video where Co_ID=@co_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@co_ID", co_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@co_Video", co_Video);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }



    

    public void ReportAdd(int CompanyID, int ContactID, int ReportTypeID)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "insert into Report_T(CompanyID, ContactID, ReportTypeID, ActivityDate) values(@CompanyID, @ContactID, @ReportTypeID, getdate())";
        mycon.sqlCmd_.Parameters.AddWithValue("@CompanyID", CompanyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ContactID", ContactID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ReportTypeID", ReportTypeID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }


    public void CompanyVideo(int co_ID, string co_Video)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "insert into  CompanyVideo_T (co_id,VideoURL) values(@co_ID,@co_Video)";
        mycon.sqlCmd_.Parameters.AddWithValue("@co_ID", co_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@co_Video", co_Video);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }

    public void jsCertification(int Ca_ID, int CertificationID, string Authority, int FromMonth, int FromYear, int ToMonth, int ToYear, bool DoNotExpire)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT INTO [dbo].[CandidateCertification_T] ([Ca_ID],[CertificationID],[Authority],[FromMonth],[FromYear],[ToMonth],[ToYear],[DoNotExpire],[DOC]) VALUES(@Ca_ID,@CertificationID,@Authority,@FromMonth,@FromYear,@ToMonth,@ToYear,@DoNotExpire,@DOC)";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@CertificationID", CertificationID);
        mycon.sqlCmd_.Parameters.AddWithValue("@Authority", Authority);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromMonth", FromMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromYear", FromYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToMonth", ToMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToYear", ToYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@DoNotExpire", DoNotExpire);
        mycon.sqlCmd_.Parameters.AddWithValue("@DOC", DateTime.Now);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }

    public void jsCertificationUpdate(int CaCertification_ID, int CertificationID, string Authority, int FromMonth, int FromYear, int ToMonth, int ToYear, bool DoNotExpire)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "UPDATE [dbo].[CandidateCertification_T] SET [CertificationID]=@CertificationID,[Authority]=@Authority," +
            "[FromMonth]=@FromMonth,[FromYear]=@FromYear,[ToMonth]=@ToMonth,[ToYear]=@ToYear,[DoNotExpire]=@DoNotExpire WHERE CaCertification_ID=@CaCertification_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@CaCertification_ID", CaCertification_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@CertificationID", CertificationID);
        mycon.sqlCmd_.Parameters.AddWithValue("@Authority", Authority);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromMonth", FromMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromYear", FromYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToMonth", ToMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToYear", ToYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@DoNotExpire", DoNotExpire);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }




    public void jsHonours(int Ca_Id, string Title, string Associatedwith, string Issuer, string Issuedon, string Description)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT INTO [dbo].[CandidateHonours]([Ca_Id],[Title],[Associatedwith],[Issuer],[Issuedon],[Description]) VALUES(@Ca_Id,@Title,@Associatedwith,@Issuer,@Issuedon,@Description)";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_Id", Ca_Id);
        mycon.sqlCmd_.Parameters.AddWithValue("@Title", Title);
        mycon.sqlCmd_.Parameters.AddWithValue("@Associatedwith", Associatedwith);
        mycon.sqlCmd_.Parameters.AddWithValue("@Issuer", Issuer);
        mycon.sqlCmd_.Parameters.AddWithValue("@Issuedon", Issuedon);
        mycon.sqlCmd_.Parameters.AddWithValue("@Description", Description);

        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();

    }

    public void jsSkillUpdate(int CaSkill_ID, string SkillID, string SkillLevelID)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "update CandidateSkill_T set SkillID=@SkillID,SkillLevelID=@SkillLevelID where CaSkill_ID=@CaSkill_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@CaSkill_ID", CaSkill_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@SkillID", SkillID);
        mycon.sqlCmd_.Parameters.AddWithValue("@SkillLevelID", SkillLevelID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }

    public void jsOSkillUpdate(int CaOSkill_ID, string OSkillID, string OSkillLevelID)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "update CandidateOtherSkill_T set OSkillID=@OSkillID,OSkillLevelID=@OSkillLevelID where CaOSkill_ID=@CaOSkill_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@CaOSkill_ID", CaOSkill_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@OSkillID", OSkillID);
        mycon.sqlCmd_.Parameters.AddWithValue("@OSkillLevelID", OSkillLevelID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }



    public void jsLanguageUpdate(int Calanguage_ID, string LanguageID, string ProficiencyID)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "update CandidateLanguage_T set languageid=@languageid,ProficiencyID=@ProficiencyID where calanguage_id=@Calanguage_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@LanguageID", LanguageID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ProficiencyID", ProficiencyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@Calanguage_ID", Calanguage_ID);

        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }

    public void jsLanguage(int Ca_Id, string LanguageID, string ProficiencyID)
    {


        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select ca_id from CandidateLanguage_T where ca_id=@Ca_ID and LanguageID=@LanguageID";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_Id);
        mycon.sqlCmd_.Parameters.AddWithValue("@LanguageID", LanguageID);


        mycon.sqlConOpen();
        int caid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();

        if (caid == 0)
        {
            mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "INSERT INTO [dbo].[CandidateLanguage_T]([Ca_Id],[LanguageID],[ProficiencyID])VALUES (@Ca_Id,@LanguageID,@ProficiencyID);Update Candidates set ca_CompletedRegTabs=5 where ca_id=@Ca_Id and ca_CompletedRegTabs<5";
            mycon.sqlCmd_.Parameters.AddWithValue("@Ca_Id", Ca_Id);
            mycon.sqlCmd_.Parameters.AddWithValue("@LanguageID", LanguageID);
            mycon.sqlCmd_.Parameters.AddWithValue("@ProficiencyID", ProficiencyID);
            mycon.sqlConOpen();
            mycon.sqlCmd().ExecuteNonQuery();
            mycon.sqlConClose();
        }
    }


    public void jsCareerMove(int Ca_ID, int JobTitleID, int FunctionID, int IndustryID, int ExperienceID, int ExpectedSalaryID)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "Delete from CandidateIdealCareerMove_T where Ca_ID=@Ca_ID;INSERT INTO CandidateIdealCareerMove_T (Ca_ID,JobTitleID,FunctionID,IndustryID,ExperienceID,ExpectedSalaryID,DOC) VALUES (@Ca_ID,@JobTitleID,@FunctionID,@IndustryID,@ExperienceID,@ExpectedSalaryID,@DOC);Update Candidates set ca_CompletedRegTabs=6 where ca_id=@Ca_ID and ca_CompletedRegTabs<6";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@JobTitleID", JobTitleID);
        mycon.sqlCmd_.Parameters.AddWithValue("@FunctionID", FunctionID);
        mycon.sqlCmd_.Parameters.AddWithValue("@IndustryID", IndustryID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ExperienceID", ExperienceID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ExpectedSalaryID", ExpectedSalaryID);
        mycon.sqlCmd_.Parameters.AddWithValue("@DOC", DateTime.Now);

        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();

    }


    public void jsSkills(int Ca_ID, int SkillID, int SkillLevelID)
    {

        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select ca_id from CandidateSkill_T where ca_id=@Ca_ID and skillid=@SkillID";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@SkillID", SkillID);
      

        mycon.sqlConOpen();
        int caid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();

        if (caid == 0)
        {

            mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "insert into [CandidateSkill_T](Ca_Id,SkillID,SkillLevelID) values(@Ca_Id,@SkillID,@SkillLevelID);";
            mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
            mycon.sqlCmd_.Parameters.AddWithValue("@SkillID", SkillID);
            mycon.sqlCmd_.Parameters.AddWithValue("@SkillLevelID", SkillLevelID);

            mycon.sqlConOpen();
            mycon.sqlCmd().ExecuteNonQuery();
            mycon.sqlConClose();
        }

    }

    public void jsNOOSkills(int Ca_ID)
    {

        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "Update Candidates set ca_CompletedRegTabs=4 where ca_id=@Ca_ID and ca_CompletedRegTabs<4";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
        mycon.sqlConOpen();
        int caid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
    }

        public void jsOSkills(int Ca_ID, int OSkillID, int OSkillLevelID)
    {

        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select ca_id from CandidateOtherSkill_T where ca_id=@Ca_ID and OSkillID=@OSkillID";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@OSkillID", OSkillID);
       

        mycon.sqlConOpen();
        int caid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();

        if (caid == 0)
        {
            mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "insert into [CandidateOtherSkill_T](Ca_Id,OSkillID,OSkillLevelID) values(@Ca_Id,@OSkillID,@OSkillLevelID);Update Candidates set ca_CompletedRegTabs=4 where ca_id=@Ca_ID and ca_CompletedRegTabs<4";
            mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
            mycon.sqlCmd_.Parameters.AddWithValue("@OSkillID", OSkillID);
            mycon.sqlCmd_.Parameters.AddWithValue("@OSkillLevelID", OSkillLevelID);

            mycon.sqlConOpen();
            mycon.sqlCmd().ExecuteNonQuery();
            mycon.sqlConClose();
        }
    }



    public void jsWorkHistory(int Ca_ID, int JobTitleID, string Company, int JobLevelID, int ReportingToID, int IndustryID, int FunctionID, int FromMonth, int FromYear, int ToMonth, int ToYear, bool CurrentlyWorkingHere, int JobLocationID, int SalaryID, string Description)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = @"INSERT INTO [dbo].[CandidateWorkHistory_T]([Ca_ID],[JobTitleID],[Company],[JobLevelID],[ReportingToID],[IndustryID],[FunctionID],[FromMonth],[FromYear],[ToMonth],[ToYear],[CurrentlyWorkingHere],[JobLocationID],[SalaryID],[Description],[DOC])
        VALUES(@Ca_ID,@JobTitleID,@Company,@JobLevelID,@ReportingToID,@IndustryID,@FunctionID,@FromMonth,@FromYear,@ToMonth,@ToYear,@CurrentlyWorkingHere,@JobLocationID,@SalaryID,@Description,@DOC);Update Candidates set ca_CompletedRegTabs=3 where ca_id=@Ca_ID and ca_CompletedRegTabs<3";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@JobTitleID", JobTitleID);
        mycon.sqlCmd_.Parameters.AddWithValue("@Company", Company);
        mycon.sqlCmd_.Parameters.AddWithValue("@JobLevelID", JobLevelID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ReportingToID", ReportingToID);
        mycon.sqlCmd_.Parameters.AddWithValue("@IndustryID", IndustryID);
        mycon.sqlCmd_.Parameters.AddWithValue("@FunctionID", FunctionID);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromMonth", FromMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromYear", FromYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToMonth", ToMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToYear", ToYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@CurrentlyWorkingHere", CurrentlyWorkingHere);
        mycon.sqlCmd_.Parameters.AddWithValue("@JobLocationID", JobLocationID);
        mycon.sqlCmd_.Parameters.AddWithValue("@SalaryID", SalaryID);
        mycon.sqlCmd_.Parameters.AddWithValue("@Description", Description);
        mycon.sqlCmd_.Parameters.AddWithValue("@DOC", DateTime.Now);

        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();

    }

    public void jsWorkHistoryUpdate(int CaWorkHistory_ID, int JobTitleID, string Company, int JobLevelID, int ReportingToID, int IndustryID, int FunctionID, int FromMonth, int FromYear, int ToMonth, int ToYear, bool CurrentlyWorkingHere, int JobLocationID, int SalaryID, string Description)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = @"UPDATE CandidateWorkHistory_T SET [JobTitleID]=@JobTitleID,[Company]=@Company,
[JobLevelID]=@JobLevelID,[ReportingToID]=@ReportingToID,[IndustryID]=@IndustryID,[FunctionID]=@FunctionID,[FromMonth]=@FromMonth,[FromYear]=@FromYear
,[ToMonth]=@ToMonth,[ToYear]=@ToYear,[CurrentlyWorkingHere]=@CurrentlyWorkingHere,[JobLocationID]=@JobLocationID,[SalaryID]=@SalaryID,[Description]=@Description where CaWorkHistory_ID=@CaWorkHistory_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@CaWorkHistory_ID", CaWorkHistory_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@JobTitleID", JobTitleID);
        mycon.sqlCmd_.Parameters.AddWithValue("@Company", Company);
        mycon.sqlCmd_.Parameters.AddWithValue("@JobLevelID", JobLevelID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ReportingToID", ReportingToID);
        mycon.sqlCmd_.Parameters.AddWithValue("@IndustryID", IndustryID);
        mycon.sqlCmd_.Parameters.AddWithValue("@FunctionID", FunctionID);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromMonth", FromMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromYear", FromYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToMonth", ToMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToYear", ToYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@CurrentlyWorkingHere", CurrentlyWorkingHere);
        mycon.sqlCmd_.Parameters.AddWithValue("@JobLocationID", JobLocationID);
        mycon.sqlCmd_.Parameters.AddWithValue("@SalaryID", SalaryID);
        mycon.sqlCmd_.Parameters.AddWithValue("@Description", Description);
        mycon.sqlCmd_.Parameters.AddWithValue("@DOC", DateTime.Now);

        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();

    }


    public int jsEducation(decimal Ca_ID, string UniversityID, string UniCountryID, string DegreeID, string SpecializationID, string GradeID, string FromMonth, string FromYear, string ToMonth, string ToYear, bool CurrentlyStudyHere, string Activities)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select ca_isfresher from candidates where ca_id=@Ca_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);

        mycon.sqlConOpen();

        int isfresher = 0;

        var frsh = mycon.sqlCmd().ExecuteScalar();

        if (frsh != null)
        {
            isfresher = 2;
        }
        else if(Convert.ToInt32(frsh) == 1)
        {
            isfresher = 3;
        }
        else if (Convert.ToInt32(frsh) == 0)
        {
            isfresher = 2;
        }

        mycon.sqlConClose();
 
        mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = @"INSERT INTO [dbo].[CandidateEducation_T]([Ca_ID],[UniversityID],[UniCountryID],[DegreeID] ,[SpecializationID] ,[GradeID] ,[FromMonth],[FromYear] ,[ToMonth] ,[ToYear] ,[CurrentlyStudyHere] ,[Activities],[DOC])
        VALUES(@Ca_ID,@UniversityID,@UniCountryID,@DegreeID,@SpecializationID,@GradeID,@FromMonth,@FromYear,@ToMonth,@ToYear,@CurrentlyStudyHere,@Activities,@DOC);Update Candidates set ca_CompletedRegTabs=" + isfresher + " where ca_id=@Ca_ID and ca_CompletedRegTabs<2;select @@identity";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@UniversityID", UniversityID);
        mycon.sqlCmd_.Parameters.AddWithValue("@UniCountryID", UniCountryID);
        mycon.sqlCmd_.Parameters.AddWithValue("@DegreeID", DegreeID);
        mycon.sqlCmd_.Parameters.AddWithValue("@SpecializationID", SpecializationID);
        mycon.sqlCmd_.Parameters.AddWithValue("@GradeID", GradeID);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromMonth", FromMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromYear", FromYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToMonth", ToMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToYear", ToYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@Activities", Activities);
        mycon.sqlCmd_.Parameters.AddWithValue("@CurrentlyStudyHere", CurrentlyStudyHere);
        mycon.sqlCmd_.Parameters.AddWithValue("@DOC", DateTime.Now);

        mycon.sqlConOpen();
        int cdid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();

        return cdid;

    }


    public void jsEducationUpdate(int CaEdu_ID, string UniversityID, string UniCountryID, string DegreeID, string SpecializationID, string GradeID, string FromMonth, string FromYear, string ToMonth, string ToYear, bool CurrentlyStudyHere, string Activities)
    {


        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = @"UPDATE CandidateEducation_T SET [UniversityID]=@UniversityID,[UniCountryID]=@UniCountryID,[DegreeID]=@DegreeID ,[SpecializationID]=@SpecializationID
,[GradeID]=@GradeID ,[FromMonth]=@FromMonth,[FromYear]=@FromYear ,[ToMonth]=@ToMonth ,[ToYear]=@ToYear ,[CurrentlyStudyHere]=@CurrentlyStudyHere ,[Activities]=@Activities where CaEdu_ID=@CaEdu_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@CaEdu_ID", CaEdu_ID);
        mycon.sqlCmd_.Parameters.AddWithValue("@UniversityID", UniversityID);
        mycon.sqlCmd_.Parameters.AddWithValue("@UniCountryID", UniCountryID);
        mycon.sqlCmd_.Parameters.AddWithValue("@DegreeID", DegreeID);
        mycon.sqlCmd_.Parameters.AddWithValue("@SpecializationID", SpecializationID);
        mycon.sqlCmd_.Parameters.AddWithValue("@GradeID", GradeID);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromMonth", FromMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@FromYear", FromYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToMonth", ToMonth);
        mycon.sqlCmd_.Parameters.AddWithValue("@ToYear", ToYear);
        mycon.sqlCmd_.Parameters.AddWithValue("@Activities", Activities);
        mycon.sqlCmd_.Parameters.AddWithValue("@CurrentlyStudyHere", CurrentlyStudyHere);
       

        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();

    }


    

    public void CompanySectors(int coID, int SecID)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT into CompanySectors (cs_created,cs_co_companyid,cs_lm_sectorid) values (@cs_created,@co_companyid,@cs_lm_sectorid)";
        mycon.sqlCmd_.Parameters.AddWithValue("@cs_created", DateTime.Now);
        mycon.sqlCmd_.Parameters.AddWithValue("@co_companyid", coID);
        mycon.sqlCmd_.Parameters.AddWithValue("@cs_lm_sectorid", SecID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();

    }

    public void CompanyHeaderExt(int coID, string extn)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "update Companies set co_HeaderExtension=@extention where co_id=@coid";
        mycon.sqlCmd_.Parameters.AddWithValue("@extention", extn);
        mycon.sqlCmd_.Parameters.AddWithValue("@coid", coID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }

    public void CompanyLogoExt(int coID, string extn)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "update Companies set co_LogoExtension=@extention, co_HasLogo=1,co_AcceptedTerms=1,co_MasterAc=1 where co_id=@coid";
        mycon.sqlCmd_.Parameters.AddWithValue("@extention", extn);
        mycon.sqlCmd_.Parameters.AddWithValue("@coid", coID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }


    public void CandidatePhotoExt(decimal caID, string extn)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "update Candidates set ca_PhotoExtension=@extention, ca_HasPhoto=1 where ca_Id=@ca_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@extention", extn);
        mycon.sqlCmd_.Parameters.AddWithValue("@ca_ID", caID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }


    public void VacancySectors(int VacancyID, int SecID)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT into VacancySectors (vs_created,vs_vc_vacancyid,vs_lm_sectorid) values (@vs_created,@vs_vc_vacancyid,@vs_lm_sectorid)";
        mycon.sqlCmd_.Parameters.AddWithValue("@vs_created", DateTime.Now);
        mycon.sqlCmd_.Parameters.AddWithValue("@vs_vc_vacancyid", VacancyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@vs_lm_sectorid", SecID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();

    }








    public bool PackageCheck(int CompanyID, string TypeID)
    {
        SqlCon mycon = new SqlCon();
        SqlDataReader sDr;
        bool IsValid = false;
        mycon.sqlCmd_.CommandText = "select cp_pt_PackageTypeId from CompanyPackages where cp_co_CompanyId=@companyID and cp_Expires>=GETDATE()";
        mycon.sqlCmd_.Parameters.AddWithValue("@companyID", CompanyID);
        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        while (sDr.Read())
        {
            if (TypeID == "CV")
            {

                if (Convert.ToString(sDr["cp_pt_PackageTypeId"]) == "19")
                {
                    IsValid = true;
                }

            }
            else if (TypeID == "JOBS")
            {
                IsValid = true;
            }


        }


        mycon.sqlConClose();

        return IsValid;
    }

    public string Vacancy_Title(int jobid)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select vc_title from vacancies where vc_id=" + jobid;
        mycon.sqlConOpen();
        string jobtitle = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        return jobtitle;
    }




    public string RecruiterEmployer(int co_id)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select co_clienttype from companies where co_id=@co_id";
        mycon.sqlCmd_.Parameters.AddWithValue("@co_id", co_id);
        mycon.sqlConOpen();
        string clienttype = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        return clienttype;
    }


    public string getCv(int ca_id)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select top 1 substring(convert(nvarchar(50), cd_Guid),1,2)+'/'+substring(convert(nvarchar(50), cd_Guid),3,2)+'/'+substring(convert(nvarchar(50), cd_Guid),5,2)+'/'+convert(nvarchar(50), cd_Guid)+'.'+cd_FileExtension as cvfile from candidatedocuments where cd_id in(select max(cd_id) from candidatedocuments where  cd_ca_candidateid=@ca_id)";
        mycon.sqlCmd_.Parameters.AddWithValue("@ca_id", ca_id);
        mycon.sqlConOpen();
        string cvname = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        return cvname;
    }


    public bool contactExist(string emailid)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = @"select co_id from companies where co_deleted is null and co_id in
                                    (select ct_co_companyid from contacts where ct_emailaddress=@email_id)";
        mycon.sqlCmd_.Parameters.AddWithValue("@email_id", emailid);
        mycon.sqlConOpen();
        string compid = "";
        compid = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();

        bool ctexist = false;
        if (compid != "")
        {
            ctexist = true;
        }

        return ctexist;
    }





    public string Company_Name(int coid)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select co_name from companies where co_id=" + coid;
        mycon.sqlConOpen();
        string companyname = Convert.ToString(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        return companyname;
    }


    public bool activationCheck(int CompanyID)
    {
        SqlCon mycon = new SqlCon();
        bool IsValid = false;
        mycon.sqlCmd_.CommandText = "select co_activated from companies where co_id=@companyID";
        mycon.sqlCmd_.Parameters.AddWithValue("@companyID", CompanyID);
        mycon.sqlConOpen();
        IsValid = Convert.ToBoolean(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        return IsValid;
    }


    public bool popUpExpVacancies(int CompanyID) // 10 times pop up will be shown for a company
    {
        SqlCon mycon = new SqlCon();

        bool ShowPopUp = false;

        int counter = 0;
        mycon.sqlCmd_.CommandText = "select count(vc_id) from vacancies where vc_co_CompanyId=" + CompanyID + " and vc_Deleted is null and vc_ExpiryDate<=GETDATE() and vc_st_statusid=1";
        mycon.sqlCmd_.Parameters.AddWithValue("@companyID", CompanyID);
        mycon.sqlConOpen();
        counter = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        // not existing
        if (counter == 0)
            return false;
        else
            return true;
    }

    public bool popUpCounter(int CompanyID) // 10 times pop up will be shown for a company
    {
        SqlCon mycon = new SqlCon();

        bool ShowPopUp = false;

        int counter = 0;
        mycon.sqlCmd_.CommandText = "select counter from PopUpCompany_T where companyid=@companyID";
        mycon.sqlCmd_.Parameters.AddWithValue("@companyID", CompanyID);
        mycon.sqlConOpen();
        counter = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        // not existing
        if (counter == 0)
        {
            mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "insert into PopUpCompany_T(companyid,counter) values(@companyID,10)";
            mycon.sqlCmd_.Parameters.AddWithValue("@companyID", CompanyID);
            mycon.sqlConOpen();
            counter = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
            mycon.sqlConClose();
            ShowPopUp = true;
        }
        else
        {

            if (counter > 1)
            {
                mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "update PopUpCompany_T set counter=counter-1 where companyid=@companyID";
                mycon.sqlCmd_.Parameters.AddWithValue("@companyID", CompanyID);
                mycon.sqlConOpen();
                counter = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
                mycon.sqlConClose();
                ShowPopUp = true;
            }
            else
            {

                ShowPopUp = false;
            }


        }


        return ShowPopUp;
    }



    public bool masterCheck(int CompanyID)
    {
        SqlCon mycon = new SqlCon();
        bool IsMaster = false;
        mycon.sqlCmd_.CommandText = "select co_MasterAc from companies where co_id=@companyID";
        mycon.sqlCmd_.Parameters.AddWithValue("@companyID", CompanyID);
        mycon.sqlConOpen();
        IsMaster = Convert.ToBoolean(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        return IsMaster;
    }

    public DataTable getAssociatedCompanies(int CompanyID)
    {
        SqlCon mycon = new SqlCon();
        SqlDataAdapter sDa = new SqlDataAdapter();
        DataTable dt = new DataTable();
        mycon.sqlCmd_.CommandText = "select co_id,co_name from companies where co_id in(select assosiatedcompanyid  from companymaster_t where mastercompanyid=@companyID)";
        mycon.sqlCmd_.Parameters.AddWithValue("@companyID", CompanyID);
        mycon.sqlConOpen();
        sDa.SelectCommand = mycon.sqlCmd();
        sDa.Fill(dt);
        mycon.sqlConClose();
        return dt;
    }


    public void countAdvSearch(int CompanyID)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT into CounterAdvSrchCompany_T (co_id) values (@co_id)";
        mycon.sqlCmd_.Parameters.AddWithValue("@co_id", CompanyID);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }

    public void countAdvSearchKeywords(int CompanyID, string keywords)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT into CounterAdvSrchKeywords_T (co_id,keywords) values (@co_id,@keywords)";
        mycon.sqlCmd_.Parameters.AddWithValue("@co_id", CompanyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@keywords", keywords);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }

    public void countAdvSearchQualification(int CompanyID, string qualification)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT into CounterAdvSrchQualification_T (co_id,qualification) values (@co_id,@qualification)";
        mycon.sqlCmd_.Parameters.AddWithValue("@co_id", CompanyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@qualification", qualification);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }


    public void countAdvSearchSector(int CompanyID, string sectorid)
    {
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT into CounterAdvSrchSectors_T (co_id,sectorid) values (@co_id,@sectorid)";
        mycon.sqlCmd_.Parameters.AddWithValue("@co_id", CompanyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@sectorid", sectorid);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();
    }




    public void ActivationCode(int companyid, string act, string emailaddress)
    {

        if (act != "")
        {

            int rowid = 0;
            SqlCon mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "select id from EmployerActivationCode_T where activationcodes=@act and employerid is null";
            mycon.sqlCmd_.Parameters.AddWithValue("@act", act);
            mycon.sqlConOpen();
            rowid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
            mycon.sqlConClose();

            if (rowid != 0) // activated
            {
                mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "update EmployerActivationCode_T set employerid=@coid where id=@id";
                mycon.sqlCmd_.Parameters.AddWithValue("@coid", companyid);
                mycon.sqlCmd_.Parameters.AddWithValue("@id", rowid);
                mycon.sqlConOpen();
                mycon.sqlCmd().ExecuteScalar();
                mycon.sqlConClose();

                mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "update companies set co_activated=1 where co_id=@coid";
                mycon.sqlCmd_.Parameters.AddWithValue("@coid", companyid);
                mycon.sqlConOpen();
                mycon.sqlCmd().ExecuteScalar();
                mycon.sqlConClose();

                common cmn = new common();

                // notify employer

                string path = HttpContext.Current.Server.MapPath("~/EN/Templates/EmployerActivation.htm");
                string bodycontent = File.ReadAllText(path);
              //  cmn.SendMail("admin@jobs4bahrainis.com", emailaddress, "Account Activation - J4B Website", bodycontent);

                string bodycontent2;
                bodycontent2 = "<font face='Arial' size='2'><br><p>";
                bodycontent2 += Company_Name(companyid) + " - has successfully activated their employer account on Jobs4bahrainis.com website.</p>";
                bodycontent2 += "<br><br><br></font>";

             //   cmn.SendMail("admin@jobs4bahrainis.com", "info@jobs4bahrainis.com", "Company Activated", bodycontent2);


            }

        }
    }


    public void Activate(int companyid)
    {



        int rowid = 0;
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select top 1 id from EmployerActivationCode_T where  employerid is null";
        mycon.sqlConOpen();
        rowid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();

        if (rowid != 0) // activated
        {
            mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "update EmployerActivationCode_T set employerid=@coid where id=@id";
            mycon.sqlCmd_.Parameters.AddWithValue("@coid", companyid);
            mycon.sqlCmd_.Parameters.AddWithValue("@id", rowid);
            mycon.sqlConOpen();
            mycon.sqlCmd().ExecuteScalar();
            mycon.sqlConClose();

            mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "update companies set co_activated=1 where co_id=@coid";
            mycon.sqlCmd_.Parameters.AddWithValue("@coid", companyid);
            mycon.sqlConOpen();
            mycon.sqlCmd().ExecuteScalar();
            mycon.sqlConClose();




        }


    }







    public void ActivationCodeAR(int companyid, string act, string emailaddress)
    {

        if (act != "")
        {

            int rowid = 0;
            SqlCon mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = "select id from EmployerActivationCode_T where activationcodes=@act and employerid is null";
            mycon.sqlCmd_.Parameters.AddWithValue("@act", act);
            mycon.sqlConOpen();
            rowid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
            mycon.sqlConClose();

            if (rowid != 0) // activated
            {
                mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "update EmployerActivationCode_T set employerid=@coid where id=@id";
                mycon.sqlCmd_.Parameters.AddWithValue("@coid", companyid);
                mycon.sqlCmd_.Parameters.AddWithValue("@id", rowid);
                mycon.sqlConOpen();
                mycon.sqlCmd().ExecuteScalar();
                mycon.sqlConClose();

                mycon = new SqlCon();
                mycon.sqlCmd_.CommandText = "update companies set co_activated=1 where co_id=@coid";
                mycon.sqlCmd_.Parameters.AddWithValue("@coid", companyid);
                mycon.sqlConOpen();
                mycon.sqlCmd().ExecuteScalar();
                mycon.sqlConClose();

                common cmn = new common();

                // notify employer

                string path = HttpContext.Current.Server.MapPath("~/AR/Templates/EmployerActivation.htm");
                string bodycontent = File.ReadAllText(path);
              //  cmn.SendMail("admin@jobs4bahrainis.com", emailaddress, "Account Activation - J4B Website", bodycontent);

                string bodycontent2;
                bodycontent2 = "<font face='Arial' size='2'><br><p>";
                bodycontent2 += Company_Name(companyid) + " - has successfully activated their employer account on Jobs4bahrainis.com website.</p>";
                bodycontent2 += "<br><br><br></font>";

            //    cmn.SendMail("admin@jobs4bahrainis.com", "info@jobs4bahrainis.com", "Company Activated", bodycontent2);


            }

        }
    }

    public string getVacancySectors(int vcid)
    {
        SqlCon mycon = new SqlCon();
        SqlDataReader sDr;
        mycon.sqlCmd_.CommandText = "select lm_value from listsmaster where lm_id in(select vs_lm_sectorid from VacancySectors where vs_vc_vacancyid=@vcid)";
        mycon.sqlCmd_.Parameters.AddWithValue("@vcid", vcid);
        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        string sectors = "";
        while (sDr.Read())
        {
            if (sDr.HasRows)
            {
                sectors = Convert.ToString(sDr["lm_value"]) + ",";

            }

        }


        mycon.sqlConClose();

        return sectors;
    }

    public List<CompanyNames> abcCompanies(string coname)
    {
        SqlConDirDev mycon = new SqlConDirDev();
        SqlDataReader sDr;
        mycon.sqlCmd_.CommandText = "select distinct top 100  companyname as companies from onlinecompany oc join country c on oc.countrycode=c.code where oc.countrycode='BH' and companyname like @coname";
        mycon.sqlCmd_.Parameters.AddWithValue("@coname", "%" + coname + "%");

        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        List<CompanyNames> companies = new List<CompanyNames>();
        CompanyNames lm;

        while (sDr.Read())
        {
            lm = new CompanyNames();
            lm.Company = Convert.ToString(sDr[0]);
            companies.Add(lm);

        }
        mycon.sqlConClose();
        return companies;
    }

    

    public List<JobTitles> jobTitles(string jtitle)
    {
        SqlCon mycon = new SqlCon();
        SqlDataReader sDr;
        //mycon.sqlCmd_.CommandText = "select distinct top 100   JobTitle  from CandidateExperience where JobTitle like @jtitle";
        mycon.sqlCmd_.CommandText = "select  top 100   lm_Value  from ListsMaster where lm_lt_ListTypeId =22 and lm_Value like @jtitle";
        mycon.sqlCmd_.Parameters.AddWithValue("@jtitle", jtitle + "%");

        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        List<JobTitles> jobTitles = new List<JobTitles>();
        JobTitles jt;

        while (sDr.Read())
        {
            jt = new JobTitles();
            jt.JobTitle = Convert.ToString(sDr[0]);
            jobTitles.Add(jt);

        }
        mycon.sqlConClose();
        return jobTitles;
    }

 



    public List<CandidateSkills> getCandidateSkills(decimal Ca_ID)
    {
        SqlCon mycon = new SqlCon();
        SqlDataReader sDr;
        mycon.sqlCmd_.CommandText = "select top 50 skillid,skilllevelid, caskill_id as CaSkillID,Ca_ID,lm.lm_Value as Skills,lmsk.lm_Value as SkillLevel from [dbo].[CandidateSkill_T] cs join ListsMaster lm on cs.SkillID=lm.lm_Id join ListsMaster lmsk on lmsk.lm_Id= cs.SkillLevelID  where Ca_ID=@Ca_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);

        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        List<CandidateSkills> lmvalues = new List<CandidateSkills>();
        CandidateSkills lm;

        while (sDr.Read())
        {
            lm = new CandidateSkills();
            lm.CaSkillID = Convert.ToInt32(sDr["CaSkillID"]);
            lm.Ca_ID = Convert.ToInt32(sDr["Ca_ID"]);
            lm.Skills = Convert.ToString(sDr["Skills"]);
            lm.SkillLevel = Convert.ToString(sDr["SkillLevel"]);
            lm.SkillID = Convert.ToInt32(sDr["SkillID"]);
            lm.SkillLevelID = Convert.ToInt32(sDr["SkillLevelID"]);

            lmvalues.Add(lm);
        }
        mycon.sqlConClose();
        return lmvalues;
    }




    public List<CandidateSkills> getCandidateOSkills(decimal Ca_ID)
    {
        SqlCon mycon = new SqlCon();
        SqlDataReader sDr;
        mycon.sqlCmd_.CommandText = "select top 50 oskillid,oskilllevelid,caoskill_id as CaSkillID,Ca_ID,lm.lm_Value as OSkills,lmsk.lm_Value as OSkillLevel from [dbo].[CandidateOtherSkill_T] cs join ListsMaster lm on cs.OSkillID=lm.lm_Id join ListsMaster lmsk on lmsk.lm_Id= cs.OSkillLevelID  where Ca_ID=@Ca_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);

        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        List<CandidateSkills> lmvalues = new List<CandidateSkills>();
        CandidateSkills lm;

        while (sDr.Read())
        {
            lm = new CandidateSkills();
            lm.CaSkillID = Convert.ToInt32(sDr["CaSkillID"]);
            lm.Ca_ID = Convert.ToInt32(sDr["Ca_ID"]);
            lm.Skills = Convert.ToString(sDr["OSkills"]);
            lm.SkillLevel = Convert.ToString(sDr["OSkillLevel"]);
            lm.SkillID = Convert.ToInt32(sDr["OSkillID"]);
            lm.SkillLevelID = Convert.ToInt32(sDr["OSkillLevelID"]);
            lmvalues.Add(lm);
        }
        mycon.sqlConClose();
        return lmvalues;
    }
    
 
    public List<CandidateLanguages> getCandidateLanguages(decimal Ca_ID)
    {
        SqlCon mycon = new SqlCon();
        SqlDataReader sDr;
        mycon.sqlCmd_.CommandText = "select top 50 LanguageID,ProficiencyID,CaLanguage_ID,Ca_ID, lml.lm_Value as [Language], lmp.lm_Value as Proficiency  from [dbo].[CandidateLanguage_T] cl join ListsMaster lml on lml.lm_Id= cl.LanguageID join ListsMaster lmp on lmp.lm_Id= cl.ProficiencyID where Ca_ID=@Ca_ID";
        mycon.sqlCmd_.Parameters.AddWithValue("@Ca_ID", Ca_ID);

        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        List<CandidateLanguages> lmvalues = new List<CandidateLanguages>();
        CandidateLanguages lm;

        while (sDr.Read())
        {
            lm = new CandidateLanguages();
            lm.Caln_ID = Convert.ToInt32(sDr["CaLanguage_ID"]);
            lm.Ca_ID = Convert.ToInt32(sDr["Ca_ID"]);
            lm.Language = Convert.ToString(sDr["Language"]);
            lm.Proficiency = Convert.ToString(sDr["Proficiency"]);
            lm.ProficiencyID = Convert.ToInt32(sDr["ProficiencyID"]);
            lm.LanguageID = Convert.ToInt32(sDr["LanguageID"]);

            lmvalues.Add(lm);
        }
        mycon.sqlConClose();
        return lmvalues;
    }

    public List<ListMaster> getlist(int listtypeid)
    {
        SqlCon mycon = new SqlCon();
        SqlDataReader sDr;
        mycon.sqlCmd_.CommandText = "select lm_Id,lm_Value from ListsMaster where  lm_lt_ListTypeId=@lm_lt_ListTypeId and lm_Deleted is null order by lm_Ordinal";
        mycon.sqlCmd_.Parameters.AddWithValue("@lm_lt_ListTypeId", listtypeid);

        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        List<ListMaster> lmvalues = new List<ListMaster>();
        ListMaster lm;

        while (sDr.Read())
        {
            lm = new ListMaster();
            lm.lm_Id = Convert.ToInt32(sDr["lm_Id"]);
            lm.lm_Value = Convert.ToString(sDr["lm_value"]);
            lmvalues.Add(lm);

        }
        mycon.sqlConClose();
        return lmvalues;
    }

    public int OrderCreate(int CompanyID, int ContactID, string OrderType, int PackageID) {

        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "INSERT INTO [dbo].[Order_T]([CompanyID],[ContactID],[OrderDate],[OrderType],[PackageID]) VALUES(@CompanyID, @ContactID, @OrderDate,@OrderType,@PackageID);select @@identity";
        mycon.sqlCmd_.Parameters.AddWithValue("@CompanyID", CompanyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ContactID", ContactID);
        mycon.sqlCmd_.Parameters.AddWithValue("@OrderDate", DateTime.Now);
        mycon.sqlCmd_.Parameters.AddWithValue("@OrderType", OrderType);
        mycon.sqlCmd_.Parameters.AddWithValue("@PackageID", PackageID);


        mycon.sqlConOpen();
        int invoiceno = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        return invoiceno;
    }


    public int CS_AssignSubscriptiontoCompany(int CompanyID, int ContactID, int PackageID)
    {
        DateTime startdate = DateTime.Now;
        DateTime enddate = DateTime.Now.AddYears(1);

        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = @"update [dbo].[CSSubscription_T] set EndDate= DATEADD(dd,-1,GETDATE()) where CompanyID=@CompanyID and enddate>getdate();
        INSERT INTO [dbo].[CSSubscription_T]([CompanyID],[ContactID],[PackageID],[StartDate],[EndDate],[DOC]) VALUES(@CompanyID, @ContactID, @PackageID, @StartDate, @EndDate, @DOC);select @@identity";
        mycon.sqlCmd_.Parameters.AddWithValue("@CompanyID", CompanyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@ContactID", ContactID);
        mycon.sqlCmd_.Parameters.AddWithValue("@PackageID", PackageID);
        mycon.sqlCmd_.Parameters.AddWithValue("@StartDate", startdate);
        mycon.sqlCmd_.Parameters.AddWithValue("@EndDate", enddate);
        mycon.sqlCmd_.Parameters.AddWithValue("@DOC", DateTime.Now);
        mycon.sqlConOpen();
        int subscriptionid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();


        mycon = new SqlCon();
        SqlDataReader sDr;
        mycon.sqlCmd_.CommandText = "select * from [dbo].[CSCredits_T] where packageid=@PackageID";
        mycon.sqlCmd_.Parameters.AddWithValue("@PackageID", PackageID);

        mycon.sqlConOpen();
        sDr = mycon.sqlCmd().ExecuteReader();
        List<CSCredits> CreditTypes = new List<CSCredits>();
        CSCredits cm;

        while (sDr.Read())
        {
            cm = new CSCredits();
            cm.CreditTypeID = Convert.ToInt32(sDr["CreditTypeID"]);
            cm.CreditsPerMonth = Convert.ToInt32(sDr["CreditsPerMonth"]);
            CreditTypes.Add(cm);

        }
        mycon.sqlConClose();

        string xml4posting = @"<root>";
        DateTime voucherdate = DateTime.Now;
        foreach (var ct in CreditTypes)
        {
            while (voucherdate.Date <= enddate.Date)
            {
                xml4posting += "<row><subscriptionid>" + subscriptionid + "</subscriptionid><vouchermonth>" + voucherdate.Month + "</vouchermonth><voucheryear>" + voucherdate.Year + "</voucheryear><credittypeid>" + ct.CreditTypeID + "</credittypeid><creditbalance>" + ct.CreditsPerMonth + "</creditbalance></row>";
                voucherdate = voucherdate.AddMonths(1);
            }
            voucherdate = DateTime.Now;
        }
        xml4posting += "</root>";


        mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "CreditSystemCreateSubscription";
        mycon.sqlCmd_.CommandType = CommandType.StoredProcedure;
        mycon.sqlCmd_.Parameters.AddWithValue("@xml", xml4posting);
        mycon.sqlConOpen();
        mycon.sqlCmd().ExecuteNonQuery();
        mycon.sqlConClose();

        return subscriptionid;
    }


    public bool CS_CreditCheck(int CompanyID, int CreditTypeID)
    {
        int VoucherID = 0;
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select TOP 1 VoucherID from CSVouchers_T where SubscriptionID=(select top 1 SubscriptionID from  CSSubscription_T where CompanyID=@CompanyID and EndDate>=GETDATE() order by subscriptionid desc) and CreditTypeID=@CreditType and VoucherMonth= MONTH(GETDATE()) and VoucherYear= YEAR(GETDATE()) and CreditBalance>0 ORDER BY VoucherID desc";
        mycon.sqlCmd_.Parameters.AddWithValue("@CompanyID", CompanyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@CreditType", CreditTypeID);
        mycon.sqlConOpen();
        VoucherID = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();
        if (VoucherID != 0)// checks if subscription exists and not expired
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CS_CreditUsage(int CompanyID, int ContactID, int CreditTypeID, int? VacancyID, int? CandidateID)
    {
        int VoucherID = 0;
        SqlCon mycon = new SqlCon();
        mycon.sqlCmd_.CommandText = "select TOP 1 VoucherID from CSVouchers_T where SubscriptionID=(select top 1 SubscriptionID from  CSSubscription_T where CompanyID=@CompanyID and EndDate>=GETDATE()  order by subscriptionid desc) and CreditTypeID=@CreditType and VoucherMonth= MONTH(GETDATE()) and VoucherYear= YEAR(GETDATE()) and CreditBalance>0 ORDER BY VoucherID desc";
        mycon.sqlCmd_.Parameters.AddWithValue("@CompanyID", CompanyID);
        mycon.sqlCmd_.Parameters.AddWithValue("@CreditType", CreditTypeID);
        mycon.sqlConOpen();
        VoucherID = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
        mycon.sqlConClose();

        if (VoucherID != 0)// checks if subscription exists and not expired
        {
            
            mycon = new SqlCon();
            mycon.sqlCmd_.CommandText = @"INSERT INTO [dbo].[CSCreditUsage_T]([VoucherID],[CompanyID],[ContactID],[UsedOn],[CreditTypeID],[VacancyID],[CandidateID]) VALUES(@VoucherID, @CompanyID, @ContactID, @UsedOn, @CreditTypeID,@VacancyID,@CandidateID);UPDATE CSVouchers_T set CreditBalance= CreditBalance-1 where VoucherID=@VoucherID";
            mycon.sqlCmd_.Parameters.AddWithValue("@CompanyID", CompanyID);
            mycon.sqlCmd_.Parameters.AddWithValue("@ContactID", ContactID);
            mycon.sqlCmd_.Parameters.AddWithValue("@UsedOn", DateTime.Now);
            mycon.sqlCmd_.Parameters.AddWithValue("@CreditTypeID", CreditTypeID);
            mycon.sqlCmd_.Parameters.AddWithValue("@VoucherID", VoucherID);


            if (VacancyID != null)
            {
                mycon.sqlCmd_.Parameters.AddWithValue("@VacancyID", VacancyID);

            }
            else
            {
                mycon.sqlCmd_.Parameters.AddWithValue("@VacancyID", DBNull.Value);

            }

            if (CandidateID != null)
            {
                mycon.sqlCmd_.Parameters.AddWithValue("@CandidateID", CandidateID);
            }
            else
            {
                mycon.sqlCmd_.Parameters.AddWithValue("@CandidateID", DBNull.Value);

            }

            mycon.sqlConOpen();
            int subscriptionid = Convert.ToInt32(mycon.sqlCmd().ExecuteScalar());
            mycon.sqlConClose();
         
            
            return true;
        }
        else
        {
            return false;
        }
    }
}