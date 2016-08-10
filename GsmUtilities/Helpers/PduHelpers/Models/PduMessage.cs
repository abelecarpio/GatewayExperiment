using System;

namespace GsmUtilities.Helpers.PduHelpers.Models
{
    public class PduMessage
    {

        public string ServiceCenterNumber
        {
            get { return LocalServiceCenterNumber; }
        }
        internal string LocalServiceCenterNumber { get; set; }

        public byte PduType
        {
            get { return LocalPduType; }
        }
        internal byte LocalPduType { get; set; }

        public bool ReplyPathExists
        {
            get { return LocalReplyPathExists; }
        }
        internal bool LocalReplyPathExists { get; set; }

        public bool UserDataStartsWithHeader
        {
            get { return LocalUserDataStartsWithHeader; }
        }
        internal bool LocalUserDataStartsWithHeader { get; set; }


        public bool StatusReportIndication
        {
            get { return LocalStatusReportIndication; }
        }
        internal bool LocalStatusReportIndication { get; set; }

        public bool MoreMessages
        {
            get{return LocalMoreMessages;}
        }
        internal bool LocalMoreMessages { get; set; }



        public ValidityPeriodFormat ValidityPeriodFormat
        {
            get { return LocalValidityPeriodFormat; }
        }
        internal ValidityPeriodFormat LocalValidityPeriodFormat { get; set; }


        public SmsDirection SmsDirection
        {
            get { return LocalSmsDirection; }
        }
        internal SmsDirection LocalSmsDirection { get; set; }

        public SmsType SmsType
        {
            get { return LocalSmsType; }
        }
        internal SmsType LocalSmsType { get; set; }

        public byte MessageReference
        {
            get { return LocalMessageReference; }
        }
        internal byte LocalMessageReference { get; set; }

        public string PhoneNumber
        {
            get { return LocalPhoneNumber; }
        }
        internal string LocalPhoneNumber { get; set; }

        public byte ProtocolIdentifier
        {
            get { return LocalProtocolIdentifier; }
        }
        internal byte LocalProtocolIdentifier { get; set; }

        public byte DataCodingSheme
        {
            get { return LocalDataCodingSheme; }
        }
        internal byte LocalDataCodingSheme { get; set; }

        public byte ValidityPeriod
        {
            get { return LocalValidityPeriod; }
        }
        internal byte LocalValidityPeriod { get; set; }

        public DateTime ServiceCenterTimeStamp
        {
            get { return LocalServiceCenterTimeStamp; }
        }
        internal DateTime LocalServiceCenterTimeStamp { get; set; }

        public string UserData
        {
            get { return LocalUserData; }
        }
        internal string LocalUserData { get; set; }


        public int MessageLength
        {
            get { return LocalMessageLength; }
        }
        internal int LocalMessageLength { get; set; }

        public byte[] UserDataHeader
        {
            get { return LocalUserDataHeader; }
        }
        internal byte[] LocalUserDataHeader { get; set; }


        public string Message
        {
            get { return LocalMessage; }
        }
        internal string LocalMessage { get; set; }
    }

    
}