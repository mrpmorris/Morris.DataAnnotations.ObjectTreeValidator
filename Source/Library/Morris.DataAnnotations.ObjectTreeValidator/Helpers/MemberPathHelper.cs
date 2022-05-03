using System;
using System.Collections.Generic;

namespace Morris.DataAnnotations.ObjectTreeValidator.Helpers;

internal static class MemberPathHelper
{
	public static string GetMemberPath(IEnumerable<string> path)
	{
		if (path is null)
			throw new ArgumentNullException(nameof(path));

		return string.Join(".", path);
	}
}