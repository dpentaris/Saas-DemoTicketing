using CareLog.Application.Abstractions;
using CareLog.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CareLog.Application.Visits;

public sealed record CreateVisitCommand(Guid PatientId, DateTimeOffset Start, DateTimeOffset End, string Address, Guid? NurseId) : IRequest<Guid>;

public sealed class CreateVisitValidator : AbstractValidator<CreateVisitCommand>
{
    public CreateVisitValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.Address).NotEmpty().MaximumLength(250);
        RuleFor(x => x.End).GreaterThan(x => x.Start);
    }
}

public sealed class CreateVisitHandler(IAppDbContext db, ITenantContext tenant, INotificationService notifications) : IRequestHandler<CreateVisitCommand, Guid>
{
    public async Task<Guid> Handle(CreateVisitCommand request, CancellationToken cancellationToken)
    {
        var visit = new Visit
        {
            TenantId = tenant.TenantId,
            PatientId = request.PatientId,
            ScheduledStart = request.Start,
            ScheduledEnd = request.End,
            Address = request.Address,
            AssignedNurseId = request.NurseId
        };

        db.Visits.Add(visit);
        await db.SaveChangesAsync(cancellationToken);
        await notifications.NotifyVisitChangedAsync(visit.Id, "Visit assigned/changed", cancellationToken);
        return visit.Id;
    }
}
