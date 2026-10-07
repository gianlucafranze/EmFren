using EmFren.Models;
using Microsoft.EntityFrameworkCore;

namespace EmFren.Data
{
    public class EmFrenDbContext : DbContext
    {
        public EmFrenDbContext(
            DbContextOptions<EmFrenDbContext> options)
            : base(options)
        {
        }


        // =========================================================
        // TABLES
        // =========================================================

        public DbSet<User> Users { get; set; }

        public DbSet<PasswordResetToken>
            PasswordResetTokens
        { get; set; }

        public DbSet<CoinPayment>
            CoinPayments
        { get; set; }

        public DbSet<Department>
            Departments
        { get; set; }

        public DbSet<BranchClient>
            BranchClients
        { get; set; }

        public DbSet<Vehicle> Vehicles { get; set; }

        public DbSet<Staff> Staff { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Article> Articles { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderArticle> OrderArticles { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<NotificationUser>
            NotificationUsers
        { get; set; }

        // =========================================================
        // CLIENT NOTIFICATIONS
        // =========================================================

        public DbSet<NotificationClient>
            NotificationsClient
        { get; set; }

        public DbSet<NotificationUserClient>
            NotificationUsersClient
        { get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // =====================================================
            // USER
            // =====================================================

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.Username)
                    .HasColumnName("username")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Email)
                    .HasColumnName("email")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.PasswordHash)
                    .HasColumnName("passwordhash")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.FromEnterprise)
                    .HasColumnName("fromenterprise")
                    .IsRequired();
            });


            // =====================================================
            // PASSWORD RESET
            // =====================================================

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.ToTable("password_reset_tokens");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(x => x.Token)
                    .HasColumnName("token")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(x => x.ExpiresAt)
                    .HasColumnName("expires_at")
                    .IsRequired();

                entity.Property(x => x.Used)
                    .HasColumnName("used")
                    .IsRequired();

                entity.HasIndex(x => x.Token)
                    .IsUnique();

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasPrincipalKey(x => x.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // =====================================================
            // COIN PAYMENT
            // =====================================================

            modelBuilder.Entity<CoinPayment>(entity =>
            {
                entity.ToTable("coin_payment");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.CoinName)
                    .HasColumnName("coin_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(x => x.CoinName)
                    .IsUnique();
            });


            // =====================================================
            // DEPARTMENT
            // =====================================================

            modelBuilder.Entity<Department>(entity =>
            {
                entity.ToTable("department");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.DepartmentName)
                    .HasColumnName("department_name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(x => x.DepartmentName)
                    .IsUnique();
            });


            // =====================================================
            // BRANCH CLIENT
            // =====================================================

            modelBuilder.Entity<BranchClient>(entity =>
            {
                entity.ToTable("branch_client");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.CountryName)
                    .HasColumnName("country_name")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.City)
                    .HasColumnName("city")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.HasIndex(x => x.City)
                    .IsUnique();
            });


            // =====================================================
            // VEHICLE
            // =====================================================

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("vehicle");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.Brand)
                    .HasColumnName("brand")
                    .HasMaxLength(50)
                    .IsRequired();
            });


            // =====================================================
            // STAFF
            // =====================================================

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.ToTable("staff");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.DocumentNumber)
                    .HasColumnName("document_number")
                    .HasMaxLength(30);

                entity.Property(x => x.Email)
                    .HasColumnName("email")
                    .HasMaxLength(50);

                entity.Property(x => x.Ubicacion)
                    .HasColumnName("ubicacion")
                    .HasMaxLength(150);

                entity.Property(x => x.DepartmentId)
                    .HasColumnName("department_id");

                entity.Property(x => x.CellphoneNumber)
                    .HasColumnName("cellphone_number")
                    .HasMaxLength(30);

                entity.Property(x => x.StaffActive)
                    .HasColumnName("staff_active")
                    .HasDefaultValue(true);

                entity.HasOne(x => x.Department)
                    .WithMany()
                    .HasForeignKey(x => x.DepartmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // CLIENT
            // =====================================================

            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("client");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.CellphoneNumber)
                    .HasColumnName("cellphone_number")
                    .HasMaxLength(30);

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(50);

                entity.Property(e => e.DocumentNumber)
                    .HasColumnName("document_number")
                    .HasMaxLength(30);

                entity.Property(e => e.BranchClientId)
                    .HasColumnName("branch_client_id");

                entity.Property(e => e.ClientActive)
                    .HasColumnName("client_active")
                    .HasDefaultValue(true);

                entity.HasOne(e => e.BranchClient)
                    .WithMany()
                    .HasForeignKey(e => e.BranchClientId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // =====================================================
            // ARTICLE
            // =====================================================

            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("articles");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.Id)
                    .HasColumnName("id");

                entity.Property(a => a.ArticleId)
                    .HasColumnName("article_id")
                    .HasMaxLength(50);

                entity.Property(a => a.VehicleId)
                    .HasColumnName("vehicle_id");

                entity.HasOne(a => a.Vehicle)
                    .WithMany()
                    .HasForeignKey(a => a.VehicleId)
                    .HasConstraintName("fk_articles_vehicle")
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(a => a.Measure)
                    .HasColumnName("measure")
                    .HasMaxLength(50);

                entity.Property(a => a.Type)
                    .HasColumnName("type")
                    .HasMaxLength(100);

                entity.Property(a => a.Friction)
                    .HasColumnName("friction")
                    .HasMaxLength(100);

                entity.Property(a => a.Price)
                    .HasColumnName("price")
                    .HasColumnType("decimal(10,2)");

                entity.Property(a => a.Stock)
                    .HasColumnName("stock")
                    .HasDefaultValue(true);

                entity.Property(a => a.Discount)
                    .HasColumnName("discount")
                    .HasColumnType("decimal(5,2)")
                    .HasDefaultValue(0m);
            });


            // =====================================================
            // ORDER
            // =====================================================

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(x => x.DateOrder)
                    .HasColumnName("date_order")
                    .HasColumnType("date");

                entity.Property(x => x.CoinPaymentId)
                    .HasColumnName("coin_payment_id")
                    .IsRequired();

                entity.Property(x => x.TotalPurchase)
                    .HasColumnName("total_purchase")
                    .HasColumnType("decimal(12,2)");


                // =================================================
                // USER RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName("fk_orders_user")
                    .OnDelete(DeleteBehavior.Restrict);


                // =================================================
                // COIN PAYMENT RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.CoinPayment)
                    .WithMany()
                    .HasForeignKey(x => x.CoinPaymentId)
                    .HasConstraintName("fk_orders_coinpayment")
                    .OnDelete(DeleteBehavior.Restrict);


                // =================================================
                // ORDER ARTICLES
                // =================================================

                entity.HasMany(x => x.OrderArticles)
                    .WithOne(x => x.Order)
                    .HasForeignKey(x => x.IdOrder)
                    .HasConstraintName("fk_order_articles_order")
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // =====================================================
            // ORDER ARTICLE
            // =====================================================

            modelBuilder.Entity<OrderArticle>(entity =>
            {
                entity.ToTable("order_articles");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.IdOrder)
                    .HasColumnName("id_order")
                    .IsRequired();

                entity.Property(x => x.IdArticle)
                    .HasColumnName("id_article")
                    .IsRequired();

                entity.Property(x => x.Quantity)
                    .HasColumnName("quantity");


                // =================================================
                // ORDER RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.Order)
                    .WithMany(x => x.OrderArticles)
                    .HasForeignKey(x => x.IdOrder)
                    .HasConstraintName("fk_order_articles_order")
                    .OnDelete(DeleteBehavior.Cascade);


                // =================================================
                // ARTICLE RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.Article)
                    .WithMany()
                    .HasForeignKey(x => x.IdArticle)
                    .HasConstraintName("fk_order_articles_article")
                    .OnDelete(DeleteBehavior.Restrict);


                // =================================================
                // UNIQUE ORDER + ARTICLE
                // =================================================

                entity.HasIndex(x => new
                {
                    x.IdOrder,
                    x.IdArticle
                })
                .IsUnique()
                .HasDatabaseName(
                    "uq_order_articles_order_article"
                );
            });


            // =====================================================
            // NOTIFICATION
            // =====================================================

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.OrderId)
                    .HasColumnName("order_id");

                entity.Property(x => x.Title)
                    .HasColumnName("title")
                    .HasMaxLength(150)
                    .IsRequired();

                entity.Property(x => x.Message)
                    .HasColumnName("message")
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();


                // =================================================
                // ORDER RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.Order)
                    .WithMany()
                    .HasForeignKey(x => x.OrderId)
                    .HasConstraintName("fk_notifications_order")
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // =====================================================
            // NOTIFICATION USER
            // =====================================================

            modelBuilder.Entity<NotificationUser>(entity =>
            {
                entity.ToTable("notification_users");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.NotificationId)
                    .HasColumnName("notification_id")
                    .IsRequired();

                entity.Property(x => x.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(x => x.IsRead)
                    .HasColumnName("is_read")
                    .HasDefaultValue(false)
                    .IsRequired();


                // =================================================
                // NOTIFICATION RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.Notification)
                    .WithMany(x => x.NotificationUsers)
                    .HasForeignKey(x => x.NotificationId)
                    .HasConstraintName(
                        "fk_notification_users_notification"
                    )
                    .OnDelete(DeleteBehavior.Cascade);


                // =================================================
                // USER RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName(
                        "fk_notification_users_user"
                    )
                    .OnDelete(DeleteBehavior.Cascade);


                // =================================================
                // UNIQUE NOTIFICATION + USER
                // =================================================

                entity.HasIndex(x => new
                {
                    x.NotificationId,
                    x.UserId
                })
                .IsUnique()
                .HasDatabaseName(
                    "uq_notification_users_notification_user"
                );
            });


            // =====================================================
            // CLIENT NOTIFICATION
            // =====================================================

            modelBuilder.Entity<NotificationClient>(entity =>
            {
                entity.ToTable("notifications_client");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.UserId)
                    .HasColumnName("user_id");

                entity.Property(x => x.OrderId)
                    .HasColumnName("order_id");

                entity.Property(x => x.Title)
                    .HasColumnName("title")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(x => x.Message)
                    .HasColumnName("message")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(x => x.IsRead)
                    .HasColumnName("is_read")
                    .HasDefaultValue(false)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .HasColumnName("created_at")
                    .IsRequired();


                // =================================================
                // USER RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName(
                        "fk_notifications_client_user"
                    )
                    .OnDelete(DeleteBehavior.Cascade);


                // =================================================
                // ORDER RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.Order)
                    .WithMany()
                    .HasForeignKey(x => x.OrderId)
                    .HasConstraintName(
                        "fk_notifications_client_order"
                    )
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // =====================================================
            // CLIENT NOTIFICATION USER
            // =====================================================

            modelBuilder.Entity<NotificationUserClient>(entity =>
            {
                entity.ToTable("notification_users_client");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                entity.Property(x => x.NotificationId)
                    .HasColumnName("notification_id")
                    .IsRequired();

                entity.Property(x => x.UserId)
                    .HasColumnName("user_id")
                    .IsRequired();

                entity.Property(x => x.IsRead)
                    .HasColumnName("is_read")
                    .HasDefaultValue(false)
                    .IsRequired();


                // =================================================
                // NOTIFICATION RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.Notification)
                    .WithMany(x => x.NotificationUsersClient)
                    .HasForeignKey(x => x.NotificationId)
                    .HasConstraintName(
                        "fk_notification_users_client_notification"
                    )
                    .OnDelete(DeleteBehavior.Cascade);


                // =================================================
                // USER RELATIONSHIP
                // =================================================

                entity.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .HasConstraintName(
                        "fk_notification_users_client_user"
                    )
                    .OnDelete(DeleteBehavior.Cascade);


                // =================================================
                // UNIQUE NOTIFICATION + USER
                // =================================================

                entity.HasIndex(x => new
                {
                    x.NotificationId,
                    x.UserId
                })
                .IsUnique()
                .HasDatabaseName(
                    "uq_notification_users_client_notification_user"
                );
            });
        }
    }
}