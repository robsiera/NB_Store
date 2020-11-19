using System.IO;
using System.Net;
using System.Xml;
using NEvoWeb.Modules.NB_Store;

namespace Nevoweb.NBrightStore.Gateway
{
    public class Utils
    {
        public static string PostSofortXmlData(int portalID,string xmlData)
        {
            string gatewayParams = SharedFunctions.GetStoreSetting(portalID, "sofortxml.gateway", "None");
            if (!string.IsNullOrEmpty(gatewayParams))
            {
                // get the gateway into a xml doc.
                var xmlDoc = new XmlDataDocument();
                xmlDoc.LoadXml(gatewayParams);

                var sofortUri = xmlDoc.SelectSingleNode("multipay/nbs/soforturi").InnerText;
                var apiKey = xmlDoc.SelectSingleNode("multipay/nbs/apikey").InnerText;
                var customernumber = xmlDoc.SelectSingleNode("multipay/nbs/customernumber").InnerText;

                var rtnXml = Utils.WebRequestPostData(sofortUri, xmlData, customernumber, apiKey);

                return rtnXml;
            }
            return "";
        }

        public static string WebRequestPostData(string uri, string postData, string username, string apiKey)
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(uri);

            req.Credentials = new NetworkCredential(username, apiKey);
            req.ContentType = "application/xml";
            req.Method = "POST";

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(postData);
            req.ContentLength = bytes.Length;

            using (Stream os = req.GetRequestStream())
            {
                os.Write(bytes, 0, bytes.Length);
            }

            using (System.Net.WebResponse resp = req.GetResponse())
            {
                if (resp == null) return null;

                using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream()))
                {
                    return sr.ReadToEnd().Trim();
                }
            }
        }
    }
}
