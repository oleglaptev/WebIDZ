namespace WebIDZ.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class Работа
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Работа()
        {
            this.Ремонты_и_техобслуживания = new HashSet<Ремонты_и_техобслуживания>();
        }

        [DisplayName("Код работы")]
        [Required]
        public int Код_работы { get; set; }

        [DisplayName("Наименование работы")]
        [Required]
        public string Наименование_работы { get; set; }

        [DisplayName("Стоимость работы")]
        [Required]
        public decimal Стоимость_работы { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Ремонты_и_техобслуживания> Ремонты_и_техобслуживания { get; set; }
    }
}
