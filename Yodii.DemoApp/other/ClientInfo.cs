using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.DemoApp
{
    public interface IClientInfo
    {
        string Name { get; }
        string Adress { get; }
        //pareil vais peut-etre regretter ne pas référencer le client ici.
        //on pourra toujours ajouter ^^.
    }
    public class ClientInfo: IClientInfo
    {
        public ClientInfo( string name, string adress )
        {
            _name = name;
            _adress = adress;
        }
        string _name;
        string _adress;
        public string Name { get { return _name; } }
        public string Adress { get { return _adress; } }
    }
}
