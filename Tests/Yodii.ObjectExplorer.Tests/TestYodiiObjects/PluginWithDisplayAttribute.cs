using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Tests.TestYodiiObjects
{
    [YodiiPlugin( DisplayName = "Yodii item (with display attribute)", Description = "Some test item with a name and description." )]
    public class PluginWithDisplayAttribute : YodiiPluginBase, IMyYodiiService
    {
        public void HelloWorld()
        {
            throw new NotImplementedException();
        }
    }
}
