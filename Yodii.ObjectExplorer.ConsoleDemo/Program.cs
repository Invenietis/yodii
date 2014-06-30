using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ConsoleDemo
{
    class Program
    {
        static void Main( string[] args )
        {
            ObjectExplorerManager m = new ObjectExplorerManager();
            m.Engine.LiveInfo.Plugins.CollectionChanged += Plugins_CollectionChanged;
            m.Engine.PropertyChanged += Engine_PropertyChanged;

            m.Run();

            Console.WriteLine( "Press Enter to stop engine." );
            Console.ReadLine();

            m.Engine.Stop();

            Console.WriteLine( "Press Enter to quit." );
            Console.ReadLine();
        }

        static void Engine_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            string value = GetPropertyValue( sender, e.PropertyName );

            Console.WriteLine( "Engine.{0} = {1}", e.PropertyName, value );
        }

        static void Plugins_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case NotifyCollectionChangedAction.Add:
                    Console.WriteLine( "Added:" );
                    foreach( var px in e.NewItems ) Console.WriteLine( "+= {0}", px.ToString() );
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Console.WriteLine( "Removed:" );
                    foreach( var px in e.NewItems ) Console.WriteLine( "-= {0}", px.ToString() );
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Console.WriteLine( "Collection reset" );
                    break;
            }
        }

        static string GetPropertyValue( object instance, string propertyName )
        {
            return instance.GetType().GetProperty( propertyName ).GetValue( instance, null ).ToString();
        }
    }
}
