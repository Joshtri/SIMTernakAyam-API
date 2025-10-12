namespace SIMTernakAyam.DTOs.Pakan
{
    public class PakanResponseDto
    {
        public Guid Id { get; set; }
        public string NamaPakan { get; set; } = string.Empty;
        public int Stok { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public static PakanResponseDto FromEntity(Models.Pakan pakan)
        {
            return new PakanResponseDto
            {
                Id = pakan.Id,
                NamaPakan = pakan.NamaPakan,
                Stok = pakan.Stok,
                CreatedAt = pakan.CreatedAt,
                UpdateAt = pakan.UpdateAt
            };
        }

        public static List<PakanResponseDto> FromEntities(IEnumerable<Models.Pakan> pakans)
        {
            return pakans.Select(FromEntity).ToList();
        }
    }
}