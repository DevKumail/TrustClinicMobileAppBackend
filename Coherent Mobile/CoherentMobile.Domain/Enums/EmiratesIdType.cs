namespace CoherentMobile.Domain.Enums;

/// <summary>
/// Emirates ID Type Enumeration
/// Based on UAE government ID patterns
/// </summary>
public enum EmiratesIdType
{
    /// <summary>
    /// Standard Emirates ID (784-YYYY-NNNNNNN-N)
    /// </summary>
    Emirates = 1,
    
    /// <summary>
    /// Passport Number
    /// </summary>
    Passport = 2,
    
    /// <summary>
    /// National without card (000-0000-0000000-0)
    /// </summary>
    NationalWithoutCard = 3,
    
    /// <summary>
    /// Expatriate resident without card (111-1111-1111111-1)
    /// </summary>
    ExpatriateWithoutCard = 4,
    
    /// <summary>
    /// Non-national, non-expat resident without card (222-2222-2222222-2)
    /// </summary>
    NonNationalWithoutCard = 5,
    
    /// <summary>
    /// Unknown status without card (999-9999-9999999-9)
    /// </summary>
    UnknownWithoutCard = 6
}
