using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.ConfigurationManager
{
    public interface IConfigurationManagerService : IYodiiService
    {
        void ActivateWindow();
    }
}
