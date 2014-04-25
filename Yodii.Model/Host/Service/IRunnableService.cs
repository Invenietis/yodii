using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IRunnableService<T> : IService<T> where T : IYodiiService
    {

    }
}
