using FinPal.Expense.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace FinPal.Expense.Api.Data;
public class FinPalDbContext : DbContext
{
    public FinPalDbContext(DbContextOptions<FinPalDbContext> options) : base(options)
    {        

    }
    
    //Declaring the tables 
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Expenses> Expenses => Set<Expenses>();
    public DbSet<Budget> Budgets => Set<Budget>();

    //Declaring the table constraints
    protected override void OnModelCreating(ModelBuilder modelBuilder) //Overriding virtual method from DbContext class
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.UserID);
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
            entity.Property(u => u.IsActive).HasDefaultValue(true);
            entity.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UQ_Users_Email");

        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(c => c.CategoryId);
            entity.Property(c => c.CategoryName).HasMaxLength(100).IsRequired();
            entity.Property(c => c.IsActive).HasDefaultValue(true);
            entity.HasOne(c => c.User).WithMany(u => u.Categories).HasForeignKey(c => c.UserId).HasConstraintName("FK_Categories_Users").OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(c => new { c.UserId, c.CategoryName }).IsUnique().HasDatabaseName("UQ_Categories_UserID_CategoryName");
            entity.HasIndex(c => new { c.UserId, c.CategoryId }).IsUnique().HasDatabaseName("UQ_Categories_UserID_CategoryID");
            entity.HasAlternateKey(c => new { c.UserId, c.CategoryId }).HasName("AK_Catgories_UserID_CategoryID");

        });

        modelBuilder.Entity<Expenses>(entity =>
        {
            entity.ToTable("Expenses");
            entity.HasKey(e => e.ExpenseId);
            entity.Property(e => e.Amount).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.ExpenseDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.HasOne(e => e.Category).WithMany(c => c.Expenses).HasForeignKey(e => new { e.UserId, e.CategoryId }).HasConstraintName("FK_Expenses_UserID_CategoryID").HasPrincipalKey(c => new {c.UserId, c.CategoryId}).OnDelete(DeleteBehavior.Restrict);

        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("Budgets");
            entity.HasKey(b => b.BudgetId);
            entity.Property(b => b.BudgetAmount).HasColumnType("decimal(10, 2)").IsRequired();
            entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
            entity.HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year }).IsUnique().HasDatabaseName("UQ_Budgets_User_Category_Month_Year");
            entity.HasOne(b => b.Category).WithMany(c => c.Budgets).HasForeignKey(b => new { b.UserId, b.CategoryId }).HasConstraintName("FK_Budgets_UserID_CategoryID").HasPrincipalKey(c => new {c.UserId, c.CategoryId}).OnDelete(DeleteBehavior.Restrict);
            entity.HasCheckConstraint("CK_Budgets_Month_6FE99F9F", "[Month] Between 1 and 12");

        });

        base.OnModelCreating(modelBuilder);

    }
}