using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Tounaent_Fixtures.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Gender> Gender { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<TblTournament> TblTournament { get; set; }

    public virtual DbSet<TblTournamentUserReg> TblTournamentUserRegs { get; set; }
    public virtual DbSet<TblCategory> TblCategory { get; set; }

    public virtual DbSet<TblWeightCategory> TblWeightCategory { get; set; }

    public virtual DbSet<TblDistrict> TblDistricts { get; set; }

    public virtual DbSet<TblDistLocalClub> TblDistLocalClubs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:tournamentfixtures.database.windows.net,1433;Initial Catalog=TournamentFixutres;Persist Security Info=False;User ID=adminnew;Password=TestPassword@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblDistLocalClub>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Tbl_Dist_LocalClub");

            entity.Property(e => e.AddedBy)
                .HasMaxLength(50)
                .HasColumnName("Added_by");
            entity.Property(e => e.AddedDt)
                .HasColumnType("datetime")
                .HasColumnName("Added_dt");
            entity.Property(e => e.ClubId)
                .ValueGeneratedOnAdd()
                .HasColumnName("Club_id");
            entity.Property(e => e.DistictId).HasColumnName("Distict_id");
            entity.Property(e => e.LocalClubName)
                .HasMaxLength(100)
                .HasColumnName("Local_Club_Name");
            entity.Property(e => e.ModifyBy)
                .HasMaxLength(50)
                .HasColumnName("Modify_by");
            entity.Property(e => e.ModifyDt)
                .HasColumnType("datetime")
                .HasColumnName("Modify_dt");
            entity.Property(e => e.StateId).HasColumnName("State_id");
        });
        modelBuilder.Entity<TblWeightCategory>(entity =>
        {
            entity.HasKey(e => e.WeightCatId);
            entity.Property(e => e.WeightCatId).HasColumnName("Weight_Cat_id");
            entity.Property(e => e.WeightCatName).HasColumnName("Weight_Cat_Name");
            entity.Property(e => e.CatId).HasColumnName("Cat_id");
            entity.Property(e => e.ModifyBy).HasColumnName("Modify_by");
            entity.Property(e => e.AddedBy).HasColumnName("Added_by");
            entity.Property(e => e.ModifyDt).HasColumnName("Modify_dt");
            entity.Property(e => e.AddedDt).HasColumnName("Added_dt");


        });
        modelBuilder.Entity<TblCategory>(entity =>
        {
            entity.HasKey(e => e.CatId);

            entity.ToTable("Tbl_Category"); // match your table name exactly

            entity.Property(e => e.CatId).HasColumnName("Cat_id");
            entity.Property(e => e.CategoryName).HasColumnName("Category_Name");
            entity.Property(e => e.GenId).HasColumnName("Gen_id");
            entity.Property(e => e.IsActive).HasColumnName("IsActive");
            entity.Property(e => e.AddedDt).HasColumnName("Added_dt");
            entity.Property(e => e.AddedBy).HasColumnName("Added_by");
            entity.Property(e => e.ModifyDt).HasColumnName("Modify_dt");
            entity.Property(e => e.ModifyBy).HasColumnName("Modify_by");
        });
        modelBuilder.Entity<TblTournamentUserReg>(entity =>
        {
            entity.HasKey(e => e.TrUserId).HasName("PK__Tbl_Tour__49FBE2A9E5484551");

            entity.ToTable("Tbl_Tournament_User_Reg");

            entity.Property(e => e.TrUserId).HasColumnName("Tr_User_id");

            entity.Property(e => e.AddedBy)
                .HasMaxLength(50)
                .HasColumnName("Added_by");
            entity.Property(e => e.AddedDt)
                .HasColumnType("datetime")
                .HasColumnName("Added_dt");
            entity.Property(e => e.AdharNumb).HasMaxLength(500);
            entity.Property(e => e.CatId).HasColumnName("Cat_id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("Category_Name");
            entity.Property(e => e.ClubName).HasMaxLength(500);
            entity.Property(e => e.District).HasMaxLength(50);
            entity.Property(e => e.DistrictId).HasColumnName("District_id");
            entity.Property(e => e.Dob)
                .HasColumnType("datetime")
                .HasColumnName("DOB");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FatherName)
                .HasMaxLength(150)
                .HasColumnName("Father_Name");
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.MobileNo).HasMaxLength(150);
            entity.Property(e => e.ModifyBy)
                .HasMaxLength(50)
                .HasColumnName("Modify_by");
            entity.Property(e => e.ModifyDt)
                .HasColumnType("datetime")
                .HasColumnName("Modify_dt");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.TrId).HasColumnName("Tr_id");
            entity.Property(e => e.UserId).HasColumnName("User_id");
            entity.Property(e => e.WeighCatName)
                .HasMaxLength(50)
                .HasColumnName("Weigh_Cat_Name");
            entity.Property(e => e.Weight).HasColumnName("weight");
            entity.Property(e => e.WeightCatId).HasColumnName("Weight_Cat_id");
        });
        modelBuilder.Entity<TblTournament>(entity =>
        {
            entity.HasKey(e => e.TournamentId).HasName("PK__Tbl_Tour__E624E7C350BD6779");

            entity.ToTable("Tbl_Tournament");

            entity.Property(e => e.TournamentId).HasColumnName("Tournament_Id");
            entity.Property(e => e.AddedBy)
                .HasMaxLength(255)
                .HasColumnName("Added_by");
            entity.Property(e => e.AddedDt)
                .HasColumnType("datetime")
                .HasColumnName("Added_dt");
            entity.Property(e => e.DistictId).HasColumnName("Distict_id");
            entity.Property(e => e.DistictName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Distict_Name");
            entity.Property(e => e.FromDt)
                .HasColumnType("datetime")
                .HasColumnName("From_dt");
            entity.Property(e => e.ModifyBy)
                .HasMaxLength(255)
                .HasColumnName("Modify_by");
            entity.Property(e => e.ModifyDt)
                .HasColumnType("datetime")
                .HasColumnName("Modify_dt");
            entity.Property(e => e.OrganizedBy).HasMaxLength(255);
            entity.Property(e => e.ToDt)
                .HasColumnType("datetime")
                .HasColumnName("To_dt");
            entity.Property(e => e.TournamentName).HasMaxLength(255);
            entity.Property(e => e.Venue).HasMaxLength(255);
        });
        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.GenderId).HasName("PK__Gender__4E24E9F701341065");

            entity.ToTable("Gender");

            entity.Property(e => e.AddedBy)
                .HasMaxLength(50)
                .HasColumnName("Added_by");
            entity.Property(e => e.AddedDt)
                .HasColumnType("datetime")
                .HasColumnName("Added_dt");
            entity.Property(e => e.GenderName).HasMaxLength(50);
            entity.Property(e => e.ModifyBy)
                .HasMaxLength(50)
                .HasColumnName("Modify_by");
            entity.Property(e => e.ModifyDt)
                .HasColumnType("datetime")
                .HasColumnName("Modify_dt");
        });
        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("PK__Registra__6EF58810C0A6896F");

            entity.ToTable("Registration");

            entity.Property(e => e.Aadhaar).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(250);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Height).HasMaxLength(20);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.PinCode).HasMaxLength(10);
            entity.Property(e => e.Weight).HasMaxLength(20);

            entity.HasOne(d => d.Gender).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.GenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Gende__5FB337D6");
        });
        modelBuilder.Entity<TblDistrict>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Tbl_District");

            entity.Property(e => e.AddedBy)
                .HasMaxLength(50)
                .HasColumnName("Added_by");
            entity.Property(e => e.AddedDt)
                .HasColumnType("datetime")
                .HasColumnName("Added_dt");
            entity.Property(e => e.DistictId)
                .ValueGeneratedOnAdd()
                .HasColumnName("Distict_id");
            entity.Property(e => e.DistictName)
                .HasMaxLength(50)
                .HasColumnName("Distict_Name");
            entity.Property(e => e.ModifyBy)
                .HasMaxLength(50)
                .HasColumnName("Modify_by");
            entity.Property(e => e.ModifyDt)
                .HasColumnType("datetime")
                .HasColumnName("Modify_dt");
            entity.Property(e => e.StateId).HasColumnName("State_id");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
