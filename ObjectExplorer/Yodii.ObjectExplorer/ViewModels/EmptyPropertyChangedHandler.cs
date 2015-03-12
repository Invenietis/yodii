using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.ObjectExplorer.ViewModels
{
    public class EmptyPropertyChangedHandler : INotifyPropertyChanged
    {
#pragma warning disable 67 // Handled by PropertyChanged.Fody
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
    }
}
