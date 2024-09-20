using System.ComponentModel.DataAnnotations;

namespace AccountingTer.Models
{
    [Display(Name = "Пользователи")]
    public class Owner
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Telegram логин")]
        public string UserName { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }

        [Display(Name = "Баланс")]
        public double Balance { get; set; }
    }
}
    