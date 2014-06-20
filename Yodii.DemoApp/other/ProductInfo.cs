using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.DemoApp
{
    public interface IProductInfo
    {
        int Price { get; }
        ProductCategory ProductCategory { get; }
        string Name { get; }

        string Producer { get; }// ou un IBusiness, 
        //c'est qu'au moment de l'implémentation que vais me rendre compte de cette bourde ><
    }
    public class ProductInfo: IProductInfo //j'ai vu que t'avais déja un truc, donc au pire osef.
    {
        int _price;
        ProductCategory _category;
        string _name;
        string _businessName;//fabricant en fait
        public int Price { get{ return _price; } }
        public ProductCategory ProductCategory { get{ return _category; } }
        public string Name { get { return _name; } }


        public string Producer { get { return _businessName; } }// ou un IBusiness, 
        //c'est qu'au moment de l'implémentation que vais me rendre compte de cette bourde ><
    }
}
