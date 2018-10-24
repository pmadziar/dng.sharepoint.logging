using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using System.IO;

namespace dng.sharepoint.logging.Features.dng.sharepoint.logging
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("5df1a15c-6aad-4acd-aa85-815c594dc599")]
    public class dngsharepointEventReceiver : SPFeatureReceiver
    {
        const string NLogConfig = "NLog.config";
        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPWebApplication webApp = properties.Feature.Parent as SPWebApplication;
            if (webApp == null)
            {
                throw new SPException("dngsharepointEventReceiver.FeatureActivated: WebApplication is null");
            }

            try
            {

                string webAppFolderPath = webApp.IisSettings[SPUrlZone.Default].Path.FullName;
                if (Directory.Exists(webAppFolderPath))
                {
                    string nlogConfigPath = Path.Combine(webAppFolderPath, NLogConfig);
                    if (File.Exists(nlogConfigPath)) File.Delete(nlogConfigPath);
                    string resourceName = this.GetType().Assembly.GetName().Name + "." + NLogConfig;

                    byte[] bytes;

                    var names = this.GetType().Assembly.GetManifestResourceNames().ToList();


                    using (Stream resFilestream = this.GetType().Assembly.GetManifestResourceStream(resourceName))
                    {
                        bytes = new byte[resFilestream.Length];
                        resFilestream.Read(bytes, 0, bytes.Length);
                    }

                    if (bytes.Length > 0)
                    {
                        File.WriteAllBytes(nlogConfigPath, bytes);
                    }
                    else
                    {
                        throw new FileLoadException("dngsharepointEventReceiver.FeatureActivated: Resource file is empty");
                    }
                }
                else
                {
                    throw new DirectoryNotFoundException(webAppFolderPath);
                }
            }
            catch (Exception ex)
            {
                throw new SPException(ex.Message, ex);
            }
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        {
            SPWebApplication webApp = properties.Feature.Parent as SPWebApplication;
            if (webApp == null)
            {
                throw new SPException("dngsharepointEventReceiver.FeatureActivated: WebApplication is null");
            }

            try
            {

                string webAppFolderPath = webApp.IisSettings[SPUrlZone.Default].Path.FullName;
                if (Directory.Exists(webAppFolderPath))
                {
                    string nlogConfigPath = Path.Combine(webAppFolderPath, NLogConfig);
                    if (File.Exists(nlogConfigPath)) File.Delete(nlogConfigPath);
                }
            }
            catch (Exception ex)
            {
                throw new SPException(ex.Message, ex);
            }

        }


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
