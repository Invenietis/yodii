using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Engine.Tests;
using Yodii.Model;
using Yodii.Updater.Impl;

namespace Yodii.Updater.Tests
{
    [TestFixture]
    public class UpdaterTests
    {
        [Test]
        public async void Updater_plugin_can_install_CK_Core()
        {
            SetTestDirectoryInAppSettings();

            IYodiiUpdater p = new Updater.Impl.YodiiUpdaterPlugin();

            var result = await p.InstallPackage( "CK.Core", new Progress<IUpdaterTaskProgress>(), CancellationToken.None );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.Status, Is.EqualTo( UpdaterTaskStatus.Complete ) );
        }

        [Test]
        public void Updater_plugin_can_start()
        {
            using( var ctx = new YodiiRuntimeTestContext().StartService<IYodiiUpdater>() )
            {
                ILiveServiceInfo service = ctx.FindLiveService<IYodiiUpdater>();

                Assert.That( service, Is.Not.Null, "IYodiiUpdater service could be found" );
                Assert.That( service.IsRunning, Is.True, "IYodiiUpdater service is started" );
            }
        }

        void SetTestDirectoryInAppSettings()
        {
            string path = Path.Combine( Assembly.GetExecutingAssembly().GetAssemblyDirectory(), "TestDir" );

            var configFile = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
            var settings = configFile.AppSettings.Settings;

            if( settings[YodiiUpdaterPlugin.AppSettingsMainDirectoryKey] == null )
            {
                settings.Add( YodiiUpdaterPlugin.AppSettingsMainDirectoryKey, path );
            }
            else
            {
                settings[YodiiUpdaterPlugin.AppSettingsMainDirectoryKey].Value = path;
            }

            configFile.Save( ConfigurationSaveMode.Modified );
            ConfigurationManager.RefreshSection( configFile.AppSettings.SectionInformation.Name );


        }
    }
}
