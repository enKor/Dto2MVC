using System.Drawing;
using Dto2Mvc.Lib.Attributes;

namespace Web.Models;

[Dto2Mvc(Dto2MvcAttribute.HttpMethod.Get,"Cars","Load")]
[Dto2Mvc(Dto2MvcAttribute.HttpMethod.Get,"Cars","Index")]
[Dto2Mvc(Dto2MvcAttribute.HttpMethod.Post,"Cars","Save")]
public class CarModel
{
    public bool IsNew { get; set; }
    public string? Type { get; set; }
    public int Length { get; set; }
    public Color Color { get; set; }
}