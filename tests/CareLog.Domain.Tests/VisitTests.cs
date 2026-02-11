using CareLog.Domain.Entities;
using CareLog.Domain.Enums;

namespace CareLog.Domain.Tests;

public class VisitTests
{
    [Fact]
    public void CheckIn_TransitionsToInProgress()
    {
        var visit = new Visit { Status = VisitStatus.Planned };
        visit.CheckIn(DateTimeOffset.UtcNow);
        Assert.Equal(VisitStatus.InProgress, visit.Status);
        Assert.NotNull(visit.CheckInAt);
    }

    [Fact]
    public void CheckOut_TransitionsToDone()
    {
        var visit = new Visit { Status = VisitStatus.InProgress };
        visit.CheckOut(DateTimeOffset.UtcNow);
        Assert.Equal(VisitStatus.Done, visit.Status);
        Assert.NotNull(visit.CheckOutAt);
    }
}
