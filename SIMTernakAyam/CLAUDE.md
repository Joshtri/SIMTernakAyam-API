

MAIN PROMPT : 

jadi saya sudah buat services, dan repository dan lainnya, jdi sekarang saya ingin anda lanjutkan lgi ke sisi controllers, jadi buatkan smua controlelrnya dan sudah ada referensinya yg kamu bisa ikut. dan jangan lupa add scoped di program.cs yaa

⚙️ Tabel Operasional
Tujuan: Mencatat kegiatan harian yang dilakukan di kandang.

- Contoh: pemberian pakan, vaksinasi, kebersihan, listrik, dll
- Ada info: tanggal, jumlah, petugas, kandang, jenis kegiatan
- Bisa dipakai untuk tracking aktivitas dan kebutuhan harian

💰 Tabel Biaya
Tujuan: Mencatat pengeluaran uang untuk setiap kegiatan.

- Contoh: beli pakan Rp500.000, vaksin Rp200.000, listrik Rp100.000
- Ada info: tanggal, nominal, jenis biaya, petugas
- Dipakai untuk laporan keuangan dan rekap pengeluaran

🌾 Tabel Pakan
Tujuan: Menyimpan daftar jenis pakan dan stoknya.

- Contoh: Pakan A, Pakan B, stok 100 kg
- Dipakai saat kegiatan operasional (pemberian pakan)

💉 Tabel Vaksin
Tujuan: Menyimpan daftar jenis vaksin dan stoknya.

- Contoh: Vaksin ND, Vaksin AI, stok 50 dosis
- Dipakai saat kegiatan operasional (vaksinasi)

🔄 Hubungan antar tabel

kegiatan Harian (Operasional)			Bisa pakai Pakan/Vaksin

Kegiatan butuh biaya			->		 Dicatat di Biaya
Semua kegiatan dilakukan oleh            Petugas di kandang


✨ Bedanya Operasional vs Biaya
Tabel				Fokus Utama						Contoh Data
Operasional			Aktivitas harian		"Pakan 10 kg di Kandang A oleh Petugas X"
Biaya				Pengeluaran uang		"Rp500.000 untuk beli pakan oleh Petugas X"




