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
            readonly ObservableCollection<InternalYodiiCommand> _commands;
            int _currentSoverNumber;

            public YodiiCommandList()
            {
                _commands = new ObservableCollection<InternalYodiiCommand>();
                _commands.CollectionChanged += RelayCollectionChanged;
                ((INotifyPropertyChanged)_commands).PropertyChanged += RelayPropertyChanged;
            }

            public void Merge( IReadOnlyList<InternalYodiiCommand> newCommands )
            {
                if( newCommands.Count == 0 )
                {
                    _commands.Clear();
                    return;
                }
                if( _commands.Count == 0 )
                {
                    _commands.AddRange( newCommands );
                    return;
                }

                if( _commands[0] != newCommands[0] ) _commands.Insert( 0, newCommands[0] );
                for( int i = 1; i < newCommands.Count; i++ )
                {
                    if( newCommands[i] != _commands[i] ) _commands.RemoveAt( i-- );
                }
                while( _commands.Count > newCommands.Count ) _commands.RemoveAt( Count - 1 );
                Debug.Assert( this.Count == newCommands.Count );
            }

            public void RemoveCaller( string callerKey )
            {
                _commands.RemoveWhereAndReturnsRemoved( c => c.Command.CallerKey == callerKey ).Count();
            }

            public void ResetOnStart( IEnumerable<YodiiCommand> persistedCommands = null )
            {
                _commands.Clear();
                if( persistedCommands != null ) _commands.AddRange( persistedCommands.Select( c => new InternalYodiiCommand( c, null ) ) );
            }

            internal void EnsureBindingsTo( ConfigurationSolver solver )
            {
                if( _currentSoverNumber != solver.UniqueNumber )
                {
                    _currentSoverNumber = solver.UniqueNumber;
                    foreach( var c in _commands ) c.BindToConfigSolverData( solver );
                }
            }

            internal void ClearBindings()
            {
                _currentSoverNumber = 0;
                foreach( var c in _commands ) c.BindToConfigSolverData( null );
            }

            internal void ApplyCommands( Func<InternalYodiiCommand, bool> existingCommandFilter, InternalYodiiCommand newOne, List<InternalYodiiCommand> collector )
            {
                int iCommand = 0;
                var prevCommands = existingCommandFilter != null ? _commands.Where( existingCommandFilter ) : _commands;
                foreach( var previous in prevCommands )
                {
                    if( newOne == null || !previous.IsMaskedBy( newOne ) )
                    {
                        if( previous.ApplyCommand( ++iCommand ) )
                        {
                            collector.Add( previous );
                        }
                    }
                }
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
                get { return _commands.Count; }
            }

            IEnumerator<YodiiCommand> IEnumerable<YodiiCommand>.GetEnumerator()
            {
                return _commands.Select( c => c.Command ).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<YodiiCommand>)this).GetEnumerator();
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public event PropertyChangedEventHandler PropertyChanged;

            YodiiCommand IReadOnlyList<YodiiCommand>.this[int index]
            {
                get { return _commands[index].Command; }
            }

            #endregion

        }

}
