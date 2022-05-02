using Morris.DataAnnotations.ObjectTreeValidator.Extensions;
using Morris.DataAnnotations.ObjectTreeValidator.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Morris.DataAnnotations.ObjectTreeValidator;

internal class PropertyValidator
{
	public delegate void ValidateDelegate(
		object instance,
		List<string> path,
		HashSet<object> alreadyValidated,
		List<RecursiveValidationResult> validationResults,
		out object? value);

	public readonly ValidateDelegate Validate;
	public readonly PropertyInfo PropertyInfo;

	private readonly ValidationAttribute[] ValidationAttributes;

	private PropertyValidator(PropertyInfo propertyInfo)
	{
		PropertyInfo = propertyInfo ?? throw new ArgumentNullException(nameof(propertyInfo));
		ValidationAttributes = Array.Empty<ValidationAttribute>();
		Validate =
			propertyInfo.PropertyType.GetGenericEnumerableElementType() is not null
			? ValidateEnumerableProperty
			: ValidateObjectProperty;
	}

	public PropertyValidator(PropertyInfo propertyInfo, ValidationAttribute[] validationAttributes)
		: this(propertyInfo)
	{
		ValidationAttributes = validationAttributes ?? throw new ArgumentNullException(nameof(validationAttributes));
		if (ValidationAttributes.Length == 0)
			throw new ArgumentException("At least one validator is required", paramName: nameof(ValidationAttributes));
		if (ValidationAttributes.Any(x => x is null))
			throw new ArgumentException("Validator attributes cannot be null", paramName: nameof(ValidationAttributes));
	}

	public static PropertyValidator CreateEmpty(PropertyInfo propertyInfo) =>
		new PropertyValidator(propertyInfo);

	public static IEnumerable<KeyValuePair<PropertyInfo, PropertyValidator?>> CreatePropertyValidators(Type type)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		var propertyInfos = type.GetProperties(
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		var result = new List<KeyValuePair<PropertyInfo, PropertyValidator?>>();
		foreach (var propertyInfo in propertyInfos)
		{
			ValidationAttribute[] validationAttributes =
				Attribute
					.GetCustomAttributes(propertyInfo, true)
					.OfType<ValidationAttribute>()
					.ToArray();
			if (validationAttributes.Length == 0)
				result.Add(new KeyValuePair<PropertyInfo, PropertyValidator?>(propertyInfo, null));
			else
			{
				var propertyValidator = new PropertyValidator(propertyInfo, validationAttributes);
				result.Add(new KeyValuePair<PropertyInfo, PropertyValidator?>(propertyInfo, propertyValidator));
			}
		}
		return result;
	}

	private void ValidateObjectProperty(
		object owner,
		List<string> path,
		HashSet<object> alreadyValidated,
		List<RecursiveValidationResult> validationResults,
		out object? value)
	{
		value = PropertyInfo.GetValue(owner);
		var validationContext = new ValidationContext(owner) {
			MemberName = PropertyInfo.Name
		};
		foreach (ValidationAttribute validationAttribute in ValidationAttributes)
		{
			ValidationResult? validationResult = validationAttribute.GetValidationResult(value, validationContext);
			if (validationResult is not null)
				validationResults.Add(
					new RecursiveValidationResult(
						owner: owner,
						fullPath: MemberPathHelper.GetMemberPath(path),
						validationResult: validationResult!));
		}
	}

	private void ValidateEnumerableProperty(
		object owner,
		List<string> path,
		HashSet<object> alreadyValidated,
		List<RecursiveValidationResult> validationResults,
		out object? propertyValue)
	{
		// Validate the object itself in case it has Length validations etc
		ValidateObjectProperty(owner, path, alreadyValidated, validationResults, out propertyValue);

		int lastIndex = path.Count - 1;
		string originalLastPathValue = "";
		if (lastIndex == -1)
			path.Add("");
		else
			originalLastPathValue = path[lastIndex];

		try
		{
			var values = (IEnumerable?)PropertyInfo.GetValue(owner);
			if (values is null)
				return;

			int index = -1;
			foreach (object? value in values)
			{
				index++;
				if (value is not null)
				{
					path[lastIndex] = $"{originalLastPathValue}[{index}]";
					RecursiveValidator.ValidateObject(value, path, alreadyValidated, validationResults);
				}
			}
		}
		finally
		{
			if (lastIndex == -1)
				path.RemoveAt(0);
			else
				path[lastIndex] = originalLastPathValue;
		}
	}
}
