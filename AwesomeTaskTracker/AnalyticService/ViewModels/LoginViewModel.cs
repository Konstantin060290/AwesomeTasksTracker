using System.ComponentModel.DataAnnotations;

namespace AnalyticService.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Форма клюва")]
    public string BeakShape { get; set; }= "";
}