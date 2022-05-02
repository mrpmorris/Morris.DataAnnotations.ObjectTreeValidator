using Morris.DataAnnotations.ObjectTreeValidator;
using System.ComponentModel.DataAnnotations;

var validAddress = new Address { Line1 = "Line1", PostalCode = "Postal code" };
Validate("No manager or address", new Office());
Validate("Invalid office address and manager",
	new Office {
		Manager = new Manager(),
		Address = new Address()
	});
Validate("Manager with invalid address",
	new Office {
		Manager = new Manager {  Name = "Bob" },
		Address = validAddress
	});
Validate("Manage with a two invalid addresses",
	new Office {
		Address = validAddress,
		Manager = new Manager {
			Name = "Bob",
			Addresses = new Address[] {
				new Address(),
				validAddress,
				validAddress,
				new Address()
			}.ToList()
		}
	});

Validate("Everything is valid",
	new Office {
		Address = validAddress,
		Manager = new Manager {
			Name = "Bob",
			Addresses = new Address[] {
				validAddress,
				validAddress,
			}.ToList()
		}
	});

static void Validate(string scenario, Office office)
{
	Console.WriteLine($"\r\nScenario: {scenario}");
	RecursiveValidator.TryValidateObject(office, out ICollection<RecursiveValidationResult> validationResults);
	foreach (var validationResult in validationResults)
		Console.WriteLine($"{validationResult.FullPath} - {validationResult.ErrorMessage}");
}

class Office
{
	[Required]
	public Manager Manager { get; set; } = null!;
	[Required]
	public Address Address { get; set; } = null!;
}

class Manager
{
	[Required, MinLength(3), MaxLength(5)]
	public string Name { get; set; } = null!;

	[Required, MinLength(1)]
	public List<Address> Addresses { get; set; } = new();
}

class Address
{
	[Required]
	public string Line1 { get; set; } = null!;
	public string? Line2 { get; set; }
	public string? Line3 { get; set; }
	public string? Line4 { get; set; }
	public string? Region { get; set; }
	[Required]
	public string PostalCode { get; set; } = null!;
}