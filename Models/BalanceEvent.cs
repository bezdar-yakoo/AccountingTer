using System.ComponentModel.DataAnnotations;

namespace AccountingTer.Models
{
    public class BalanceEvent
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Сумма")]
        public int Value { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; } = "Описание отсутствует";

        [Display(Name = "Id владелеца счета")]
        public int OwnerBalanceId { get; set; } = -1;

        [Display(Name = "Добавлять или снимать сумму")]
        public bool IsAdded { get; set; }

        [Display(Name = "Id пользователя, который вызвал команду")]
        public int OwnerId { get; set; }
        [Display(Name = "Время вызова команды")]
        public DateTime DateTime { get; set; } = DateTime.Now;
    }
}