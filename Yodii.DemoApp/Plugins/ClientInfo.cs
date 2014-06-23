using System;

namespace Yodii.DemoApp
{
    public class ClientInfo: IClientInfo
    {
        readonly string _name;
        readonly string _adress;

        public ClientInfo( string name, string adress )
        {
            _name = name;
            _adress = adress;
        }
        
        public string Name { get { return _name; } }
        public string Adress { get { return _adress; } }
    }
}
