namespace TTtasarim.Web.Models
{
    public class FaturaViewModel
    {
        public string Id { get; set; }
        public string AboneNo { get; set; }
        public string SirketAdi { get; set; }
        public decimal Tutar { get; set; }
        public bool OdendiMi { get; set; }
        public DateTime OlusturulmaTarihi { get; set; }
    }
}
