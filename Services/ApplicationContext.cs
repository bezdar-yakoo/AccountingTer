using AccountingTer.Models;
using AccountingTer.TelegramExtentions;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace AccountingTer.Services
{
    public class ApplicationContext : DbContext
    {
        public DbSet<BalanceEvent> BalanceEvents { get; set; }
        public DbSet<Owner> Owners { get; set; }
        public DbSet<StringProperties> StringProperties { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<StringProperties>().HasData(
                 new StringProperties() { Id = 1, Key = Debug.IdsForBackupDataBase, Value = "475031431",Description = "ID чатов, в которые бот будет присылать дамп базы данных" },
                 new StringProperties() { Id = 2, Key = Debug.DailyStatisticHour, Value = "23", Description = "час, когда надо присылать статистику и дамп базы данных" },
                 new StringProperties() { Id = 3, Key = Debug.ChatsForStatistic, Value = "475031431", Description = "ID чатов, в которые бот будет писать статистику в конце дня" },
                 new StringProperties() { Id = 4, Key = Debug.BybitCredentials, Value = "JK3uhAcX7Zh7Puhtbz:aTTCR8cH8ttm4v4lMypDll9FCGExHUsEVHEF", Description = "Ключ и секрет от апи в формате key:secret" },
                 new StringProperties() { Id = 5, Key = Debug.Url, Value = "https://google.com", Description = "Ссылка на веб интерфейс" }
                 //,new StringProperties() { Key = "", Value = ""},
                 //new StringProperties() { Key = "", Value = ""},
                 //new StringProperties() { Key = "", Value = ""}
                 );
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }


    }
}
