using System;

namespace Core.DTOs.Schedule;

public class ScheduleDto
{
    public int Id { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public bool IsClosed { get; set; }
    public int ShopId { get; set; }
}

public class CreateScheduleDto
{
    public DayOfWeek Day { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public bool IsClosed { get; set; }
}

public class UpdateScheduleDto
{
    public DayOfWeek Day { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public bool IsClosed { get; set; }
}