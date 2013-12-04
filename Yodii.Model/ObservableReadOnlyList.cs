using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public class ObservableReadOnlyList<T> : ObservableCollection<T>, IObservableReadOnlyList<T>
    {
    }
}
