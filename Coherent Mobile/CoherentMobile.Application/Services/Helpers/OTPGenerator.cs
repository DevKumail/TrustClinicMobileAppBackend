using System.Security.Cryptography;

namespace CoherentMobile.Application.Services.Helpers;

/// <summary>
/// OTP generation service
/// </summary>
public class OTPGenerator
{
    public static string Generate(int length = 6)
    {
        var randomNumber = new byte[4];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        var value = BitConverter.ToUInt32(randomNumber, 0);
        var otp = (value % (int)Math.Pow(10, length)).ToString($"D{length}");
        
        return otp;
    }
}
