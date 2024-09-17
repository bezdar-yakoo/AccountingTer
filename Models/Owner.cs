using System.ComponentModel.DataAnnotations;

namespace AccountingTer.Models
{
    [Display(Name = "Пользователи")]
    public class Owner
    {
        public int Id { get; set; }
        [Display(Name = "Telegram логин")]
        public string TelegramLogin { get; set; }
        [Display(Name = "Описание")]
        public string Description { get; set; }
        [Display(Name = "Баланс")]
        public int Balance { get; set; }
    }
}
    