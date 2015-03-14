using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.ObjectExplorer
{
    /// <summary>
    /// Object Explorer service interface marker.
    /// It should open when plugin starts, and close when plugin stops.
    /// </summary>
    public interface IObjectExplorerService : IYodiiService
    {
    }
}
