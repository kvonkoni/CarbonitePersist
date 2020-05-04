using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace CarbonitePersist.UnitTest
{
    public class Customer
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime CustomerSince { get; set; }
    }

    public class Order
    {
        public Guid Id { get; set; }
        public double Subtotal { get; set; }
        public double Total { get; set; }
        public bool IsFilled { get; set; }
        public Customer Customer { get; set; }
    }

    public class TestCarboniteCollection
    {
        [Fact]
        public async Task TestInsertAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var customer = new Customer
            {
                Id = 2,
                Firstname = "Bill",
                Lastname = "Masterson",
                Email = "bill.masterson@email.com",
                Phone = "555-1234",
                CustomerSince = DateTime.Parse("2008-11-01"),
            };

            var order = new Order
            {
                Id = new Guid("16db1209-ac9c-472f-bf76-5ba4dcecf2bd"),
                Subtotal = 240.99,
                Total = 258.25,
                IsFilled = false,
                Customer = customer,
            };

            await col.InsertAsync(order);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestInsertOverloadAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var customerA = new Customer
            {
                Id = 2,
                Firstname = "Bill",
                Lastname = "Masterson",
                Email = "bill.masterson@email.com",
                Phone = "555-1234",
                CustomerSince = DateTime.Parse("2008-11-01"),
            };

            var customerB = new Customer
            {
                Id = 3,
                Firstname = "Jill",
                Lastname = "James",
                Email = "jjames@domain.com",
                Phone = "555-9456",
                CustomerSince = DateTime.Parse("2012-03-11"),
            };

            var orderA = new Order
            {
                Id = new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6"),
                Subtotal = 55.23,
                Total = 60.91,
                IsFilled = true,
                Customer = customerA,
            };

            var orderB = new Order
            {
                Id = new Guid("16db1209-ac9c-472f-bf76-5ba4dcecf2bd"),
                Subtotal = 240.99,
                Total = 258.25,
                IsFilled = false,
                Customer = customerB,
            };

            await col.InsertAsync(new List<Order> { orderA, orderB });

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestGetAllAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var orders = await col.GetAllAsync();

            Assert.Equal(2, orders.Length);

            Assert.Equal(new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6"), orders[0].Id);
            Assert.Equal(55.23, orders[0].Subtotal);
            Assert.Equal(60.91, orders[0].Total);
            Assert.True(orders[0].IsFilled);

            Assert.Equal(2, orders[0].Customer.Id);
            Assert.Equal("Bill", orders[0].Customer.Firstname);
            Assert.Equal("Masterson", orders[0].Customer.Lastname);
            Assert.Equal("bill.masterson@email.com", orders[0].Customer.Email);
            Assert.Equal("555-1234", orders[0].Customer.Phone);
            Assert.Equal(DateTime.Parse("2008-11-01"), orders[0].Customer.CustomerSince);

            Assert.Equal(new Guid("16db1209-ac9c-472f-bf76-5ba4dcecf2bd"), orders[1].Id);
            Assert.Equal(240.99, orders[1].Subtotal);
            Assert.Equal(258.25, orders[1].Total);
            Assert.False(orders[1].IsFilled);

            Assert.Equal(4, orders[1].Customer.Id);
            Assert.Equal("Armin", orders[1].Customer.Firstname);
            Assert.Equal("Slate", orders[1].Customer.Lastname);
            Assert.Equal("slatea@domain.com", orders[1].Customer.Email);
            Assert.Equal("555-1956", orders[1].Customer.Phone);
            Assert.Equal(DateTime.Parse("2011-12-21"), orders[1].Customer.CustomerSince);
        }

        [Fact]
        public async Task TestFindAllAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var orders = await col.FindAllAsync(x => x.Id == "06a1bac6-b534-421d-a130-1441fe0ef5c6");

            Assert.Single(orders);

            Assert.Equal(new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6"), orders[0].Id);
            Assert.Equal(55.23, orders[0].Subtotal);
            Assert.Equal(60.91, orders[0].Total);
            Assert.True(orders[0].IsFilled);

            Assert.Equal(2, orders[0].Customer.Id);
            Assert.Equal("Bill", orders[0].Customer.Firstname);
            Assert.Equal("Masterson", orders[0].Customer.Lastname);
            Assert.Equal("bill.masterson@email.com", orders[0].Customer.Email);
            Assert.Equal("555-1234", orders[0].Customer.Phone);
            Assert.Equal(DateTime.Parse("2008-11-01"), orders[0].Customer.CustomerSince);
        }

        [Fact]
        public async Task TestGetByIdAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var order = await col.GetByIdAsync("06a1bac6-b534-421d-a130-1441fe0ef5c6");

            Assert.Equal(new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6"), order.Id);
            Assert.Equal(55.23, order.Subtotal);
            Assert.Equal(60.91, order.Total);
            Assert.True(order.IsFilled);

            Assert.Equal(2, order.Customer.Id);
            Assert.Equal("Bill", order.Customer.Firstname);
            Assert.Equal("Masterson", order.Customer.Lastname);
            Assert.Equal("bill.masterson@email.com", order.Customer.Email);
            Assert.Equal("555-1234", order.Customer.Phone);
            Assert.Equal(DateTime.Parse("2008-11-01"), order.Customer.CustomerSince);
        }

        [Fact]
        public async Task TestGetByIdsAsync()
        {
            var path = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + "\\CarboniteTest";
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var orders = await col.GetByIdsAsync(new List<object> { "06a1bac6-b534-421d-a130-1441fe0ef5c6", "16db1209-ac9c-472f-bf76-5ba4dcecf2bd" });

            Assert.Equal(2, orders.Length);

            Assert.Equal(new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6"), orders[0].Id);
            Assert.Equal(55.23, orders[0].Subtotal);
            Assert.Equal(60.91, orders[0].Total);
            Assert.True(orders[0].IsFilled);

            Assert.Equal(2, orders[0].Customer.Id);
            Assert.Equal("Bill", orders[0].Customer.Firstname);
            Assert.Equal("Masterson", orders[0].Customer.Lastname);
            Assert.Equal("bill.masterson@email.com", orders[0].Customer.Email);
            Assert.Equal("555-1234", orders[0].Customer.Phone);
            Assert.Equal(DateTime.Parse("2008-11-01"), orders[0].Customer.CustomerSince);

            Assert.Equal(new Guid("16db1209-ac9c-472f-bf76-5ba4dcecf2bd"), orders[1].Id);
            Assert.Equal(240.99, orders[1].Subtotal);
            Assert.Equal(258.25, orders[1].Total);
            Assert.False(orders[1].IsFilled);

            Assert.Equal(4, orders[1].Customer.Id);
            Assert.Equal("Armin", orders[1].Customer.Firstname);
            Assert.Equal("Slate", orders[1].Customer.Lastname);
            Assert.Equal("slatea@domain.com", orders[1].Customer.Email);
            Assert.Equal("555-1956", orders[1].Customer.Phone);
            Assert.Equal(DateTime.Parse("2011-12-21"), orders[1].Customer.CustomerSince);
        }

        [Fact]
        public async Task TestDeleteAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var customerA = new Customer
            {
                Id = 2,
                Firstname = "Bill",
                Lastname = "Masterson",
                Email = "bill.masterson@email.com",
                Phone = "555-1234",
                CustomerSince = DateTime.Parse("2008-11-01"),
            };

            var customerB = new Customer
            {
                Id = 3,
                Firstname = "Jill",
                Lastname = "James",
                Email = "jjames@domain.com",
                Phone = "555-9456",
                CustomerSince = DateTime.Parse("2012-03-11"),
            };

            var orderA = new Order
            {
                Id = new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6"),
                Subtotal = 55.23,
                Total = 60.91,
                IsFilled = true,
                Customer = customerA,
            };

            var orderB = new Order
            {
                Id = new Guid("16db1209-ac9c-472f-bf76-5ba4dcecf2bd"),
                Subtotal = 240.99,
                Total = 258.25,
                IsFilled = false,
                Customer = customerB,
            };

            await col.InsertAsync(new List<Order> { orderA, orderB });

            await col.DeleteAsync("06a1bac6-b534-421d-a130-1441fe0ef5c6");

            var orders = await col.GetAllAsync();

            Assert.Single(orders);
            Assert.Equal(new Guid("16db1209-ac9c-472f-bf76-5ba4dcecf2bd"), orders[0].Id);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestDeleteMultipleAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var customerA = new Customer
            {
                Id = 2,
                Firstname = "Bill",
                Lastname = "Masterson",
                Email = "bill.masterson@email.com",
                Phone = "555-1234",
                CustomerSince = DateTime.Parse("2008-11-01"),
            };

            var customerB = new Customer
            {
                Id = 3,
                Firstname = "Jill",
                Lastname = "James",
                Email = "jjames@domain.com",
                Phone = "555-9456",
                CustomerSince = DateTime.Parse("2012-03-11"),
            };

            var orderA = new Order
            {
                Id = new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6"),
                Subtotal = 55.23,
                Total = 60.91,
                IsFilled = true,
                Customer = customerA,
            };

            var orderB = new Order
            {
                Id = new Guid("16db1209-ac9c-472f-bf76-5ba4dcecf2bd"),
                Subtotal = 240.99,
                Total = 258.25,
                IsFilled = false,
                Customer = customerB,
            };

            await col.InsertAsync(new List<Order> { orderA, orderB });

            await col.DeleteMultipleAsync(new List<object> { "06a1bac6-b534-421d-a130-1441fe0ef5c6", "16db1209-ac9c-472f-bf76-5ba4dcecf2bd" });

            var orders = await col.GetAllAsync();

            Assert.Empty(orders);

            Directory.Delete(path, true);
        }

        [Fact]
        public async Task TestDeleteAllAsync()
        {
            var database = Guid.NewGuid().ToString();
            var path = Path.Combine(Path.GetTempPath(), database);
            var ct = new CarboniteTool($"Path={path}");
            var col = ct.GetCollection<Order>();

            var customerA = new Customer
            {
                Id = 2,
                Firstname = "Bill",
                Lastname = "Masterson",
                Email = "bill.masterson@email.com",
                Phone = "555-1234",
                CustomerSince = DateTime.Parse("2008-11-01"),
            };

            var customerB = new Customer
            {
                Id = 3,
                Firstname = "Jill",
                Lastname = "James",
                Email = "jjames@domain.com",
                Phone = "555-9456",
                CustomerSince = DateTime.Parse("2012-03-11"),
            };

            var orderA = new Order
            {
                Id = new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6"),
                Subtotal = 55.23,
                Total = 60.91,
                IsFilled = true,
                Customer = customerA,
            };

            var orderB = new Order
            {
                Id = new Guid("16db1209-ac9c-472f-bf76-5ba4dcecf2bd"),
                Subtotal = 240.99,
                Total = 258.25,
                IsFilled = false,
                Customer = customerB,
            };

            await col.InsertAsync(new List<Order> { orderA, orderB });

            await col.DeleteAllAsync();

            var orders = await col.GetAllAsync();

            Assert.Empty(orders);

            Directory.Delete(path, true);
        }
    }
}
