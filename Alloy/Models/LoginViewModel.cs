using System.ComponentModel.DataAnnotations;

namespace Epicweb.Alloy.QuickNavExtension.Models;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; }

    [Required]
    public string Password { get; set; }
}
