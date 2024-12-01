using CateringManagement.Models;
using CateringManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CateringManagement.Data
{
    public class CateringContext : DbContext
    {
        //To give access to IHttpContextAccessor for Audit Data with IAuditable
        private readonly IHttpContextAccessor _httpContextAccessor;

        //Property to hold the UserName value
        public string UserName
        {
            get; private set;
        }

        public CateringContext(DbContextOptions<CateringContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            if (_httpContextAccessor.HttpContext != null)
            {
                //We have a HttpContext, but there might not be anyone Authenticated
                UserName = _httpContextAccessor.HttpContext?.User.Identity.Name;
                UserName ??= "Unknown";
            }
            else
            {
                //No HttpContext so seeding data
                UserName = "Seed Data";
            }
        }
        public CateringContext(DbContextOptions<CateringContext> options) 
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<FunctionType> FunctionTypes { get; set; }
        public DbSet<Function> Functions { get; set; }
        public DbSet<MealType> MealTypes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<FunctionRoom> FunctionRooms { get; set; }
        public DbSet<Worker> Workers { get; set; }
        public DbSet<Work> Works { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<FunctionEquipment> FunctionEquipments { get; set; }
        public DbSet<FunctionDocument> FunctionDocuments { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<CustomerPhoto> CustomerPhotos { get; set; }
        public DbSet<CustomerThumbnail> CustomerThumbnails { get; set; }
        public DbSet<FunctionRevenueVM> FunctionRevenueSummary { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Add a unique index to Equipment Name 
            modelBuilder.Entity<Equipment>()
            .HasIndex(c => c.Name)
            .IsUnique();

            //For the FunctionRevenueVM ViewModel
            //Note: The Database View name is AppointmentSummaries
            modelBuilder
                .Entity<FunctionRevenueVM>()
                .ToView(nameof(FunctionRevenueSummary))
                .HasKey(a => a.ID);


            //Many to Many Intersection
            modelBuilder.Entity<FunctionRoom>()
            .HasKey(t => new { t.FunctionID, t.RoomID });

            //Many to Many Intersection
            modelBuilder.Entity<Work>()
            .HasKey(t => new { t.FunctionID, t.WorkerID });

            //NOT a Composite Primary Key but we want to 
            //guarantee that the combination of
            //the two foreing keys is unique
            modelBuilder.Entity<FunctionEquipment>()
                .HasIndex(e => new { e.EquipmentID, e.FunctionID })
                .IsUnique();



            //Prevent Cascade Delete from Worker to Work
            //so we are prevented from deleting a Worker with
            //Functions they have worked
            modelBuilder.Entity<Worker>()
                .HasMany<Work>(c => c.Works)
                .WithOne(f => f.Worker)
                .HasForeignKey(f => f.WorkerID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from Equipment to FunctionEquipment
            //so we are prevented from deleting a Equipment used at a 
            //Function.
            modelBuilder.Entity<Equipment>()
                .HasMany<FunctionEquipment>(c => c.FunctionEquipments)
                .WithOne(f => f.Equipment)
                .HasForeignKey(f => f.EquipmentID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from Customer to Function
            //so we are prevented from deleting a Customer with
            //Functions assigned
            modelBuilder.Entity<Customer>()
                .HasMany<Function>(c => c.Functions)
                .WithOne(f => f.Customer)
                .HasForeignKey(f => f.CustomerID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from FunctionType to Function
            //so we are prevented from deleting a FunctionType with
            //Functions assigned
            modelBuilder.Entity<FunctionType>()
                .HasMany<Function>(ft => ft.Functions)
                .WithOne(f => f.FunctionType)
                .HasForeignKey(f => f.FunctionTypeID)
                .OnDelete(DeleteBehavior.Restrict);

            //Prevent Cascade Delete from Room to FunctionRoom
            //so we are prevented from deleting a Room used for a Function
            modelBuilder.Entity<Room>()
                .HasMany<FunctionRoom>(ft => ft.FunctionRooms)
                .WithOne(f => f.Room)
                .HasForeignKey(f => f.RoomID)
                .OnDelete(DeleteBehavior.Restrict);

            //Add a unique index to the CustomerCode
            modelBuilder.Entity<Customer>()
            .HasIndex(c => c.CustomerCode)
            .IsUnique();
        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            OnBeforeSaving();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            OnBeforeSaving();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void OnBeforeSaving()
        {
            var entries = ChangeTracker.Entries();
            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable trackable)
                {
                    var now = DateTime.UtcNow;
                    switch (entry.State)
                    {
                        case EntityState.Modified:
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;

                        case EntityState.Added:
                            trackable.CreatedOn = now;
                            trackable.CreatedBy = UserName;
                            trackable.UpdatedOn = now;
                            trackable.UpdatedBy = UserName;
                            break;
                    }
                }
            }
        }

        //public DbSet<CateringManagement.Models.CustomerFunction> CustomerFunction { get; set; }
    }
}
