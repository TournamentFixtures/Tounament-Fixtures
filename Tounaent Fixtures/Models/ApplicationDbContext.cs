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


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:tournamentfixtures.database.windows.net,1433;Initial Catalog=TournamentFixutres;Persist Security Info=False;User ID=adminnew;Password=TestPassword@123;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblTournament>(entity =>
        {
            entity.ToTable("Tbl_Tournament"); 

            entity.HasKey(e => e.TournamentId);
            entity.Property(e => e.TournamentId).ValueGeneratedOnAdd()
            .HasColumnName("Tournament_Id"); 
            entity.Property(e => e.FromDt)
              .HasColumnName("From_dt");
            entity.Property(e => e.AddedBy)
              .HasColumnName("Added_by");
            entity.Property(e => e.AddedDt)
              .HasColumnName("Added_dt");
            entity.Property(e => e.ModifyBy)
              .HasColumnName("Modify_by");
            entity.Property(e => e.ModifyDt)
              .HasColumnName("Modify_dt");
            entity.Property(e => e.ToDt)
              .HasColumnName("To_dt");
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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
