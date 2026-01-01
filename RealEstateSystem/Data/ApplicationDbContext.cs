using System.Linq;
using Microsoft.EntityFrameworkCore;
using RealEstateSystem.Models;

namespace RealEstateSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // === DbSets ===

        public DbSet<User> Users { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<Buyer> Buyers { get; set; }

        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyImage> PropertyImages { get; set; }
        public DbSet<PropertyDocument> PropertyDocuments { get; set; }

        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Payment> Payments { get; set; }

        public DbSet<PropertyAnalytics> PropertyAnalytics { get; set; }
        public DbSet<SystemActivity> SystemActivities { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SearchHistory> SearchHistories { get; set; }
        public DbSet<ChatConversation> ChatConversations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<CommissionInvoice> CommissionInvoices { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChatConversation>()
            .HasIndex(c => new { c.UserAId, c.UserBId })
            .IsUnique();

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId);



            // ================================================================
            // COMMISSION INVOICE (One per Property)
            // ================================================================
            modelBuilder.Entity<CommissionInvoice>()
                .HasIndex(ci => ci.PropertyId)
                .IsUnique();

            modelBuilder.Entity<CommissionInvoice>()
                .HasOne(ci => ci.Property)
                .WithOne(p => p.CommissionInvoice)
                .HasForeignKey<CommissionInvoice>(ci => ci.PropertyId);

            modelBuilder.Entity<CommissionInvoice>()
                .HasOne(ci => ci.Seller)
                .WithMany()
                .HasForeignKey(ci => ci.SellerId);


            // ✅ Make ProfilePhoto optional in EF
            modelBuilder.Entity<User>()
                .Property(u => u.ProfilePhoto)
                .IsRequired(false);


            // Buyer: SavedSearchPreferences optional
            modelBuilder.Entity<Buyer>()
                .Property(b => b.SavedSearchPreferences)
                .IsRequired(false);

            // Seller: Agency fields optional
            modelBuilder.Entity<Seller>()
                .Property(s => s.AgencyName)
                .IsRequired(false);

            modelBuilder.Entity<Seller>()
                .Property(s => s.AgencyLicense)
                .IsRequired(false);

            modelBuilder.Entity<Seller>()
                .Property(s => s.AgencyAddress)
                .IsRequired(false);

            // ...rest of your existing configuration (relationships etc.)

            // ================================================================
            // PROPERTY – DECIMAL PRECISION (Latitude / Longitude)
            // ================================================================
            modelBuilder.Entity<Property>()
                .Property(p => p.Latitude)
                .HasPrecision(9, 6);

            modelBuilder.Entity<Property>()
                .Property(p => p.Longitude)
                .HasPrecision(9, 6);


            // ================================================================
            // USER ↔ SELLER (1–1)
            // ================================================================
            modelBuilder.Entity<User>()
                .HasOne(u => u.SellerProfile)
                .WithOne(s => s.User)
                .HasForeignKey<Seller>(s => s.UserId);

            // ================================================================
            // USER ↔ BUYER (1–1)
            // ================================================================
            modelBuilder.Entity<User>()
                .HasOne(u => u.BuyerProfile)
                .WithOne(b => b.User)
                .HasForeignKey<Buyer>(b => b.UserId);

            // ================================================================
            // SELLER – APPROVED BY (ADMIN USER)
            // ================================================================
            modelBuilder.Entity<Seller>()
                .HasOne(s => s.ApprovedByUser)
                .WithMany()
                .HasForeignKey(s => s.ApprovedBy);

            // ================================================================
            // PROPERTY ↔ SELLER (Many–1)
            // ================================================================
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Seller)
                .WithMany(s => s.Properties)
                .HasForeignKey(p => p.SellerId);

            // ================================================================
            // PROPERTY – APPROVED BY (ADMIN USER)
            // ================================================================
            modelBuilder.Entity<Property>()
                .HasOne(p => p.ApprovedByUser)
                .WithMany()
                .HasForeignKey(p => p.ApprovedBy);

            // ================================================================
            // WISHLIST
            // ================================================================
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Buyer)
                .WithMany(b => b.Wishlists)
                .HasForeignKey(w => w.BuyerId);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Property)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.PropertyId);

            // ================================================================
            // INQUIRY
            // ================================================================
            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.Buyer)
                .WithMany(b => b.Inquiries)
                .HasForeignKey(i => i.BuyerId);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.Seller)
                .WithMany(s => s.Inquiries)
                .HasForeignKey(i => i.SellerId);

            modelBuilder.Entity<Inquiry>()
                .HasOne(i => i.Property)
                .WithMany(p => p.Inquiries)
                .HasForeignKey(i => i.PropertyId);

            // ================================================================
            // OFFER
            // ================================================================
            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Property)
                .WithMany(p => p.Offers)
                .HasForeignKey(o => o.PropertyId);

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Buyer)
                .WithMany(b => b.Offers)
                .HasForeignKey(o => o.BuyerId);

            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Seller)
                .WithMany(s => s.Offers)
                .HasForeignKey(o => o.SellerId);

            // ================================================================
            // PAYMENT (1–1 with OFFER)
            // ================================================================
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Offer)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OfferId);

            // ================================================================
            // PROPERTY ANALYTICS
            // ================================================================
            modelBuilder.Entity<PropertyAnalytics>()
                .HasOne(a => a.Property)
                .WithMany(p => p.AnalyticsRecords)
                .HasForeignKey(a => a.PropertyId);

            // ================================================================
            // SYSTEM ACTIVITY
            // ================================================================
            modelBuilder.Entity<SystemActivity>()
                .HasOne(a => a.User)
                .WithMany(u => u.SystemActivities)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<SystemActivity>()
                .HasOne(a => a.PerformedByUser)
                .WithMany()
                .HasForeignKey(a => a.PerformedBy);

            // ================================================================
            // REPORTS
            // ================================================================
            modelBuilder.Entity<Report>()
                .HasOne(r => r.GeneratedByUser)
                .WithMany(u => u.ReportsGenerated)
                .HasForeignKey(r => r.GeneratedBy);

            // ================================================================
            // NOTIFICATIONS
            // ================================================================
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId);

            // ================================================================
            // SEARCH HISTORY
            // ================================================================
            modelBuilder.Entity<SearchHistory>()
                .HasOne(s => s.Buyer)
                .WithMany(b => b.SearchHistories)
                .HasForeignKey(s => s.BuyerId);

            // ================================================================
            // APPOINTMENTS
            // ================================================================
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Property)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PropertyId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Buyer)
                .WithMany(b => b.Appointments)
                .HasForeignKey(a => a.BuyerId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Seller)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.SellerId);

            // ================================================================
            // 🔥 GLOBAL FIX: DISABLE CASCADE DELETE EVERYWHERE
            // ================================================================
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict; // Same as NO ACTION
            }
        }
    }
}
