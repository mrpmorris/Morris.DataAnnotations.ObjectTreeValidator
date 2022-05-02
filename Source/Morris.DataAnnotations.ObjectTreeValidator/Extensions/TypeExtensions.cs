using System;
using System.Collections.Generic;
using System.Linq;

namespace Morris.DataAnnotations.ObjectTreeValidator.Extensions;

internal static class TypeExtensions
{
	public static Type? GetGenericEnumerableElementType(this Type type)
	{
		Type? iEnumerableElementType = null;

		if (type.IsGenericType)
			iEnumerableElementType = type
				.GetInterfaces()
				.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				.FirstOrDefault()
				?.GetGenericArguments()
				?.FirstOrDefault();

		if (iEnumerableElementType is not null && iEnumerableElementType.Assembly != typeof(object).Assembly)
			return iEnumerableElementType;
		return null;
	}
}