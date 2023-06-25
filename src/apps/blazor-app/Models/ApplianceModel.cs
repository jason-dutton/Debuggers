using System.ComponentModel.DataAnnotations;

namespace BlazorApp.Models;
public class ApplianceModel
{
  public int applianceId { get; set; }
  public string? type { get; set; }
  public int powerUsage { get; set; }
}
