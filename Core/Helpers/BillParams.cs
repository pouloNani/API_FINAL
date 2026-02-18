// Core/Helpers/BillParams.cs
using Core.Entities;

namespace Core.Helpers;

public class BillParams : PaginationParams
{
    public int? ShopId { get; set; }
    public BillStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}