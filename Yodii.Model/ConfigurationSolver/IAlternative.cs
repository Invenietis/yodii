using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Model.ConfigurationSolver
{
    interface IAlternative
    {
        bool MoveNext();
    }
}
