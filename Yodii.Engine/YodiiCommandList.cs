using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
        class YodiiCommandList : IObservableReadOnlyList<YodiiCommand>
        {
            public readonly ObservableCollection<InternalYodiiCommand> Commands;
            int _currentSoverNumber;

            public YodiiCommandList()
            {
                Commands = new ObservableCollection<InternalYodiiCommand>();
                Commands.CollectionChanged += RelayCollectionChanged;
                ((INotifyPropertyChanged)Commands).PropertyChanged += RelayPropertyChanged;
            }

            public void Merge( IReadOnlyList<InternalYodiiCommand> newCommands )
            {
                if( newCommands.Count == 0 )
                {
                    Commands.Clear();
                    return;
                }
                if( Commands.Count == 0 )
                {
                    Commands.AddRange( newCommands );
                    return;
                }

                if( Commands[0] != newCommands[0] ) Commands.Insert( 0, newCommands[0] );
                for( int i = 1; i < newCommands.Count; i++ )
                {
                    if( newCommands[i] != Commands[i] ) Commands.RemoveAt( i-- );
                }
                while( Commands.Count > newCommands.Count ) Commands.RemoveAt( Count - 1 );
                Debug.Assert( this.Count == newCommands.Count );
            }

            internal void EnsureBindingsTo( ConfigurationSolver solver )
            {
                if( _currentSoverNumber != solver.UniqueNumber )
                {
                    _currentSoverNumber = solver.UniqueNumber;
                    foreach( var c in Commands ) c.BindToConfigSolverData( solver );
                }
            }

            internal void ClearBindings()
            {
                _currentSoverNumber = 0;
                foreach( var c in Commands ) c.BindToConfigSolverData( null );
            }

            #region IObservableReadOnlyList<YodiiCommand> support

            void RelayCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
            {
                var h = CollectionChanged;
                if( h != null ) h( this, e );
            }

            void RelayPropertyChanged( object sender, PropertyChangedEventArgs e )
            {
                var h = PropertyChanged;
                if( h != null ) h( this, e );
            }

            public int Count
            {
                get { return Commands.Count; }
            }

            IEnumerator<YodiiCommand> IEnumerable<YodiiCommand>.GetEnumerator()
            {
                return Commands.Select( c => c.Command ).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<YodiiCommand>)this).GetEnumerator();
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public event PropertyChangedEventHandler PropertyChanged;

            YodiiCommand IReadOnlyList<YodiiCommand>.this[int index]
            {
                get { return Commands[index].Command; }
            }

            #endregion

        }

}
