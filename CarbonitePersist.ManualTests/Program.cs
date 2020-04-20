using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarbonitePersist.ManualTests
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
    
    class Program
    {
        static async Task OldMain(string[] args)
        {
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

            var newcustomers = new List<Customer> { customerA, customerB };

            var ct = new CarboniteTool(@"Path=C:\Temp\Test");
            var customerCollection = ct.GetCollection<Customer>();

            await customerCollection.InsertAsync(newcustomers);

            var customers = await customerCollection.GetAllAsync();

            foreach (Customer e in customers)
            {
                Console.WriteLine($"Customer {e.Firstname} {e.Lastname} has ID {e.Id}");
            }

            var id = Guid.NewGuid();

            var order = new Order
            {
                Id = id,
                Subtotal = 55.23,
                Total = 60.91,
                IsFilled = true,
                Customer = customerA,
            };

            var orderCollection = ct.GetCollection<Order>("OrderNameOverride");

            await orderCollection.InsertAsync(order);

            var pullorder = await orderCollection.GetByIdAsync(id);

            Console.WriteLine($"Order {pullorder.Id} for customer {pullorder.Customer.Firstname} {pullorder.Customer.Lastname} came to a total of {pullorder.Total}");
        }
    }
}
