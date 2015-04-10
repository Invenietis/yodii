using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Engine.Tests;
using Yodii.Model;
using Yodii.Updater.Impl;
using CK.Core;
using System.Xml.Linq;
using NuGet;
using NuGet.Runtime;

namespace Yodii.Updater.Tests
{
    [TestFixture]
    public class UpdaterTests
    {
        YodiiRuntimeTestContext _ctx;
        IYodiiUpdater _updater;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            ClearTestDataDirectory();
            SetTestDataDirectoryInAppSettings();
            SetTestPackageSourceInAppSettings();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            ClearAppSettings();
            ClearTestDataDirectory();
        }

        [SetUp]
        public void SetUp()
        {
            _ctx = new YodiiRuntimeTestContext( TestHelper.GetDiscoveredInfoFromAppDomainAssemblies() )
                .StartService<IYodiiUpdater>();

            _updater = _ctx.InteractWithServiceDirectly<IYodiiUpdater>();
        }

        [TearDown]
        public void TearDown()
        {
            _updater = null;
            _ctx.Dispose();
            ClearTestDataDirectory();
        }

        [Test]
        public async void Updater_plugin_can_install_and_remove_package()
        {
            string packageName = "NUnit";

            IYodiiUpdater p = _updater;

            var result = await p.InstallPackage( packageName, new Progress<IUpdaterTaskProgress>(), CancellationToken.None );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.Status, Is.EqualTo( UpdaterTaskStatus.Complete ) );

            Assert.That( p.IsPackageInstalled( packageName ), Is.True );

            result = await p.UninstallPackage( packageName, new Progress<IUpdaterTaskProgress>(), CancellationToken.None );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.Status, Is.EqualTo( UpdaterTaskStatus.Complete ) );

            Assert.That( p.IsPackageInstalled( packageName ), Is.False );
        }

        [Test]
        public void Updater_plugin_can_start()
        {
            Assert.That( _updater, Is.Not.Null );

            Assert.That( _updater, Is.InstanceOf<YodiiUpdaterPlugin>() );
        }

        [Test]
        public void FrameworkName_CanBeGuessed_FromAppDomainSetup()
        {
            Assert.That( AppDomain.CurrentDomain.SetupInformation, Is.Not.Null );
            Assert.That( AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName, Is.Not.Null.Or.Empty );

            FrameworkName name = GetAppDomainFramework();

            TestHelper.ConsoleMonitor.Info().Send( "AppDomain framework name: {0}", name );

        }

        [Test]
        public void ConfigurationFile_CanBeGuessed_FromAppDomainSetup()
        {
            Assert.That( AppDomain.CurrentDomain.SetupInformation, Is.Not.Null );
            Assert.That( AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, Is.Not.Null.Or.Empty );

            TestHelper.ConsoleMonitor.Info().Send( "AppDomain ConfigurationFile {0}", AppDomain.CurrentDomain.SetupInformation.ConfigurationFile );
            XDocument d = XDocument.Load( AppDomain.CurrentDomain.SetupInformation.ConfigurationFile );
        }

        static FrameworkName GetAppDomainFramework()
        {
            return new FrameworkName( AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName );
        }

        [Test]
        public void NuGet_Core_CanUse_ConfigurationFile_For_BindingRedirects()
        {
            Assume.That( AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, Is.Not.Null.Or.Empty );

            AppDomainSetup appSetup = new AppDomainSetup()
            {
                ApplicationName = "TestDomain",
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
            };

            AppDomain appDomain = AppDomain.CreateDomain( "TestDomain", null, appSetup );

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where( a => !a.IsDynamic )
                .Select( a => a.Location );

            var redirects = BindingRedirectResolver.GetBindingRedirects( assemblies, appDomain );

            BindingRedirectManager brm = new BindingRedirectManager(
                new PhysicalFileSystem( GetTestDataDirectory() ),
                AppDomain.CurrentDomain.SetupInformation.ConfigurationFile
                );

            brm.AddBindingRedirects( redirects );


        }

        void SetTestDataDirectoryInAppSettings()
        {
            string path = GetTestDataDirectory();
            var settings = GetSettings();


            if( settings[YodiiUpdaterPlugin.AppSettingsMainDirectoryKey] == null )
            {
                settings.Add( YodiiUpdaterPlugin.AppSettingsMainDirectoryKey, path );
            }
            else
            {
                settings[YodiiUpdaterPlugin.AppSettingsMainDirectoryKey].Value = path;
            }
            SaveSettings();
        }

        void SetTestPackageSourceInAppSettings()
        {
            string path = GetSolutionPackagesDirectory();
            var settings = GetSettings();


            if( settings[YodiiUpdaterPlugin.AppSettingsPackageSourceKey] == null )
            {
                settings.Add( YodiiUpdaterPlugin.AppSettingsPackageSourceKey, path );
            }
            else
            {
                settings[YodiiUpdaterPlugin.AppSettingsPackageSourceKey].Value = path;
            }
            SaveSettings();
        }

        void ClearAppSettings()
        {
            var settings = GetSettings();

            settings.Remove( YodiiUpdaterPlugin.AppSettingsMainDirectoryKey );
            settings.Remove( YodiiUpdaterPlugin.AppSettingsPackageSourceKey );

            SaveSettings();
        }

        void ClearTestDataDirectory()
        {
            string d = GetTestDataDirectory();
            if( Directory.Exists( d ) )
            {
                Directory.Delete( d, true );
            }
        }

        Configuration _currentConfigFile;
        KeyValueConfigurationCollection GetSettings()
        {
            if( _currentConfigFile == null )
            {
                _currentConfigFile = ConfigurationManager.OpenExeConfiguration( ConfigurationUserLevel.None );
            }

            return _currentConfigFile.AppSettings.Settings;
        }

        void SaveSettings()
        {
            if( _currentConfigFile != null )
            {
                _currentConfigFile.Save( ConfigurationSaveMode.Modified );
                ConfigurationManager.RefreshSection( _currentConfigFile.AppSettings.SectionInformation.Name );
            }
        }

        string GetTestDataDirectory()
        {
            return Path.Combine( Assembly.GetExecutingAssembly().GetAssemblyDirectory(), "TestDir" );
        }

        string GetSolutionPackagesDirectory()
        {
            string path = Path.Combine( Assembly.GetExecutingAssembly().GetAssemblyDirectory(), "..", "..", "..", "..", "packages" );

            path = Path.GetFullPath( path );

            if( !Directory.Exists( path ) ) throw new DirectoryNotFoundException( "Couldn't find solution's packages directory" );

            return path;
        }
    }
}
