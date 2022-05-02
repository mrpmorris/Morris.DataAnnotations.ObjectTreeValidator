using Morris.DataAnnotations.ObjectTreeValidator.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Morris.DataAnnotations.ObjectTreeValidator;

internal static class ValidatorCache
{
	private readonly static ConcurrentDictionary<Type, PropertyValidator[]> TypeToPropertyValidatorsLookup = new();

	public static PropertyValidator[] CachePropertyValidators(Type type)
	{
		return TypeToPropertyValidatorsLookup.GetOrAdd(
			key: type,
			valueFactory: CreatePropertyValidators);
	}

	private static PropertyValidator[] CreatePropertyValidators(Type type)
	{
		if (type.Assembly == typeof(object).Assembly)
			return Array.Empty<PropertyValidator>();

		List<KeyValuePair<PropertyInfo, PropertyValidator?>> propertiesAndValidators =
			PropertyValidator
				.CreatePropertyValidators(type)
				.ToList();

		for (int o = propertiesAndValidators.Count - 1; o >= 0; o--)
		{
			KeyValuePair<PropertyInfo, PropertyValidator> item = propertiesAndValidators[o]!;
			// If a property has no validator then check if any properties (recursively)
			// need a validator
			if (item.Value is null)
			{
				bool hasSubValidators = HasSubValidators(item);

				if (!hasSubValidators)
				{
					// Remove the property if there are no subvalidtors
					propertiesAndValidators.RemoveAt(o);
				}
				else
				{
					// Add a no-operation validator so that the property's
					// sub validators are executed
					propertiesAndValidators[o] =
						new KeyValuePair<PropertyInfo, PropertyValidator?>(
							item.Key,
							PropertyValidator.CreateEmpty(item.Key));
				}
			}
		}
		return propertiesAndValidators.Select(x => x.Value!).ToArray();
	}

	private static bool HasSubValidators(KeyValuePair<PropertyInfo, PropertyValidator> item)
	{
		// Cache any validators on property type itself
		bool propertyTypeHasSubValidators = CachePropertyValidators(item.Key.PropertyType).Any();

		// If it's an IEnumerable<T> then cache properties on T also
		bool enumerableElementTypeHasSubValidators = false;
		Type? iGenericEnumerableElementType = item.Key.PropertyType.GetGenericEnumerableElementType();
		if (iGenericEnumerableElementType is not null)
			enumerableElementTypeHasSubValidators = CachePropertyValidators(iGenericEnumerableElementType).Any();

		return propertyTypeHasSubValidators || enumerableElementTypeHasSubValidators;
	}
}

