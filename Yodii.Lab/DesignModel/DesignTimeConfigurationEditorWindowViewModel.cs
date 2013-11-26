using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Lab.ConfigurationEditor;

namespace Yodii.Lab.DesignModel
{
    internal class DesignTimeConfigurationEditorWindowViewModel
        : ConfigurationEditorWindowViewModel
    {
        public DesignTimeConfigurationEditorWindowViewModel() : base(null, new DesignTimeConfigurationManager(), new DesignTimeServiceInfoManager())
        {

        }
    }
}
