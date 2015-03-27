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
    /// It should open when item starts, and close when item stops.
    /// </summary>
    public interface IObjectExplorerService : IYodiiService
    {
    }
}
