using System.ComponentModel.DataAnnotations;

namespace BlazorServerClient.ViewModel;

public class FileUploadViewModel
{
    [Required]    
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    public string Filename { get; set; }    
}
