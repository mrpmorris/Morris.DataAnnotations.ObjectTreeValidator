using System;
using System.Collections.Generic;

namespace Morris.DataAnnotations.ObjectTreeValidator;

public static class RecursiveValidator
{
	public static bool TryValidateObject(
		object instance,
		out ICollection<RecursiveValidationResult> validationResults)
	{
		if (instance is null)
			throw new ArgumentNullException(nameof(instance));

		var outValidationResults = new List<RecursiveValidationResult>();
		ValidateObject(
			instance: instance,
			new List<string>(),
			alreadyValidated: new HashSet<object>(),
			validationResults: outValidationResults);
		validationResults = outValidationResults;
		return validationResults.Count == 0;
	}

	internal static void ValidateObject(
		object instance,
		List<string> path,
		HashSet<object> alreadyValidated,
		List<RecursiveValidationResult> validationResults)
	{
		if (!alreadyValidated.Add(instance))
			return;
		PropertyValidator[] validators = ValidatorCache.CachePropertyValidators(instance.GetType());

		foreach (PropertyValidator validator in validators)
		{
			path.Add(validator.PropertyInfo.Name);
			validator.Validate(instance, path, alreadyValidated, validationResults, out object? value);
			try
			{
				if (value is not null)
					ValidateObject(value, path, alreadyValidated, validationResults);
			}
			finally
			{
				path.RemoveAt(path.Count - 1);
			}
		}
	}
}
