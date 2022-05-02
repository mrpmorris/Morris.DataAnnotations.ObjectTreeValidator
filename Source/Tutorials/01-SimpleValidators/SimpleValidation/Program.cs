using Morris.DataAnnotations.ObjectTreeValidator;
using System.ComponentModel.DataAnnotations;

Validate("Name required", new Person());
Validate("Name too short", new Person { Name = "12" });
Validate("Name too long", new Person { Name = "123456" });
Validate("Name just right", new Person { Name = "1234" });

static void Validate(string scenario, Person person)
{
	Console.WriteLine($"\r\nScenario: {scenario}");
	RecursiveValidator.TryValidateObject(person, out ICollection<RecursiveValidationResult> validationResults);
	foreach(var validationResult in validationResults)
		Console.WriteLine($"{validationResult.FullPath} - {validationResult.ErrorMessage}");
}

class Person
{
	[Required, MinLength(3), MaxLength(5)]
	public string Name { get; set; } = null!;
}