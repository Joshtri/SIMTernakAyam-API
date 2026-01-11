-- Migration: Update CASCADE DELETE for Ayam relations
-- Date: 2026-01-12
-- Description: Change foreign key constraints to ON DELETE CASCADE for Mortalitas and Panen

-- Drop existing foreign key constraints
ALTER TABLE "Mortalitas" DROP CONSTRAINT IF EXISTS "FK_Mortalitas_Ayams_AyamId";
ALTER TABLE "Panens" DROP CONSTRAINT IF EXISTS "FK_Panens_Ayams_AyamId";

-- Re-create foreign key constraints with ON DELETE CASCADE
ALTER TABLE "Mortalitas"
    ADD CONSTRAINT "FK_Mortalitas_Ayams_AyamId"
    FOREIGN KEY ("AyamId")
    REFERENCES "Ayams"("Id")
    ON DELETE CASCADE;

ALTER TABLE "Panens"
    ADD CONSTRAINT "FK_Panens_Ayams_AyamId"
    FOREIGN KEY ("AyamId")
    REFERENCES "Ayams"("Id")
    ON DELETE CASCADE;
