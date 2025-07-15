namespace AdCampaignMVP.Models
{
    public class AdCampaign
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Budget { get; set; }
        public string CreatedByUserId { get; set; } = string.Empty;
    }
}
