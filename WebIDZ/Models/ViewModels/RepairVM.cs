using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebIDZ.Models.ViewModels
{
        public class RepairVM
        {
            public string Местоположение_оборудования { get; set; } // Местоположение оборудования

            public Guid ID_ремонта { get; set; }

            [Display(Name = "Дата начала")]
            public DateTime Дата_начала { get; set; }

            [Display(Name = "Дата окончания")]
            public DateTime? Дата_окончания { get; set; }

            [Display(Name = "Состояние")]
            public string Состояние { get; set; }

            [Display(Name = "ФИО мастера")]
            public string ФИО_мастера { get; set; }
            public Guid? ID_мастера { get; set; } // Добавляем ID мастера

            [Display(Name = "Название оборудования")]
            public string Название_оборудования { get; set; }

            [Display(Name = "Инвентарный номер")]
            [Required]
            public int Инвентарный_номер { get; set; }

            [Display(Name = "Суммарная стоимость")]
            public decimal Суммарная_стоимость { get; set; }

            [Display(Name = "Выполненные работы")]
            public List<Работа> Работы { get; set; }
    }
}