using CoherentMobile.Domain.Enums;

namespace CoherentMobile.Application.Helpers;

/// <summary>
/// Helper class for Emirates ID type detection and validation
/// </summary>
public static class EmiratesIdHelper
{
    /// <summary>
    /// Determines the Emirates ID type based on the ID pattern
    /// </summary>
    /// <param name="emiratesId">The Emirates ID string</param>
    /// <param name="hasPassport">Whether the user has a passport number instead</param>
    /// <returns>The corresponding EmiratesIdType enum value</returns>
    public static EmiratesIdType DetermineIdType(string? emiratesId, bool hasPassport = false)
    {
        // If passport number is provided instead
        if (hasPassport || string.IsNullOrEmpty(emiratesId))
        {
            return EmiratesIdType.Passport;
        }

        // Check for special patterns
        if (emiratesId.StartsWith("000-0000-0000000"))
        {
            return EmiratesIdType.NationalWithoutCard;
        }
        
        if (emiratesId.StartsWith("111-1111-1111111"))
        {
            return EmiratesIdType.ExpatriateWithoutCard;
        }
        
        if (emiratesId.StartsWith("222-2222-2222222"))
        {
            return EmiratesIdType.NonNationalWithoutCard;
        }
        
        if (emiratesId.StartsWith("999-9999-9999999"))
        {
            return EmiratesIdType.UnknownWithoutCard;
        }

        // Default: Standard Emirates ID (784-YYYY-NNNNNNN-N)
        return EmiratesIdType.Emirates;
    }

    /// <summary>
    /// Gets a user-friendly description of the Emirates ID type
    /// </summary>
    public static string GetIdTypeDescription(EmiratesIdType idType)
    {
        return idType switch
        {
            EmiratesIdType.Emirates => "Emirates ID",
            EmiratesIdType.Passport => "Passport",
            EmiratesIdType.NationalWithoutCard => "National without Emirates ID card",
            EmiratesIdType.ExpatriateWithoutCard => "Expatriate resident without Emirates ID card",
            EmiratesIdType.NonNationalWithoutCard => "Non-national resident without Emirates ID card",
            EmiratesIdType.UnknownWithoutCard => "Unknown status without Emirates ID card",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Validates if the Emirates ID matches the expected pattern for its type
    /// </summary>
    public static bool IsValidIdPattern(string? emiratesId, EmiratesIdType expectedType)
    {
        if (string.IsNullOrEmpty(emiratesId))
        {
            return expectedType == EmiratesIdType.Passport;
        }

        // Check standard Emirates ID format: XXX-YYYY-NNNNNNN-N
        var pattern = @"^\d{3}-\d{4}-\d{7}-\d{1}$";
        
        if (!System.Text.RegularExpressions.Regex.IsMatch(emiratesId, pattern))
        {
            return false;
        }

        var detectedType = DetermineIdType(emiratesId, false);
        return detectedType == expectedType;
    }

    /// <summary>
    /// Gets the string representation of EmiratesIdType for database storage
    /// </summary>
    public static string GetIdTypeString(EmiratesIdType idType)
    {
        return idType switch
        {
            EmiratesIdType.Emirates => "Emirates",
            EmiratesIdType.Passport => "Passport",
            EmiratesIdType.NationalWithoutCard => "National",
            EmiratesIdType.ExpatriateWithoutCard => "Expatriate",
            EmiratesIdType.NonNationalWithoutCard => "NonNational",
            EmiratesIdType.UnknownWithoutCard => "Unknown",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Parses a string to EmiratesIdType enum
    /// </summary>
    public static EmiratesIdType ParseIdTypeString(string? idTypeString)
    {
        return idTypeString?.ToLower() switch
        {
            "emirates" => EmiratesIdType.Emirates,
            "passport" => EmiratesIdType.Passport,
            "national" => EmiratesIdType.NationalWithoutCard,
            "expatriate" => EmiratesIdType.ExpatriateWithoutCard,
            "nonnational" => EmiratesIdType.NonNationalWithoutCard,
            "unknown" => EmiratesIdType.UnknownWithoutCard,
            _ => EmiratesIdType.UnknownWithoutCard
        };
    }
}
