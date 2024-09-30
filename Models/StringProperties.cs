using System.ComponentModel.DataAnnotations;

namespace AccountingTer.Models
{
    public class StringProperties
    {
        [Key]
        public int Id { get; set; }
        [Display(Name ="Ключ значение", Description ="Используется для поиска значения в базе данных")]
        public string  Key { get; set; }
        [Display(Name = "Значение", Description = "Используется в процессе работы бота")]
        public string Value { get; set; }
        [Display(Name = "Описание", Description = "Описывает область применения")]
        public string Description { get; set; }
    }
}
