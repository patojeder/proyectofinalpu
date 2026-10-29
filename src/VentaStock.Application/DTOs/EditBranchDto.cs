using System.ComponentModel.DataAnnotations;

namespace VentaStock.Application.DTOs;

public class EditBranchDto : BranchFieldsDto
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }
}

