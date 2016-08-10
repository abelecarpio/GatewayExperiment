using System;

namespace GsmUtilities.Helpers.PduHelpers
{
    public enum ValidityPeriodFormat
    {
        FieldNotPresent = 0,
        Relative = 0x10,
        Enhanced = 0x08,
        Absolute = 0x18
    }

    public enum SmsDirection
    {
        Received = 0,
        Submited = 1
    }

    public enum SmsType
    {
        Sms = 0,
        StatusReport = 1
    }

    [Flags]
    public enum SmsEncoding
    {
        ReservedMask = 0x0C /*1100*/,
        SevenBit = 0,
        EightBit = 0x04 /*0100*/,
        Ucs2 = 0x08 /*1000*/
    }
}