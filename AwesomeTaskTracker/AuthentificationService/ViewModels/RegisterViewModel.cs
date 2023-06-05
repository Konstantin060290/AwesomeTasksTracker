using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthentificationService.ViewModels;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Форма клюва")]
    public string BeakShape { get; set; } = "";

    [Required]
    [Compare(nameof(BeakShape), ErrorMessage = "Формы клюва не совпадают")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтвердить форму клюва")]
    public string? BeakShapeConfirm { get; set; }

    public List<SelectListItem> Roles { get; set; } = new();
    
    public SelectListItem SelectedRole { get; set; } = new();
}