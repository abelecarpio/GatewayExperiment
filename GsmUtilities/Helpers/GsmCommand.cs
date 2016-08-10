using System;

namespace GsmUtilities.Helpers
{
    internal static class GsmCommand
    {
        internal const string OK_RESPONSE = "OK";
        internal const string ERROR_RESPONSE = "ERROR";
        internal const string END_OF_LINE ="\r\n";
        internal const char CONTROL_Z = (char)26;
        internal const string START_MESSAGE_RESPONSE = @">";


        internal const string PLAIN_AT = @"AT" + END_OF_LINE;
        internal const string ENABLE_ERROR = @"AT+CMEE=2" + END_OF_LINE;
        internal const string GET_MANUFACTURER = @"AT+CGMI" + END_OF_LINE;
        internal const string GET_MODEL_NUMBER = @"AT+CGMM" + END_OF_LINE;
        internal const string GET_IMEI_NUMBER = @"AT+CGSN" + END_OF_LINE;
        internal const string GET_SOFTWARE_VERSION = @"AT+CGMR" + END_OF_LINE;
        internal const string GET_SIGNAL_STRENGTH = @"AT+CSQ" + END_OF_LINE;
        internal const string SET_MESSAGE_STORAGE = "AT+CPMS=";
        internal const string SET_PDU_MODE = @"AT+CMGF=0" + END_OF_LINE;

        internal const string GET_AVAILABLE_STORAGE = @"AT+CPMS=?" + END_OF_LINE;
        internal const string LIST_AVAILABLE_MESSAGES = @"AT+CMGL=4" + END_OF_LINE;


        internal const string SEND_MESSAGE_CMGW = @"AT+CMGW=";
        internal const string SEND_MESSAGE = @"AT+CMGS=";
        internal const string GET_MESSAGE_CENTER = @"AT+CSCA?" + END_OF_LINE;
        internal const string MESSAGE_CENTER_RESPONSE = "+CSCA";
        internal const string SET_MESSAGE_CENTER = @"AT+CSCA=";


        internal const string PUSH_MESSAGE = @"AT+CMSS=";
        internal const string PUSH_MESSAGE_RESPONSE = @"+CMSS";
    }
}