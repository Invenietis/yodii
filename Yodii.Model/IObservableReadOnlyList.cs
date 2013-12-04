using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IObservableReadOnlyList<T> : INotifyPropertyChanged, IReadOnlyList<T>, INotifyCollectionChanged
    {
    }
}
