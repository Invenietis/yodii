﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
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
        ///   Looks up a localized string similar to Calling BindToStartingPlugin is possible only when service is  Swapping..
        /// </summary>
        internal static string BindToStartingPluginMustBeSwapping {
            get {
                return ResourceManager.GetString("BindToStartingPluginMustBeSwapping", resourceCulture);
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
        ///   Looks up a localized string similar to Plugin &apos;{0}&apos; must be disabled but is already disabled..
        /// </summary>
        internal static string HostApplyPluginAlreadyDisabled {
            get {
                return ResourceManager.GetString("HostApplyPluginAlreadyDisabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Plugin &apos;{0}&apos; must start but is already started..
        /// </summary>
        internal static string HostApplyStartPluginAlreadyStarted {
            get {
                return ResourceManager.GetString("HostApplyStartPluginAlreadyStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Plugin &apos;{0}&apos; must stop but is currently disabled..
        /// </summary>
        internal static string HostApplyStopPluginAlreadyDisabled {
            get {
                return ResourceManager.GetString("HostApplyStopPluginAlreadyDisabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Plugin &apos;{0}&apos; must stop but is already stopped..
        /// </summary>
        internal static string HostApplyStopPluginAlreadyStopped {
            get {
                return ResourceManager.GetString("HostApplyStopPluginAlreadyStopped", resourceCulture);
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
