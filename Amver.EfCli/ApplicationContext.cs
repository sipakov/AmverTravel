using System;
using Amver.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Amver.EfCli
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext()
        {
        }

        public ApplicationContext(int timeOut)
        {
            Database.SetCommandTimeout(timeOut);
        }

        public ApplicationContext(DbContextOptions options)
            : base(options)
        {
          //  Database.EnsureCreated();
        }

        private static readonly ILoggerFactory Factory
            = LoggerFactory.Create(builder => { builder.AddConsole(); });
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var appConnectionString = Configurator.GetConfiguration().GetConnectionString("PostgreSqlConnection");
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseLoggerFactory(Factory).UseNpgsql(appConnectionString);
            }
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<FavouriteTrip> FavouriteTrips { get; set; }

        public DbSet<UserAuthentication> UserAuthentications { get; set; }
        public DbSet<ObjectionableContent> ObjectionableContents { get; set; }
        public DbSet<UserToFcmToken> UserToFcmTokens { get; set; }
        public DbSet<UserToBlockedUser> UserToBlockedUsers { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureUsersModelCreation(modelBuilder);
            ConfigureTripsModelCreation(modelBuilder);
            ConfigureCitiesModelCreation(modelBuilder);
            ConfigureConversationsModelCreation(modelBuilder);
            ConfigureFavouriteTripsModelCreation(modelBuilder);
            ConfigureUserAuthenticationsModelCreation(modelBuilder);
            ConfigureMessageModelCreation(modelBuilder);
            ConfigureCountriesModelCreation(modelBuilder);
            ConfigureObjectionableContentModelCreation(modelBuilder);
            ConfigureUserToFcmTokenModelCreation(modelBuilder);
            ConfigureUserToBlockedUserModelCreation(modelBuilder);
        }
        
        private static void ConfigureUsersModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.Entity<User>().HasKey(x => x.Id);
            modelBuilder.Entity<User>().HasIndex(x => x.CityId);
            modelBuilder.Entity<User>().HasIndex(x => x.Login).IsUnique();
            modelBuilder.Entity<User>().Property(x => x.Login).IsRequired();
            modelBuilder.Entity<User>().Property(x => x.Login).HasMaxLength(50);
            modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(50);
            modelBuilder.Entity<User>().Property(x => x.PhoneNumber).HasMaxLength(50);
            modelBuilder.Entity<User>().Property(x => x.FirstName).HasMaxLength(50);
            modelBuilder.Entity<User>().Property(x => x.LastName).HasMaxLength(50);
            modelBuilder.Entity<User>().Property(x => x.Comment).HasMaxLength(500);
        }
        
        private static void ConfigureTripsModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.Entity<Trip>().HasKey(x => x.Id);
            modelBuilder.Entity<Trip>().HasIndex(x => x.FromCountryId);
            modelBuilder.Entity<Trip>().HasIndex(x => x.FromCityId);
            modelBuilder.Entity<Trip>().Property(x => x.FromCityId).IsRequired();
            modelBuilder.Entity<Trip>().HasIndex(x => x.ToCountryId);
            modelBuilder.Entity<Trip>().Property(x => x.ToCountryId).IsRequired();
            modelBuilder.Entity<Trip>().HasIndex(x => x.ToCityId);
            modelBuilder.Entity<Trip>().HasIndex(x => x.DateFrom);
            modelBuilder.Entity<Trip>().Property(x => x.DateFrom).IsRequired();
            modelBuilder.Entity<Trip>().HasIndex(x => x.DateTo);
            modelBuilder.Entity<Trip>().Property(x => x.DateTo).IsRequired();
            modelBuilder.Entity<Trip>().HasIndex(x => x.UserId);
            modelBuilder.Entity<Trip>().Property(x => x.UserId).IsRequired();

            modelBuilder.Entity<Trip>().HasIndex(x => new
                {x.UserId, x.FromCityId, x.ToCityId, x.DateFrom, x.DateTo, x.IsDeleted}).IsUnique();
            modelBuilder.Entity<Trip>().Property(x => x.Comment).HasMaxLength(500);
        }
        
        private static void ConfigureCitiesModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.Entity<City>().HasKey(x => new { x.Id});
            modelBuilder.Entity<City>().HasIndex(x => x.CountryId);
            modelBuilder.Entity<City>().Property(x => x.ruRu).HasMaxLength(200);
        }
        
        private static void ConfigureConversationsModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            
            modelBuilder.Entity<Conversation>().HasKey(x => new {x.Id});
            modelBuilder.Entity<Conversation>().HasIndex(x => new {x.TripId, x.UserId});
        }
        
        private static void ConfigureFavouriteTripsModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<FavouriteTrip>().HasKey(x => new {x.UserId, x.TripId});
        }
        
        private static void ConfigureUserAuthenticationsModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));

            modelBuilder.Entity<UserAuthentication>().HasIndex(x=>x.UserId).IsUnique();
        }
        
        private static void ConfigureMessageModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            
            modelBuilder.Entity<Message>().HasKey(x => new { x.SaveDate, x.Sender, x.ConversationId});
            modelBuilder.Entity<Message>().HasIndex(x=>x.ConversationId);
            modelBuilder.Entity<Message>().Property(x => x.MessageStr).HasMaxLength(500);
        }
        private static void ConfigureCountriesModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.Entity<Country>().HasKey(x => new { x.Id});
            modelBuilder.Entity<Country>().Property(x => x.ruRu).HasMaxLength(200);
        }

        private static void ConfigureObjectionableContentModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.Entity<ObjectionableContent>().HasKey(x => new { x.Id});
            modelBuilder.Entity<ObjectionableContent>().Property(x=>x.Comment).HasMaxLength(500);
        }
        
        private static void ConfigureUserToFcmTokenModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.Entity<UserToFcmToken>().Property(x=>x.FcmToken).HasMaxLength(500);
            modelBuilder.Entity<UserToFcmToken>().HasKey(x => new { x.UserId, x.FcmToken});
        }
        
        private static void ConfigureUserToBlockedUserModelCreation(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            modelBuilder.Entity<UserToBlockedUser>().HasKey(x => new { x.UserId, x.BlockedUserId});
        }
    }
}
