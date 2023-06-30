using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthentificationService.ViewModels;

public class ManageUsersViewModel
{
    public List<UserViewModel> Users { get; set; } = new();
    public List<SelectListItem> Roles { get; set; } = new();
}