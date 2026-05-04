using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Refunds.Commands;

// ----- Approve -----
public sealed record ApproveRefundCommand(Guid OrderId) : IRequest<Result>;

public sealed class ApproveRefundCommandHandler : IRequestHandler<ApproveRefundCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IDateTimeService _clock;
    private readonly IRealtimeNotifier _rt;
    public ApproveRefundCommandHandler(IApplicationDbContext db, IDateTimeService clock, IRealtimeNotifier rt)
    { _db = db; _clock = clock; _rt = rt; }

    public async Task<Result> Handle(ApproveRefundCommand req, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == req.OrderId, ct);
        if (order is null) return Result.Failure("Order not found.");
        if (!order.RefundRequestedAtUtc.HasValue) return Result.Failure("No refund request on file.");

        order.Status = OrderStatus.Refunded;
        order.RefundedAtUtc = _clock.UtcNow;

        var captured = order.Payments.FirstOrDefault(p => p.Status == PaymentStatus.Captured);
        if (captured is not null) captured.Status = PaymentStatus.Refunded;

        var title = "Refund approved";
        var body  = $"Your refund for order {order.OrderNumber} has been approved.";
        var url   = $"/Orders/Details/{order.Id}";

        _db.Notifications.Add(new Notification
        {
            UserId = order.CustomerUserId,
            Title = title, Body = body, Type = NotificationType.OrderUpdate, ActionUrl = url
        });

        await _db.SaveChangesAsync(ct);
        await _rt.NotifyUserAsync(order.CustomerUserId, title, body, url, ct);
        return Result.Success();
    }
}

// ----- Reject -----
public sealed record RejectRefundCommand(Guid OrderId, string? Note = null) : IRequest<Result>;

public sealed class RejectRefundCommandHandler : IRequestHandler<RejectRefundCommand, Result>
{
    private readonly IApplicationDbContext _db;
    private readonly IRealtimeNotifier _rt;
    public RejectRefundCommandHandler(IApplicationDbContext db, IRealtimeNotifier rt) { _db = db; _rt = rt; }

    public async Task<Result> Handle(RejectRefundCommand req, CancellationToken ct)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == req.OrderId, ct);
        if (order is null) return Result.Failure("Order not found.");
        if (!order.RefundRequestedAtUtc.HasValue) return Result.Failure("No refund request on file.");

        order.RefundRequestedAtUtc = null;
        order.RefundReason = null;

        var title = "Refund declined";
        var body  = $"Your refund request for order {order.OrderNumber} was not approved." + (string.IsNullOrEmpty(req.Note) ? "" : $" Note: {req.Note}");
        var url   = $"/Orders/Details/{order.Id}";

        _db.Notifications.Add(new Notification
        {
            UserId = order.CustomerUserId,
            Title = title, Body = body, Type = NotificationType.OrderUpdate, ActionUrl = url
        });

        await _db.SaveChangesAsync(ct);
        await _rt.NotifyUserAsync(order.CustomerUserId, title, body, url, ct);
        return Result.Success();
    }
}
