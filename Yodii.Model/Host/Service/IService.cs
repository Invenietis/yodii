using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IService<T> where T : IYodiiService
    {
        T Service { get; }
        
        RunningStatus RunningStatus { get; }
    }
}
