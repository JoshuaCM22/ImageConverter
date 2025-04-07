using System;
using System.Configuration;

namespace ImageConverter
{
    public partial class PageNotFound : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public string GetRedirectUrl()
        {
            if (Request.Url.Host == "localhost") return ConfigurationManager.AppSettings["RedirectUrlLocal"];
            else return ConfigurationManager.AppSettings["RedirectUrlIIS"];
        }

    }
}