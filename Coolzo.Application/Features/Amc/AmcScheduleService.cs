using Coolzo.Domain.Entities;

namespace Coolzo.Application.Features.Amc;

public sealed class AmcScheduleService
{
    public IReadOnlyCollection<AmcVisitSchedule> BuildInitialSchedule(
        CustomerAmc customerAmc,
        string createdBy,
        string ipAddress,
        DateTime createdOnUtc)
    {
        var scheduledDates = BuildVisitDates(customerAmc.StartDateUtc, customerAmc.EndDateUtc, customerAmc.TotalVisitCount);
        var schedules = new List<AmcVisitSchedule>(scheduledDates.Count);

        for (var index = 0; index < scheduledDates.Count; index++)
        {
            schedules.Add(new AmcVisitSchedule
            {
                VisitNumber = index + 1,
                ScheduledDate = scheduledDates[index],
                VisitRemarks = $"AMC visit {index + 1} scheduled during subscription activation.",
                CreatedBy = createdBy,
                DateCreated = createdOnUtc,
                IPAddress = ipAddress
            });
        }

        return schedules;
    }

    private static IReadOnlyList<DateOnly> BuildVisitDates(DateTime startDateUtc, DateTime endDateUtc, int visitCount)
    {
        var startDate = DateOnly.FromDateTime(startDateUtc);
        var endDate = DateOnly.FromDateTime(endDateUtc);
        var totalDays = Math.Max(0, endDate.DayNumber - startDate.DayNumber);

        if (visitCount <= 1 || totalDays == 0)
        {
            return new[] { startDate };
        }

        var dates = new List<DateOnly>(visitCount);

        for (var index = 0; index < visitCount; index++)
        {
            var offset = (int)Math.Round((double)totalDays * index / (visitCount - 1), MidpointRounding.AwayFromZero);
            dates.Add(startDate.AddDays(offset));
        }

        return dates;
    }
}
