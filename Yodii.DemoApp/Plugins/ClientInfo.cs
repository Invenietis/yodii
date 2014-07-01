using System;

namespace Yodii.DemoApp
{
    public class ClientInfo: IClientInfo
    {
        readonly string _name;
        readonly string _address;

        public ClientInfo( string name, string address )
        {
            _name = name;
            _address = address;
        }
        
        public string Name { get { return _name; } }
        public string Address { get { return _address; } }
    }
}
