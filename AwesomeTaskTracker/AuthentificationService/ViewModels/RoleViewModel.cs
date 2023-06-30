using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuthentificationService.ViewModels;

public class RoleViewModel
{
    [Required]
    [Display(Name = "RoleName")]
    public string RoleName { get; set; } = "";
    
    public int RoleId { get; set; }

    public List<RoleViewModel>? ViewModels { get; set; } = new();
}