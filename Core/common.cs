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
using System.Net.Mail;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;

/// <summary>
/// Summary description for common
/// </summary>
public class common
{
	public common()
	{
		//
		// TODO: Add constructor logic here
		//
	}

    public string canprofilepic(Guid guidx, string photoextn)
    {
        string guid = Convert.ToString(guidx);
        return "Documents/photos/" + guid.Substring(0, 2) + "/" + guid.Substring(2, 2) + "/" + guid.Substring(4, 2) + "/" + guid + "." + photoextn;
    }



    public string GetMonthName(int month)
    {

        DateTime date = new DateTime(2018, month, 1);
        return date.ToString("MMMM");

    }
    public void SendMail(string To, string Subject, string emailbody)
    {
        Execute(Subject, emailbody, To).Wait(50);
    }

    static async Task Execute(string subject, string htmlcontent2, string allemails)
    {
        var client = new SendGridClient("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");
        var from = new EmailAddress("info@jobs4bahrainis.com", "Jobs4Bahrainis");
        var to = new EmailAddress(allemails);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, "", htmlcontent2);
        var response = await client.SendEmailAsync(msg);
    }


    public string StripHTMLExceptsometags(string sText)
    {

        sText = Regex.Replace(sText, "style=(\"|')[^(\"|')]*(\"|')", "");
        sText = sText.Replace(Environment.NewLine, "<br />");

        // font div br tags are only removed

        return Regex.Replace(sText, "</?(font|div|br|\t|\n|\r)[^>]*>", String.Empty, RegexOptions.IgnoreCase);
    }


    public string urlcleaner(string originalstr)
    {
        string cleanstr = string.Empty;
        if (originalstr != null)
        {
            cleanstr = originalstr.Replace(" ", "-").Replace(".", "").Replace("?", "").Replace("&", "").Replace("'", "").Replace("!", "").Replace(":", "").Replace("%", "").Replace(";", "").Replace("*", "").Replace("\"", "");
        }

        return cleanstr;
    }


    public string ConvertNumbertoWords(int number)
    {
        if (number == 0)
            return "ZERO";
        if (number < 0)
            return "minus " + ConvertNumbertoWords(Math.Abs(number));
        string words = "";

        if ((number / 1000000000) > 0)
        {
            words += ConvertNumbertoWords(number / 1000000000) + " Billion ";
            number %= 1000000000;
        }

        if ((number / 10000000) > 0)
        {
            words += ConvertNumbertoWords(number / 10000000) + " Crore ";
            number %= 10000000;
        }

        if ((number / 1000000) > 0)
        {
            words += ConvertNumbertoWords(number / 1000000) + " MILLION ";
            number %= 1000000;
        }
        if ((number / 1000) > 0)
        {
            words += ConvertNumbertoWords(number / 1000) + " THOUSAND ";
            number %= 1000;
        }
        if ((number / 100) > 0)
        {
            words += ConvertNumbertoWords(number / 100) + " HUNDRED ";
            number %= 100;
        }
        if (number > 0)
        {
            if (words != "")
                words += "AND ";
            var unitsMap = new[] { "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE", "TEN", "ELEVEN", "TWELVE", "THIRTEEN", "FOURTEEN", "FIFTEEN", "SIXTEEN", "SEVENTEEN", "EIGHTEEN", "NINETEEN" };
            var tensMap = new[] { "ZERO", "TEN", "TWENTY", "THIRTY", "FORTY", "FIFTY", "SIXTY", "SEVENTY", "EIGHTY", "NINETY" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += " " + unitsMap[number % 10];
            }
        }
        return words;
    }
}
