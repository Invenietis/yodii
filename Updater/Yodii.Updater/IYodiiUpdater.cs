using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Updater
{
    /// <summary>
    /// Yodii updater, allowing runtime installation of plugins and services through packages.
    /// </summary>
    public interface IYodiiUpdater : IYodiiService
    {
        /// <summary>
        /// Starts a task to install a package.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="progressReporter">The progress reporter to use when reporting progress.</param>
        /// <param name="cancellationToken">The cancellation token listened to for cancellation.</param>
        /// <returns>An asynchronous IUpdaterTaskResult when the task is complete.</returns>
        Task<IUpdaterTaskResult> InstallPackage( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken );

        /// <summary>
        /// Starts a task to uninstall a package.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="progressReporter">The progress reporter to use when reporting progress.</param>
        /// <param name="cancellationToken">The cancellation token listened to for cancellation.</param>
        /// <returns>An asynchronous IUpdaterTaskResult when the task is complete.</returns>
        Task<IUpdaterTaskResult> UninstallPackage( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken );

        /// <summary>
        /// Starts a task to update a package.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <param name="progressReporter">The progress reporter to use when reporting progress.</param>
        /// <param name="cancellationToken">The cancellation token listened to for cancellation.</param>
        /// <returns>An asynchronous IUpdaterTaskResult when the task is complete.</returns>
        Task<IUpdaterTaskResult> UpdatePackage( string packageName, IProgress<IUpdaterTaskProgress> progressReporter, CancellationToken cancellationToken );

        /// <summary>
        /// Determines whether the specified package name is installed.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <returns>True if the specified package name is installed; otherwise false.</returns>
        bool IsPackageInstalled( string packageName );
    }
}
