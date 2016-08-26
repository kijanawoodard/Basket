using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

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

    public class ProductSearchIndex : AbstractIndexCreationTask<ProductCatalog, ProductSearchIndex.Query>
    {
        public class Query
        {
            public string Search { get; set; }
        }
        public class Result
        {
            public string ProductId { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
            public string ImageUrl { get; set; }
        }
        public ProductSearchIndex()
        {
            Map = catalogs => from catalog in catalogs
                            from product in catalog.Products
                            from sku in product.Skus
                            select new
                            {
                                product.ProductId,
                                sku.Description,
                                sku.ImageUrl,
                                sku.Price,
                                Search = new[]
                                {
                                    sku.Description,
                                    product.LongDescription   
                                }
                            };

            Indexes.Add(x => x.Search, FieldIndexing.Analyzed);
            StoreAllFields(FieldStorage.Yes);

            MaxIndexOutputsPerDocument = 30*1000;
        }
    }
}