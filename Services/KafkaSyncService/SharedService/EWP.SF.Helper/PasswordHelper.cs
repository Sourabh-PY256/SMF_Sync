using System.Buffers;

namespace EWP.SF.Helper;

/// <summary>
/// PasswordStrength is an enumeration that represents the strength of a password.
/// </summary>
public enum PasswordStrength
{
	/// <summary>
	/// Blank Password (empty and/or space chars only)
	/// </summary>
	Blank = 0,

	/// <summary>
	/// Either too short (less than 5 chars), one-case letters only or digits only
	/// </summary>
	VeryWeak = 1,

	/// <summary>
	/// At least 5 characters, one strong condition met (>= 8 chars with 1 or more UC letters, LC letters, digits &amp; special chars)
	/// </summary>
	Weak = 2,

	/// <summary>
	/// At least 5 characters, two strong conditions met (>= 8 chars with 1 or more UC letters, LC letters, digits &amp; special chars)
	/// </summary>
	Medium = 3,

	/// <summary>
	/// At least 8 characters, three strong conditions met (>= 8 chars with 1 or more UC letters, LC letters, digits &amp; special chars)
	/// </summary>
	Strong = 4,

	/// <summary>
	/// At least 8 characters, all strong conditions met (>= 8 chars with 1 or more UC letters, LC letters, digits &amp; special chars)
	/// </summary>
	VeryStrong = 5
}

/// <summary>
/// PasswordHelper is a static class that provides methods to evaluate password strength and validate passwords based on various criteria.
/// </summary>
public static class PasswordHelper
{
	private static readonly SearchValues<char> s_myChars = SearchValues.Create("!@#$%^&*?_~-£().,");

	/// <summary>
	/// Generic method to retrieve password strength: use this for general purpose scenarios,
	/// i.e. when you don't have a strict policy to follow.
	/// </summary>
	/// <param name="password"></param>
	/// <returns></returns>
	public static PasswordStrength GetPasswordStrength(string password)
	{
		int score = 0;
		if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(password.Trim()))
		{
			return PasswordStrength.Blank;
		}

		if (!HasMinimumLength(password, 5))
		{
			return PasswordStrength.VeryWeak;
		}
		else
		{
			score++;
		}

		if (HasMinimumLength(password, 8))
		{
			score++;
		}

		if (HasUpperCaseLetter(password) && HasLowerCaseLetter(password))
		{
			score++;
		}

		if (HasDigit(password))
		{
			score++;
		}

		if (HasSpecialChar(password))
		{
			score++;
		}

		return (PasswordStrength)score;
	}

	/// <summary>
	/// Sample password policy implementation:
	/// - minimum 8 characters
	/// - at lease one UC letter
	/// - at least one LC letter
	/// - at least one non-letter char (digit OR special char)
	/// </summary>
	/// <returns></returns>
	public static bool IsStrongPassword(string password)
	{
		return HasMinimumLength(password, 8)
			&& HasUpperCaseLetter(password)
			&& HasLowerCaseLetter(password)
			&& (HasDigit(password) || HasSpecialChar(password));
	}

	/// <summary>
	/// Sample password policy implementation following the Microsoft.AspNetCore.Identity.PasswordOptions standard.
	/// </summary>
	public static bool IsValidPassword(
		string password,
		int requiredLength,
		int requiredUniqueChars,
		bool requireNonAlphanumeric,
		bool requireLowercase,
		bool requireUppercase,
		bool requireDigit)
	{
		if (!HasMinimumLength(password, requiredLength))
		{
			return false;
		}

		if (!HasMinimumUniqueChars(password, requiredUniqueChars))
		{
			return false;
		}

		if (requireNonAlphanumeric && !HasSpecialChar(password))
		{
			return false;
		}

		if (requireLowercase && !HasLowerCaseLetter(password))
		{
			return false;
		}

		if (requireUppercase && !HasUpperCaseLetter(password))
		{
			return false;
		}

		return !requireDigit || HasDigit(password);
	}

	#region Helper Methods

	/// <summary>
	/// Returns TRUE if the password has at least the minimum length
	/// </summary>
	public static bool HasMinimumLength(string password, int minLength)
	{
		return password.Length >= minLength;
	}

	/// <summary>
	/// Returns TRUE if the password has at least the minimum number of unique characters
	/// </summary>
	public static bool HasMinimumUniqueChars(string password, int minUniqueChars)
	{
		return password.Distinct().Count() >= minUniqueChars;
	}

	/// <summary>
	/// Returns TRUE if the password has at least one digit
	/// </summary>
	public static bool HasDigit(string password)
	{
		return password.Any(char.IsDigit);
	}

	/// <summary>
	/// Returns TRUE if the password has at least one special character
	/// </summary>
	public static bool HasSpecialChar(string password)
	{
		// return password.Any(c => char.IsPunctuation(c)) || password.Any(c => char.IsSeparator(c)) || password.Any(c => char.IsSymbol(c));
		return password.AsSpan().IndexOfAny(s_myChars) != -1;
	}

	/// <summary>
	/// Returns TRUE if the password has at least one uppercase letter
	/// </summary>
	public static bool HasUpperCaseLetter(string password)
	{
		return password.Any(char.IsUpper);
	}

	/// <summary>
	/// Returns TRUE if the password has at least one lowercase letter
	/// </summary>
	public static bool HasLowerCaseLetter(string password)
	{
		return password.Any(char.IsLower);
	}

	#endregion Helper Methods
}
