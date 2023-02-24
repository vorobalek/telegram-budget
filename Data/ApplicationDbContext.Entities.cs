using Microsoft.EntityFrameworkCore;
using TelegramBudget.Data.Entities;

namespace TelegramBudget.Data;

public partial class ApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<TransactionConfirmation> TransactionConfirmations { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Participating> Participating { get; set; }
}