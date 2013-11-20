using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    enum ServiceSolvedConfigStatusReason
    {
        /// <summary>
        /// Initialized by ServiceData constructor.
        /// </summary>
        Config,

        /// <summary>
        /// Sets by PluginData.CheckReferencesWhenMustExist method.
        /// </summary>
        FromMustExistReference,

        /// <summary>
        /// Sets by ServiceData.GetMustExistService method.
        /// </summary>
        FromGeneralization,

        /// <summary>
        /// Sets by ServiceData.RetrieveOrUpdateTheCommonServiceReferences.
        /// </summary>
        FromServiceConfigToCommonPluginReferences,

        /// <summary>
        /// Sets by ServiceData.RetrieveOrUpdateTheCommonServiceReferences.
        /// </summary>
        FromServiceToCommonPluginReferences,

        /// <summary>
        /// 
        /// </summary>
        FromMustExistSpecialization,
        FromMustExistGeneralization
    }
}
