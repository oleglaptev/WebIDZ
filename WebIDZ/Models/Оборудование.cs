namespace WebIDZ.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Оборудование
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Оборудование()
        {
            this.Ремонты_и_техобслуживания = new HashSet<Ремонты_и_техобслуживания>();
        }

        [Key]
        [Display(Name = "ID оборудования")]
        public System.Guid ID_оборудования { get; set; }

        [Required]
        [Display(Name = "Название")]
        public string Название { get; set; }

        [Required]
        [Display(Name = "Дата ввода в эксплуатацию")]
        public System.DateTime Дата_ввода_в_эскплуатацию { get; set; }

        [Required]
        [Display(Name = "Местоположение")]
        public string Местоположение { get; set; }

        [Required]
        [Display(Name = "Состояние")]
        public bool Состояние { get; set; }

        [Required]
        [Display(Name = "Код типа")]
        public int Код_типа { get; set; }

        [Required]
        [Index(IsUnique = true)] // Уникальный индекс
        [Display(Name = "Инвентарный номер")]
        public int Инвентарный_номер { get; set; }

        public virtual Тип_оборудования Типы_оборудования { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ремонты_и_техобслуживания> Ремонты_и_техобслуживания { get; set; }
    }
}