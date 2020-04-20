﻿using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CarbonitePersist.ManualTests
{
    public class Testing
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

        public static async Task Main()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);

            // Apply config           
            LogManager.Configuration = config;

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

            var customerC = new Customer
            {
                Id = 4,
                Firstname = "Armin",
                Lastname = "Slate",
                Email = "slatea@domain.com",
                Phone = "555-1956",
                CustomerSince = DateTime.Parse("2011-12-21"),
            };

            var newcustomers = new List<Customer> { customerA, customerB };

            var ct = new CarboniteTool(@"Path=C:\Temp\Test");
            var customerCollection = ct.GetCollection<Customer>();

            await customerCollection.InsertAsync(newcustomers);

            await customerCollection.InsertAsync(customerC);

            var customers = await customerCollection.GetAllAsync();

            foreach (Customer c in customers)
            {
                Console.WriteLine($"Customer {c.Firstname} {c.Lastname} has ID {c.Id}");
            }

            var id = new Guid("06a1bac6-b534-421d-a130-1441fe0ef5c6");

            var orderA = new Order
            {
                Id = id,
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
                Customer = customerC,
            };

            var orderCollection = ct.GetCollection<Order>("OrderNameOverride");

            await orderCollection.InsertAsync(orderA);
            await orderCollection.InsertAsync(orderB);

            var pullorder = await orderCollection.GetByIdAsync(id);
            Console.WriteLine($"Pulled order {pullorder.Id} for customer {pullorder.Customer.Firstname} {pullorder.Customer.Lastname} came to a total of {pullorder.Total}");

            foreach (Order o in await orderCollection.GetAllAsync())
                Console.WriteLine($"Order {o.Id} for customer {o.Customer.Firstname} {o.Customer.Lastname} came to a total of {o.Total}");

            var fileStore = ct.GetStorage();
            await fileStore.UploadAsync(1, @"C:\Temp\filesource\permissions.docx");

            var metadata = await fileStore.GetAllAsync();

            foreach (FileStorageMetadata file in metadata)
                Console.WriteLine($"There is a file called {file.Filename} with ID {file.Id} in storage");
        }
    }
}
