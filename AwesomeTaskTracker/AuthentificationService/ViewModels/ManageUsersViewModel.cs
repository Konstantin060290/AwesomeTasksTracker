using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthentificationService.ViewModels;

public class ManageUsersViewModel
{
    public List<UserViewModel> Users { get; set; } = new();
    public List<SelectListItem> Roles { get; set; } = new();
}

public class UserViewModel
{
    [Display(Name = "Id попуга")]
    public int UserId { get; set; }
    
    [Display(Name = "Текущая роль")]
    public string RoleName { get; set; } = "";
    
    [Display(Name = "Имя попуга")]
    public string UserName { get; set; } = "";
    
    [Display(Name = "E-mail")]
    public string UserEmail { get; set; } = "";
    
    [Display(Name = "Новая роль")]
    public SelectListItem SelectedUserRole { get; set; } = new();

    public string SelectedRoleString => SelectedUserRole.Value;
    
    public List<SelectListItem> Roles { get; set; } = new();
}

