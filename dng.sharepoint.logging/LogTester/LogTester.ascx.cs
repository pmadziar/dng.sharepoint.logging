using System;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using NLog;
using System.Linq;

namespace dng.sharepoint.logging.LogTester
{
    [ToolboxItemAttribute(false)]
    public partial class LogTester : WebPart
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public LogTester()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnDivide_Click(object sender, EventArgs e)
        {
            lblErr.Text = string.Empty;
            try
            {
                double num1 = double.Parse(tbNum01.Text);
                double num2 = double.Parse(tbNum02.Text);
                double num3 = num1 / num2;
                lblErr.Text = num3.ToString();
            }
            catch (Exception ex)
            {
                logException(ex);
                lblErr.Text = ex.Message;
            }
        }

        private static void logException(Exception ex, bool isInner = false, int level = 0)
        {
            string msg = string.Empty;
            if (isInner) msg += "InnerException ";
            if (level > 0) msg += string.Concat(Enumerable.Repeat("==", level)) + "> ";
            msg += ex.Message;
            logger.Error(msg);
            if (!string.IsNullOrEmpty(ex.StackTrace)) logger.Error(ex.StackTrace);
            if (ex.InnerException != null) logException(ex.InnerException, true, level + 1);
        }

        protected void btnError_Click(object sender, EventArgs e)
        {
            logger.Error("Error button clicked");
        }

        protected void btnDebug_Click(object sender, EventArgs e)
        {
            logger.Debug("Debug button clicked");
        }
    }
}
