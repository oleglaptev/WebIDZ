//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebIDZ.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Тип_оборудования
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Тип_оборудования()
        {
            this.Оборудование = new HashSet<Оборудование>();
        }
    
        public int Код_типа { get; set; }
        public string Наименование { get; set; }
        public int Количество_раз_обслуживания { get; set; }
        public string Отрезок_времени { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Оборудование> Оборудование { get; set; }
    }
}
