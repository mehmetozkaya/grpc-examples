﻿using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ProductsContext _productDbContext;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductsContext context, ILogger<ProductService> logger)
        {
            _productDbContext = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, 
                                                                ServerCallContext context)
        {
            var product = await _productDbContext.Product.FindAsync(request.ProductId);
            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Product with ID={request.ProductId} is not found."));
            }
            
            var productModel = new ProductModel
            {
                ProductId = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Status = ProductStatus.Instock,
                CreatedTime = Timestamp.FromDateTime(product.CreateTime)
            };

            return productModel;                        
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, 
                                                    IServerStreamWriter<ProductModel> responseStream, 
                                                    ServerCallContext context)
        {
            var productList = await _productDbContext.Product.ToListAsync();

            foreach (var product in productList)
            {
                var productModel = new ProductModel
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(product.CreateTime)
                };

                await responseStream.WriteAsync(productModel);
            }            
        }

    }
}
