using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Morris.DataAnnotations.ObjectTreeValidator;

public class RecursiveValidationResult : ValidationResult
{
	public string FullPath { get; }
	public object GetOwner() => Owner;

	[NonSerialized]
	private object Owner;

	public RecursiveValidationResult(
		object owner,
		string fullPath,
		ValidationResult validationResult)
		: base(validationResult)
	{
		Owner = owner ?? throw new ArgumentNullException(nameof(owner));
		FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
	}
}