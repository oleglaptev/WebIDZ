using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace WebIDZ.Models.ViewModels
{   
    public class RepairmanVM
    {
        [Key]
        public Guid ID_мастера { get; set; }

        [Required(ErrorMessage = "Квалификация обязательна")]
        [StringLength(100, ErrorMessage = "Квалификация не должна превышать 100 символов")]
        public string Квалификация { get; set; }

        [Required(ErrorMessage = "Должность обязательна")]
        [StringLength(100, ErrorMessage = "Должность не должна превышать 100 символов")]
        public string Должность { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна")]
        [StringLength(100, ErrorMessage = "Фамилия не должна превышать 100 символов")]
        public string Фамилия { get; set; }

        [Required(ErrorMessage = "Имя обязательно")]
        [StringLength(100, ErrorMessage = "Имя не должно превышать 100 символов")]
        public string Имя { get; set; }

        [StringLength(100, ErrorMessage = "Отчество не должно превышать 100 символов")]
        public string Отчество { get; set; }
    }
}