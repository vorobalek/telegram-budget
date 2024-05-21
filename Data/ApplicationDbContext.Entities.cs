using Microsoft.EntityFrameworkCore;
using TelegramBudget.Data.Entities;

namespace TelegramBudget.Data;

public partial class ApplicationDbContext
{
    public DbSet<User> User { get; set; }
    public DbSet<Transaction> Transaction { get; set; }
    public DbSet<TransactionConfirmation> TransactionConfirmation { get; set; }
    public DbSet<Budget> Budget { get; set; }
    public DbSet<Participant> Participant { get; set; }
}