using Morris.DataAnnotations.ObjectTreeValidator;
using System.ComponentModel.DataAnnotations;

Validate("Name required", new Manager());
Validate("Name too short", new Manager { Name = "12" });
Validate("Name too long", new Manager { Name = "123456" });
Validate("Name just right", new Manager { Name = "1234" });

static void Validate(string scenario, Manager manager)
{
	Console.WriteLine($"Scenario: {scenario}");
	var office = new Office { Manager = manager };
	RecursiveValidator.TryValidateObject(office, out ICollection<RecursiveValidationResult> validationResults);
	foreach (var validationResult in validationResults)
		Console.WriteLine($"{validationResult.FullPath} - {validationResult.ErrorMessage}");
}

class Office
{
	public Manager? Manager { get; set; }
}

class Manager
{
	[Required, MinLength(3), MaxLength(5)]
	public string Name { get; set; } = null!;
}