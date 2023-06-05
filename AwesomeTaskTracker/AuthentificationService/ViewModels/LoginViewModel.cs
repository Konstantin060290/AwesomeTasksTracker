using System.ComponentModel.DataAnnotations;

namespace AuthentificationService.ViewModels;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Email")]
    public string? Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Форма клюва")]
    public string? BeakShape { get; set; }
    
    [Display(Name ="Запомнить меня")] 
    public bool RememberMe { get; set; } 
    
    public string? ReturnUrl { get; set; } 

}