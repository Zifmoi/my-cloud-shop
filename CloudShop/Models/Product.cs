using System.ComponentModel.DataAnnotations;

namespace CloudShop.Models
{
    // Класс модели, который Entity Framework автоматически превратит в таблицу Products в базе данных
    public class Product
    {
        public int Id { get; set; } // Первичный ключ (заполняется базой автоматически)

        [Required(ErrorMessage = "Введите название товара")] // Валидация обязательного поля
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }
    }
}