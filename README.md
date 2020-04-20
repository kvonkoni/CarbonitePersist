# CarbonitePersist

CarbonitePersist is a project to create a simple persistence tool without the use of a database, storing entities within collections in individual XML files and allowing them to be retrieved by ID. It favours simplicity, human readability, and portability over advanced features.

## Purpose

CarbonitePersist stores entities as individual XML files within a folder structure, allowing you to store and retrieve them without the need manage the files directly. Search and retrieval is done by assigning each entity an Id at the time it is inserted into Carbonite. An existing entity in the collection with the same Id is automatically replaced.

The package also allows you to store and retrieve large files from elsewhere on the disk, as well as associate them with additional metadata.

## Limitations

CarbonitePersist is designed to be a simple file manager without advanced features. Its simplicily leads to a number of limitations that should be kept in mind:
* While it provides TPL  methods for asynchronous programming, it does not implement any locks on collections while they're in use. Since each entity is a separate file on the disk, it sill allows for simultaneous access of different collections. However, simultaneous accessing of files within the same collection may result in conflicts. You should avoid modifying the contents of the same collection in multiple threads.
* Since the collections are, by design, easily modifyable in any file manager, use caution when modifying the files manually. Changing the structure or naming scheme of the files in the folder structure may cause errors.

## Usage

A minimal example for storing clients and orders in separate collections.

```cs
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
    static async Task Main(string[] args)
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
        await fileStore.UploadAsync(1, @"C:\Temp\filesource\This is a test file.docx");
        await fileStore.UploadAsync(2, @"C:\Temp\filesource\broken.pdf");
        await fileStore.UploadAsync(3, @"C:\Temp\filesource\letter.txt");

        await fileStore.UpdateDescription(3, "this is a description");

        var metadata = await fileStore.GetAllAsync();

        foreach (FileStorageMetadata file in metadata)
        {
            if (file.Description != null)
                Console.WriteLine($"There is a file called {file.Filename} with ID {file.Id} and description \"{file.Description}\" in storage");
            else
                Console.WriteLine($"There is a file called {file.Filename} with ID {file.Id} and no description in storage");
        }

        await fileStore.DownloadAsync(metadata[0].Id, $"C:\\Temp\\filedest\\{metadata[0].Filename}", true);
    }
}
```

## Contributing

Feel free to contribute by creating an issue or submitting a PR.

## Author

[Kier von Konigslow](https://github.com/kvonkoni)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details