using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Data.SqlClient;

/// <summary>
/// Summary description for SqlCon
/// </summary>
public class SqlCon
{
    SqlConnection sqlCon_ = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["theConnectionString"].ConnectionString);

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
