using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace RMS_API.Models
{
    public partial class RMS_SEP490Context : DbContext
    {
        public RMS_SEP490Context()
        {
        }

        public RMS_SEP490Context(DbContextOptions<RMS_SEP490Context> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; set; } = null!;
        public virtual DbSet<Building> Buildings { get; set; } = null!;
        public virtual DbSet<BuildingStatus> BuildingStatuses { get; set; } = null!;
        public virtual DbSet<District> Districts { get; set; } = null!;
        public virtual DbSet<FacilitiesStatus> FacilitiesStatuses { get; set; } = null!;
        public virtual DbSet<Facility> Facilities { get; set; } = null!;
        public virtual DbSet<Image> Images { get; set; } = null!;
        public virtual DbSet<MaintainanceRequest> MaintainanceRequests { get; set; } = null!;
        public virtual DbSet<Province> Provinces { get; set; } = null!;
        public virtual DbSet<Report> Reports { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Room> Rooms { get; set; } = null!;
        public virtual DbSet<RoomHistory> RoomHistories { get; set; } = null!;
        public virtual DbSet<RoomStatus> RoomStatuses { get; set; } = null!;
        public virtual DbSet<Service> Services { get; set; } = null!;
        public virtual DbSet<ServicesBill> ServicesBills { get; set; } = null!;
        public virtual DbSet<ServicesOfRoom> ServicesOfRooms { get; set; } = null!;
        public virtual DbSet<ServicesRecord> ServicesRecords { get; set; } = null!;
        public virtual DbSet<Tennant> Tennants { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserStatus> UserStatuses { get; set; } = null!;
        public virtual DbSet<Ward> Wards { get; set; } = null!;
        public object FacilityStatus { get; internal set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Address>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.DistrictId).HasColumnName("districtId");

                entity.Property(e => e.Information).HasColumnName("information");

                entity.Property(e => e.ProvinceId).HasColumnName("provinceId");

                entity.Property(e => e.WardId).HasColumnName("wardId");

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Addresses_Districts");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Addresses_Provinces");

                entity.HasOne(d => d.Ward)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.WardId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Addresses_Wards");
            });

            modelBuilder.Entity<Building>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AddressId).HasColumnName("addressId");

                entity.Property(e => e.BuildingStatusId).HasColumnName("buildingStatusId");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("date")
                    .HasColumnName("createdDate");

                entity.Property(e => e.Distance).HasColumnName("distance");

                entity.Property(e => e.DistrictId).HasColumnName("districtId");

                entity.Property(e => e.LinkEmbedMap).HasColumnName("linkEmbedMap");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.NumberOfRooms).HasColumnName("numberOfRooms");

                entity.Property(e => e.ProvinceId).HasColumnName("provinceId");

                entity.Property(e => e.TotalFloors).HasColumnName("totalFloors");

                entity.Property(e => e.UpdatedDate)
                    .HasColumnType("date")
                    .HasColumnName("updatedDate");

                entity.Property(e => e.UserId).HasColumnName("userId");

                entity.Property(e => e.WardId).HasColumnName("wardId");

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.AddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Buildings_Addresses");

                entity.HasOne(d => d.BuildingStatus)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.BuildingStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Buildings_BuildingStatus");

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Buildings_Districts");

                entity.HasOne(d => d.Province)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.ProvinceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Buildings_Provinces");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Buildings_Users");

                entity.HasOne(d => d.Ward)
                    .WithMany(p => p.Buildings)
                    .HasForeignKey(d => d.WardId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Buildings_Wards");
            });

            modelBuilder.Entity<BuildingStatus>(entity =>
            {
                entity.ToTable("BuildingStatus");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("date")
                    .HasColumnName("createdDate");

                entity.Property(e => e.LastModifiedDate)
                    .HasColumnType("date")
                    .HasColumnName("lastModifiedDate");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.ProvincesId).HasColumnName("provincesId");

                entity.HasOne(d => d.Provinces)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(d => d.ProvincesId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Districts_Provinces");
            });

            modelBuilder.Entity<FacilitiesStatus>(entity =>
            {
                entity.ToTable("FacilitiesStatus");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Status).HasColumnName("status");
            });

            modelBuilder.Entity<Facility>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FacilityStatusId).HasColumnName("facilityStatusId");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.RoomId).HasColumnName("roomId");

                entity.HasOne(d => d.FacilityStatus)
                    .WithMany(p => p.Facilities)
                    .HasForeignKey(d => d.FacilityStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Facilities_FacilitiesStatus");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Facilities)
                    .HasForeignKey(d => d.RoomId)
                    .HasConstraintName("FK_Facilities_Rooms");
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Link).HasColumnName("link");

                entity.Property(e => e.RoomId).HasColumnName("roomID");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_images_Rooms");
            });

            modelBuilder.Entity<MaintainanceRequest>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.RequestDate)
                    .HasColumnType("date")
                    .HasColumnName("requestDate");

                entity.Property(e => e.RoomId).HasColumnName("roomId");

                entity.Property(e => e.Status).HasColumnName("status");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.MaintainanceRequests)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_MaintainanceRequests_Rooms");
            });

            modelBuilder.Entity<Province>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("date")
                    .HasColumnName("createdDate");

                entity.Property(e => e.LastModifiedDate)
                    .HasColumnType("date")
                    .HasColumnName("lastModifiedDate");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Report>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Description).HasColumnName("description");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Area).HasColumnName("area");

                entity.Property(e => e.BuildingId).HasColumnName("buildingId");

                entity.Property(e => e.Description)
                    .HasMaxLength(100)
                    .HasColumnName("description");

                entity.Property(e => e.ExpiredDate)
                    .HasColumnType("date")
                    .HasColumnName("expiredDate");

                entity.Property(e => e.Floor).HasColumnName("floor");

                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");

                entity.Property(e => e.RoomNumber).HasColumnName("roomNumber");

                entity.Property(e => e.RoomStatusId).HasColumnName("roomStatusId");

                entity.Property(e => e.StartedDate)
                    .HasColumnType("date")
                    .HasColumnName("startedDate");

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Rooms_Buildings");

                entity.HasOne(d => d.RoomStatus)
                    .WithMany(p => p.Rooms)
                    .HasForeignKey(d => d.RoomStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Rooms_RoomStatus");
            });

            modelBuilder.Entity<RoomHistory>(entity =>
            {
                entity.ToTable("RoomHistory");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CustomerName)
                    .HasMaxLength(100)
                    .HasColumnName("customerName");

                entity.Property(e => e.EndDate)
                    .HasColumnType("date")
                    .HasColumnName("endDate");

                entity.Property(e => e.RoomId).HasColumnName("roomId");

                entity.Property(e => e.StartDate)
                    .HasColumnType("date")
                    .HasColumnName("startDate");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.RoomHistories)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoomHistory_Rooms");
            });

            modelBuilder.Entity<RoomStatus>(entity =>
            {
                entity.ToTable("RoomStatus");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");
            });

            modelBuilder.Entity<ServicesBill>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Date)
                    .HasColumnType("date")
                    .HasColumnName("date");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Price)
                    .HasColumnType("money")
                    .HasColumnName("price");

                entity.Property(e => e.ServiceId).HasColumnName("serviceId");

                entity.Property(e => e.ServiceRecordId).HasColumnName("serviceRecordId");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.ServicesBills)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ServicesBills_Services");

                entity.HasOne(d => d.ServiceRecord)
                    .WithMany(p => p.ServicesBills)
                    .HasForeignKey(d => d.ServiceRecordId)
                    .HasConstraintName("FK_ServicesBills_ServicesRecord");
            });

            modelBuilder.Entity<ServicesOfRoom>(entity =>
            {
                // Định nghĩa khóa chính kết hợp là RoomId và ServiceId
                entity.HasKey(e => new { e.RoomId, e.ServiceId });

                entity.ToTable("ServicesOfRoom");

                entity.Property(e => e.RoomId).HasColumnName("roomId");

                entity.Property(e => e.ServiceId).HasColumnName("serviceId");

                entity.HasOne(d => d.Room)
                    .WithMany()
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ServicesOfRoom_Rooms");

                entity.HasOne(d => d.Service)
                    .WithMany()
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ServicesOfRoom_Services");
            });

            modelBuilder.Entity<ServicesRecord>(entity =>
            {
                entity.ToTable("ServicesRecord");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.NewMeter).HasColumnName("newMeter");

                entity.Property(e => e.OldMeter).HasColumnName("oldMeter");

                entity.Property(e => e.ServiceId).HasColumnName("serviceId");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.ServicesRecords)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ServicesRecord_Services");
            });

            modelBuilder.Entity<Tennant>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .HasColumnName("email");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.Property(e => e.Phone).HasColumnName("phone");

                entity.Property(e => e.RoomId).HasColumnName("roomId");

                entity.HasOne(d => d.Room)
                    .WithMany(p => p.Tennants)
                    .HasForeignKey(d => d.RoomId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tennants_Rooms");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("email");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasColumnName("firstName");

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasColumnName("lastName");

                entity.Property(e => e.MidName)
                    .HasMaxLength(50)
                    .HasColumnName("midName");

                entity.Property(e => e.Password)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("password");

                entity.Property(e => e.Phone)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("phone");

                entity.Property(e => e.RoleId).HasColumnName("roleId");

                entity.Property(e => e.UserStatusId).HasColumnName("userStatusId");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_Roles");

                entity.HasOne(d => d.UserStatus)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UserStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_UserStatus");
            });

            modelBuilder.Entity<UserStatus>(entity =>
            {
                entity.ToTable("UserStatus");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Ward>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("date")
                    .HasColumnName("createdDate");

                entity.Property(e => e.DistrictId).HasColumnName("districtId");

                entity.Property(e => e.LastModifiedDate)
                    .HasColumnType("date")
                    .HasColumnName("lastModifiedDate");

                entity.Property(e => e.Name)
                    .HasMaxLength(100)
                    .HasColumnName("name");

                entity.HasOne(d => d.District)
                    .WithMany(p => p.Wards)
                    .HasForeignKey(d => d.DistrictId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Wards_Districts");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
