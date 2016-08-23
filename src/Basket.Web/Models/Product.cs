using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Basket.Web.Models
{
    public class Product
    {
        public string ProductId { get; set; }
        public string Description { get; set; }
        public string LongDescription { get; set; }
        public string ImageUrl { get; set; }
        public string[] Categories { get; set; } = {};

        public Sku[] Skus { get; set; } = {};
    }

    public class Sku
    {
        public string Code { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public bool Available { get; set; }
    }

    public class ProductCatalog
    {
        public string Id { get; set; }
        public Product[] Products { get; set; } = {};
    }
}