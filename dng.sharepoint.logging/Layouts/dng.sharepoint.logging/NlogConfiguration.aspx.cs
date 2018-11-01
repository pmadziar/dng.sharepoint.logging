using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Web.UI;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web;
using System.Xml;
using System.Web.UI.WebControls;
using Microsoft.SharePoint.Administration;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Reflection;

namespace dng.sharepoint.logging.Layouts.dng.sharepoint.logging
{
    public partial class NlogConfiguration : LayoutsPageBase
    {

        public class ValidationInfo
        {
            public bool Valid { get; set; }
            public string Message { get; set; }
        }

        public const string NLogConfig = "NLog.config";
        public const string NLogXsd = "NLog.xsd";

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            ScriptManager scm = ScriptManager.GetCurrent(this.Page);
            if (scm == null)
            {
                throw new ArgumentNullException("ScriptManager is not available");
            }
            scm.EnablePageMethods = true;
            lblMessage.Text = getScriptControlIds().TrimEnd(", \r\n".ToCharArray());
            liGeneratedScript.Mode = LiteralMode.PassThrough;
            close.Click += close_Click;
        }

        void close_Click(object sender, EventArgs e)
        {
            Page.Response.Redirect(SPContext.Current.Web.Url.TrimEnd("/".ToCharArray()) + "/_layouts/15/settings.aspx", true);
        }

        private static string getNlogConfigContent()
        {
            string webAppFolderPath = SPContext.Current.Site.WebApplication.IisSettings[SPUrlZone.Default].Path.FullName;
            string nlogConfigPath = Path.Combine(webAppFolderPath, NLogConfig);
            return File.ReadAllText(nlogConfigPath);
        }

        private static void saveNlogConfigContent(string xmlstr)
        {
            string webAppFolderPath = SPContext.Current.Site.WebApplication.IisSettings[SPUrlZone.Default].Path.FullName;
            string nlogConfigPath = Path.Combine(webAppFolderPath, NLogConfig);
            SPSecurity.RunWithElevatedPrivileges(delegate()
            {
                File.WriteAllText(nlogConfigPath, xmlstr, Encoding.UTF8);
            });
        }


        private string getScriptControlIds()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<script type=""text/javascript"">");
            sb.AppendLine(@"var controlIds = {");
            appendControlIdRecurs(sb, this);
            sb.AppendLine(@"};");
            //sb.Append("var nlogJsonSchema = ");
            //sb.AppendLine(getNlogSchemaJson());
            sb.AppendLine(@"</script>");
            return sb.ToString();
        }

        private void appendControlIdRecurs(StringBuilder sb, Control cnt)
        {
            try
            {
                string id = cnt.ID;
                string clientId = cnt.ClientID;
                if (string.IsNullOrWhiteSpace(id))
                {
                    id = clientId;
                }
                if (!string.IsNullOrWhiteSpace(id)) sb.AppendLine(string.Format(@"{0}: ""{1}"",", id, clientId));
            }
            catch { }
            if (cnt.Controls != null && cnt.Controls.Count > 0)
                foreach (Control child in cnt.Controls) appendControlIdRecurs(sb, child);
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = false)]
        public static ValidationInfo SaveConfig(string input)
        {
            bool isValid = true;
            string errmsg = null;

            string xmlstr = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(input));
            string retXmlStr = xmlstr;
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.LoadXml(xmlstr);
                byte[] xsdBytes = loadXsdBytes();
                MemoryStream ms = new MemoryStream(xsdBytes);
                XmlSchema schema = XmlSchema.Read(ms, null);
                xdoc.Schemas.Add(schema);
                xdoc.Validate(DocumentValidationHandler);
            }
            catch (Exception ex)
            {
                isValid = false;
                errmsg = ex.Message;
            }

            if (isValid)
            {
                try
                {
                    saveNlogConfigContent(xmlstr);
                }
                catch (Exception ex)
                {
                    isValid = false;
                    errmsg = "Error saving file: " + ex.Message;
                }
            }

            return new ValidationInfo()
            {
                Valid = isValid,
                Message = errmsg
            };
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = false)]
        public static ValidationInfo GetConfig()
        {
            bool isValid = true;
            string msg = null;
            try
            {
                string configContent = getNlogConfigContent();
                msg = Convert.ToBase64String(Encoding.UTF8.GetBytes(configContent));
            }
            catch (Exception ex)
            {
                isValid = false;
                msg = ex.Message;
            }

            return new ValidationInfo()
            {
                Valid = isValid,
                Message = msg
            };
        }

        private static void SchemaValidationHandler(object sender, ValidationEventArgs e)
        {
            throw new ApplicationException("Error loadig XSD: " + e.Message);
        }

        private static void DocumentValidationHandler(object sender, ValidationEventArgs e)
        {
            throw new ApplicationException("Error validating config: " + e.Message);
        }

        private static string getNlogSchemaJson()
        {
            Assembly asm = (typeof(NlogConfiguration)).Assembly;
            string resourceName = asm.GetName().Name + ".Resources.NlogConfig.json";
            string[] xxx = asm.GetManifestResourceNames();
            byte[] bytes;

            using (Stream resFilestream = asm.GetManifestResourceStream(resourceName))
            {
                bytes = new byte[resFilestream.Length];
                resFilestream.Read(bytes, 0, bytes.Length);
            }
            string ret = Encoding.UTF8.GetString(bytes);
            return ret;
        }

        private static byte[] loadXsdBytes()
        {
            Assembly asm = (typeof (NlogConfiguration)).Assembly;
            string resourceName = asm.GetName().Name + "." + NLogXsd;
            string[] xxx = asm.GetManifestResourceNames();
            byte[] bytes;

            using (Stream resFilestream = asm.GetManifestResourceStream(resourceName))
            {
                bytes = new byte[resFilestream.Length];
                resFilestream.Read(bytes, 0, bytes.Length);
            }

            return bytes;
        }
    }
}
