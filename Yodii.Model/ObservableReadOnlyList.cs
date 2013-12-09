using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Wrapper class around an ObservableCollection, implementing IObservableReadOnlyList.
    /// <see cref="System.Collections.ObjectModel.ObservableCollection{T}"/>
    /// </summary>
    /// <typeparam name="T">Type of list items.</typeparam>
    public class ObservableReadOnlyList<T> : ObservableCollection<T>, IObservableReadOnlyList<T>
    {
    }
}
