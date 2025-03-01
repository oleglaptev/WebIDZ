using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebIDZ.Models.ViewModels
{   
    public class HardwareVM
    {
        [Key]
        public System.Guid ID_оборудования { get; set; }

        [Required]
        public string Название { get; set; }

        [Required]
        [DisplayName("Дата ввода в эксплуатацию")]
        public System.DateTime Дата_ввода_в_эскплуатацию { get; set; }

        [Required]
        public string Местоположение { get; set; }

        [Required]
        public bool Состояние { get; set; }

        [Required]
        [DisplayName("Инвентарный номер")]
        public int Инвентарный_номер { get; set; }

        [DisplayName("Тип оборудования")]
        public string ТипОборудования { get; set; } // Новое свойство для наименования типа оборудования

        [DisplayName("Частота обслуживания")]
        public string ЧастотаОбслуживания { get; set; } // Новое свойство для частоты обслуживания

        //  свойство для хранения кода типа оборудования
        [DisplayName("Код типа")]
        public int Код_типа { get; set; }
    }
}

