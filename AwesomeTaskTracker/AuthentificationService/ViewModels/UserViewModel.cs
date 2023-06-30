using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthentificationService.ViewModels;

public class UserViewModel
{
    [Required]
    [Display(Name = "Id попуга")]
    public int UserId { get; set; }
    [Required]
    [Display(Name = "Текущая роль")]
    public string CurrentRoleName { get; set; } = "";
    [Required]
    [Display(Name = "Имя попуга")]
    public string UserName { get; set; } = "";
    [Required]
    [Display(Name = "E-mail")]
    public string UserEmail { get; set; } = "";
    
    [BindProperty]
    [Required]
    [Display(Name = "Новая роль")]
    public string SelectedUserRole { get; set; }

    public List<SelectListItem> Roles { get; set; } = new();
}