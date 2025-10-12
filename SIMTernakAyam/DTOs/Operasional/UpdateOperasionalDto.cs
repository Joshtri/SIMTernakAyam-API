using System.ComponentModel.DataAnnotations;

namespace SIMTernakAyam.DTOs.Operasional
{
    public class UpdateOperasionalDto
    {
        [Required(ErrorMessage = "ID wajib diisi.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Jenis kegiatan ID wajib diisi.")]
        public Guid JenisKegiatanId { get; set; }

        [Required(ErrorMessage = "Tanggal wajib diisi.")]
        public DateTime Tanggal { get; set; }

        [Required(ErrorMessage = "Jumlah wajib diisi.")]
        [Range(1, int.MaxValue, ErrorMessage = "Jumlah harus lebih dari 0.")]
        public int Jumlah { get; set; }

        [Required(ErrorMessage = "Petugas ID wajib diisi.")]
        public Guid PetugasId { get; set; }

        [Required(ErrorMessage = "Kandang ID wajib diisi.")]
        public Guid KandangId { get; set; }

        public Guid? PakanId { get; set; }

        public Guid? VaksinId { get; set; }
    }
}