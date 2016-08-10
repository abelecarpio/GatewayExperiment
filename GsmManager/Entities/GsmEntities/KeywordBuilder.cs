namespace GsmManager.Entities.GsmEntities
{
    public class KeywordBuilder
    {
        public int KeywordId { get; set; }
        public string Keyword { get; set; }
        public string Spiel { get; set; }
        public bool IsActive { get; set; }
        public bool EnableCallback { get; set; }

        //[System.ComponentModel.Browsable(false)]
        public int ModemId { get; set; }
    }
}