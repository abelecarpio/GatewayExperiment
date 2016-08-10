namespace GsmUtilities.Models
{
    public class ModemPreference
    {
        public string ComPort { get; set; }
        public int BaudRate { get; set; }
        public string Imei { get; set; }
        public string Manufacturer { get; set; }
        public string ModemModel { get; set; }

        public string FriendlyName
        {
            get
            {
                return string.Format("{0}{1}{2}{3}",
                    string.IsNullOrEmpty(Manufacturer) ? string.Empty : Manufacturer.Trim().ToUpper() + " ",
                    string.IsNullOrEmpty(ModemModel) ? string.Empty : "- " + ModemModel.Trim() + " ",
                    string.IsNullOrEmpty(Imei) ? string.Empty : "[" + Imei.Trim() + "] ",
                    string.IsNullOrEmpty(ComPort) ? string.Empty : "(" + ComPort.Trim() + ")"
                    );
            }
        }
    }
}