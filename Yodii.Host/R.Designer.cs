﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Yodii.Host {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class R {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal R() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Yodii.Host.R", typeof(R).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling BindToSwappedPlugin is possible only when service is  Swapping..
        /// </summary>
        internal static string BindToSwappedPluginMustBeSwapping {
            get {
                return ResourceManager.GetString("BindToSwappedPluginMustBeSwapping", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling a Service method from a Plugin constructor is not allowed..
        /// </summary>
        internal static string CallingServiceFromCtor {
            get {
                return ResourceManager.GetString("CallingServiceFromCtor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling a Service method from a Dispose method is not allowed..
        /// </summary>
        internal static string CallingServiceFromDisable {
            get {
                return ResourceManager.GetString("CallingServiceFromDisable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling a Service method from a Plugin PreStart method is not allowed..
        /// </summary>
        internal static string CallingServiceFromPreStart {
            get {
                return ResourceManager.GetString("CallingServiceFromPreStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling a Service method from a Plugin PreStart rollback action is not allowed..
        /// </summary>
        internal static string CallingServiceFromPreStartRollbackAction {
            get {
                return ResourceManager.GetString("CallingServiceFromPreStartRollbackAction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling a Service method from a Stop method is not allowed..
        /// </summary>
        internal static string CallingServiceFromStop {
            get {
                return ResourceManager.GetString("CallingServiceFromStop", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IPreStopContext.Cancel can not be called since the engine is stopping..
        /// </summary>
        internal static string CannotCancelSinceEngineIsStopping {
            get {
                return ResourceManager.GetString("CannotCancelSinceEngineIsStopping", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find a constructor with [0}  parameters for plugin {1}..
        /// </summary>
        internal static string DefaultPluginCreatorUnableToFindCtor {
            get {
                return ResourceManager.GetString("DefaultPluginCreatorUnableToFindCtor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Some parameters for the plugin constructor could not be resolved. Ensure that all plugin constructor parameters are either Yodii services (interfaces implementing IYodiiService), IService&lt;T&gt; where T : IYodiiService or are types that have been injected (through PluginHost.InjectExternalService). If you need to inject other types through more standard dependency injection, you should set your own PluginCreator..
        /// </summary>
        internal static string DefaultPluginCreatorUnresolvedParams {
            get {
                return ResourceManager.GetString("DefaultPluginCreatorUnresolvedParams", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to An error occurred while creating a new plugin instance..
        /// </summary>
        internal static string ErrorWhileCreatingPluginInstance {
            get {
                return ResourceManager.GetString("ErrorWhileCreatingPluginInstance", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to External Service implementation must be provided..
        /// </summary>
        internal static string ExternalImplRequiredAsANonNullObject {
            get {
                return ResourceManager.GetString("ExternalImplRequiredAsANonNullObject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Missing Exception object in call to ExternalLogError..
        /// </summary>
        internal static string ExternalLogErrorMissException {
            get {
                return ResourceManager.GetString("ExternalLogErrorMissException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There must be no Stopped or Running plugins when stopping engine, ony disabled ones must exist..
        /// </summary>
        internal static string HostApplyHasStoppedOrRunningPluginWhileEngineIsStopping {
            get {
                return ResourceManager.GetString("HostApplyHasStoppedOrRunningPluginWhileEngineIsStopping", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid generalization mismatch: 2 plugins implementing the same Service must both start..
        /// </summary>
        internal static string HostApplyInvalidGeneralizationMismatchStart {
            get {
                return ResourceManager.GetString("HostApplyInvalidGeneralizationMismatchStart", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invalid generalization mismatch: 2 plugins implementing the same Service must both stop..
        /// </summary>
        internal static string HostApplyInvalidGeneralizationMismatchStopped {
            get {
                return ResourceManager.GetString("HostApplyInvalidGeneralizationMismatchStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Plugin &apos;{0}&apos; appears more than one in configuration list..
        /// </summary>
        internal static string HostApplyPluginMustBeInOneList {
            get {
                return ResourceManager.GetString("HostApplyPluginMustBeInOneList", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Host.Engine must be set before starting the Engine..
        /// </summary>
        internal static string HostEngineMustBeSetBeforeStartingTheEngine {
            get {
                return ResourceManager.GetString("HostEngineMustBeSetBeforeStartingTheEngine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Host.Engine must be set only once..
        /// </summary>
        internal static string HostEngineMustBeSetOnlyOnce {
            get {
                return ResourceManager.GetString("HostEngineMustBeSetOnlyOnce", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Interface must extend IYodiiService to be a Dynamic Service..
        /// </summary>
        internal static string InterfaceMustExtendIYodiiService {
            get {
                return ResourceManager.GetString("InterfaceMustExtendIYodiiService", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PluginRunner.PluginConfigurator must be not null..
        /// </summary>
        internal static string PluginConfiguratorIsNull {
            get {
                return ResourceManager.GetString("PluginConfiguratorIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PluginRunner.PluginCreator must not be null..
        /// </summary>
        internal static string PluginCreatorIsNull {
            get {
                return ResourceManager.GetString("PluginCreatorIsNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to PluginCreator returned a null plugin for &apos;{0}&apos;..
        /// </summary>
        internal static string PluginCreatorReturnedNull {
            get {
                return ResourceManager.GetString("PluginCreatorReturnedNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Previous plugin must not be null..
        /// </summary>
        internal static string PreviousPluginMustNotBeNull {
            get {
                return ResourceManager.GetString("PreviousPluginMustNotBeNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Service &apos;{0}&apos; is already bound to an external implementation. Plugin &apos;{1}&apos; can not offer it..
        /// </summary>
        internal static string ServiceIsAlreadyExternal {
            get {
                return ResourceManager.GetString("ServiceIsAlreadyExternal", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Service &apos;{0}&apos; is a dynamic service that is implemented by plugins. It can not be associated to an external implementation..
        /// </summary>
        internal static string ServiceIsPluginBased {
            get {
                return ResourceManager.GetString("ServiceIsPluginBased", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The type must be an Interface..
        /// </summary>
        internal static string TypeMustBeAnInterface {
            get {
                return ResourceManager.GetString("TypeMustBeAnInterface", resourceCulture);
            }
        }
    }
}
