using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Observable, read-only list.
    /// </summary>
    /// <typeparam name="T">Type of the list items.</typeparam>
    public interface IObservableReadOnlyList<T> : INotifyPropertyChanged, IReadOnlyList<T>, INotifyCollectionChanged
    {
    }
}
