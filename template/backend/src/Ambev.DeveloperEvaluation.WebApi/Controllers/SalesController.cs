using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ambev.DeveloperEvaluation.Domain.Models;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private static ConcurrentBag<Sale> Sales = new ConcurrentBag<Sale>();
        private readonly IValidator<Sale> _saleValidator;

        public SalesController(IValidator<Sale> saleValidator)
        {
            _saleValidator = saleValidator;
        }

        [HttpPost]
        public IActionResult CreateSale([FromBody] Sale sale)
        {
            var validationResult = _saleValidator.Validate(sale);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            sale.Id = Guid.NewGuid();
            sale.Date = DateTime.UtcNow;
            ApplyBusinessRules(sale);
            Sales.Add(sale);
            Console.WriteLine("SaleCreated");
            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }

        [HttpGet]
        public IActionResult GetSales()
        {
            return Ok(Sales);
        }

        [HttpGet("{id}")]
        public IActionResult GetSale(Guid id)
        {
            var sale = Sales.FirstOrDefault(s => s.Id == id);
            if (sale == null)
            {
                return NotFound();
            }
            return Ok(sale);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSale(Guid id, [FromBody] Sale updatedSale)
        {
            var sale = Sales.FirstOrDefault(s => s.Id == id);
            if (sale == null)
            {
                return NotFound();
            }

            var validationResult = _saleValidator.Validate(updatedSale);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            sale.Customer = updatedSale.Customer;
            sale.Branch = updatedSale.Branch;
            sale.Products = updatedSale.Products;
            ApplyBusinessRules(sale);
            Console.WriteLine("SaleModified");
            return Ok(sale);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSale(Guid id)
        {
            var sale = Sales.FirstOrDefault(s => s.Id == id);
            if (sale == null)
            {
                return NotFound();
            }
            sale.Cancelled = true;
            Console.WriteLine("SaleCancelled");
            return NoContent();
        }

        private void ApplyBusinessRules(Sale sale)
        {
            foreach (var product in sale.Products)
            {
                if (product.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Quantity for '{product.Name}' must be greater than zero."
                    );
                }
                if (product.UnitPrice <= 0)
                {
                    throw new InvalidOperationException(
                        $"Unit price for '{product.Name}' must be greater than zero."
                    );
                }
                if (product.Quantity >= 4 && product.Quantity < 10)
                {
                    product.Discount = 0.10m;
                    Console.WriteLine($"Applying 10% discount to '{product.Name}'.");
                }
                else if (product.Quantity >= 10 && product.Quantity <= 20)
                {
                    product.Discount = 0.20m;
                    Console.WriteLine($"Applying 20% discount to '{product.Name}'.");
                }
                else if (product.Quantity > 20)
                {
                    throw new InvalidOperationException($"Cannot sell more than 20 identical items of '{product.Name}'.");
                }
                else
                {
                    product.Discount = 0;
                }

                product.TotalAmount = product.Quantity * product.UnitPrice * (1 - product.Discount);
            }
            sale.TotalSaleAmount = sale.Products.Sum(p => p.TotalAmount);
            Console.WriteLine("Business rules applied to sale.");
        }
    }
}
