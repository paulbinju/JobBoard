using System.Data.SqlClient;

/// <summary>
/// Summary description for SqlCon
/// </summary>
public class SqlConDirDev
{
    SqlConnection sqlCon_ = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["directorydev"].ConnectionString);

    public SqlCommand sqlCmd_ = new SqlCommand();

    public void sqlConOpen()
    {
        SqlConnection.ClearAllPools();
        sqlCon_.Open();
    }

    public SqlCommand sqlCmd()
    {
        sqlCmd_.Connection = sqlCon_;
        return sqlCmd_;
    }


    public void sqlConClose()
    {
        sqlCon_.Close();
        sqlCon_.Dispose();
    }
}
