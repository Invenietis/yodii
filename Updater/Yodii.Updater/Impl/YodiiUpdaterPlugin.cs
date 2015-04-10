using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CK.Core;
using NuGet;
using Yodii.Model;

namespace Yodii.Updater.Impl
{
    public class YodiiUpdaterPlugin : YodiiPluginBase, IYodiiUpdater
    {
        public static readonly string AppSettingsMainDirectoryKey = @"YodiiUpdater:MainDirectory";
        public static readonly string AppSettingsPackageSourceKey = @"YodiiUpdater:PackageSource";

        public static readonly string FallbackPackageSource = @"https://www.nuget.org/api/v2/";

        readonly string _mainDirectoryPath;

        PackageManager _packageManager;

        public YodiiUpdaterPlugin()
        {
            _mainDirectoryPath = GetMainDirectoryPath();

            IPackageRepository sourceRepo = PackageRepositoryFactory.Default.CreateRepository( GetPackageSource().Source );

            _packageManager = new PackageManager( sourceRepo, _mainDirectoryPath );
        }

        string GetMainDirectoryPath()
        {
            string path = AppSettings.Default.Get<string>( AppSettingsMainDirectoryKey, String.Empty );

            if( String.IsNullOrWhiteSpace( path ) )
            {
                // Use %APPDATA% with a default path corresponding
                // to the executing assembly qualified name
                string basePath = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );

                Assembly entryAssembly = Assembly.GetEntryAssembly();
                if( entryAssembly == null ) { throw new InvalidOperationException( String.Format( "No entry assembly was found, and {0} AppSettings was not set. Cannot guess local package repository location.", AppSettingsMainDirectoryKey ) ); }

                path = Path.Combine( basePath, entryAssembly.FullName );
            }
            else
            {
                path = Environment.ExpandEnvironmentVariables( path );
            }

            return path;
        }

        PackageSource GetPackageSource()
        {
            PackageSource packageSource;

            string packageSourceStr = AppSettings.Default.Get<string>( AppSettingsPackageSourceKey, String.Empty );

            if( String.IsNullOrWhiteSpace( packageSourceStr ) )
            {
                packageSource = NuGet.ConfigurationDefaults.Instance.DefaultPackageSources.FirstOrDefault();
                if( packageSource == null ) { packageSource = new PackageSource( FallbackPackageSource ); }
            }
            else
            {
                packageSource = new PackageSource( packageSourceStr );
            }

            return packageSource;
        }


        class InternalTask
        {
            internal CancellationTokenSource CancellationTokenSource;
            internal Task Task;
        }

        public Task<IUpdaterTaskResult> InstallPackage( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken )
        {
            if( packageName == null ) { throw new ArgumentNullException( "packageName" ); }
            if( progressReporter == null ) { throw new ArgumentNullException( "progressReporter" ); }
            if( cancellationToken == null ) { throw new ArgumentNullException( "cancellationToken" ); }

            return Task.Run<IUpdaterTaskResult>( () => InstallPackageSynchronous( packageName, progressReporter, cancellationToken ) );
        }

        IUpdaterTaskResult InstallPackageSynchronous( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken )
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                progressReporter.Report( new UpdaterTaskProgress( String.Format( "Installing {0}", packageName ), 0.0 ) );

                _packageManager.InstallPackage( packageName );

                progressReporter.Report( new UpdaterTaskProgress( String.Format( "Installed {0}", packageName ), 1.0 ) );

                return new UpdaterTaskResult( UpdaterTaskStatus.Complete );
            }
            catch( TaskCanceledException )
            {
                return new UpdaterTaskResult( UpdaterTaskStatus.Canceled );
            }
            catch( Exception e )
            {
                return new UpdaterTaskResult( e );
            }
        }

        public Task<IUpdaterTaskResult> UninstallPackage( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken )
        {
            if( packageName == null ) { throw new ArgumentNullException( "packageName" ); }
            if( progressReporter == null ) { throw new ArgumentNullException( "progressReporter" ); }
            if( cancellationToken == null ) { throw new ArgumentNullException( "cancellationToken" ); }

            return Task.Run<IUpdaterTaskResult>( () => UninstallPackageSynchronous( packageName, progressReporter, cancellationToken ) );
        }

        IUpdaterTaskResult UninstallPackageSynchronous( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken )
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                progressReporter.Report( new UpdaterTaskProgress( String.Format( "Installing {0}", packageName ), 0.0 ) );

                _packageManager.UninstallPackage( packageName );

                progressReporter.Report( new UpdaterTaskProgress( String.Format( "Installed {0}", packageName ), 1.0 ) );

                

                return new UpdaterTaskResult( UpdaterTaskStatus.Complete );
            }
            catch( TaskCanceledException )
            {
                return new UpdaterTaskResult( UpdaterTaskStatus.Canceled );
            }
            catch( Exception e )
            {
                return new UpdaterTaskResult( e );
            }
        }

        public Task<IUpdaterTaskResult> UpdatePackage( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken )
        {
            if( packageName == null ) { throw new ArgumentNullException( "packageName" ); }
            if( progressReporter == null ) { throw new ArgumentNullException( "progressReporter" ); }
            if( cancellationToken == null ) { throw new ArgumentNullException( "cancellationToken" ); }

            return Task.Run<IUpdaterTaskResult>( () => UpdatePackageSynchronous( packageName, progressReporter, cancellationToken ) );
        }

        IUpdaterTaskResult UpdatePackageSynchronous( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken )
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                progressReporter.Report( new UpdaterTaskProgress( String.Format( "Installing {0}", packageName ), 0.0 ) );

                _packageManager.UpdatePackage( packageName, true, false );

                progressReporter.Report( new UpdaterTaskProgress( String.Format( "Installed {0}", packageName ), 1.0 ) );

                return new UpdaterTaskResult( UpdaterTaskStatus.Complete );
            }
            catch( TaskCanceledException )
            {
                return new UpdaterTaskResult( UpdaterTaskStatus.Canceled );
            }
            catch( Exception e )
            {
                return new UpdaterTaskResult( e );
            }
        }

        public bool IsPackageInstalled( string packageName )
        {
            if( packageName == null ) { throw new ArgumentNullException( "packageName" ); }


            return _packageManager.LocalRepository.FindPackage( packageName ) != null;
        }
    }
}
