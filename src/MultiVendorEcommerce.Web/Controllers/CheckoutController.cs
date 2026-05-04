using Application.Orders.Commands.PlaceOrder;
using Application.Orders.Queries.GetCheckoutSummary;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.ViewModels.Checkout;

namespace Web.Controllers;

[Authorize]
public class CheckoutController : Controller
{
    private readonly ISender _mediator;
    public CheckoutController(ISender mediator) => _mediator = mediator;

    public async Task<IActionResult> Index()
    {
        var summary = await _mediator.Send(new GetCheckoutSummaryQuery());
        if (!summary.Cart.Items.Any())
        {
            TempData["Error"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }
        if (!summary.Addresses.Any())
        {
            TempData["Error"] = "Please add a shipping address before checking out.";
            return RedirectToAction("Create", "Addresses");
        }

        ViewBag.AddressOptions = new SelectList(
            summary.Addresses,
            nameof(Application.Addresses.Queries.GetMyAddresses.AddressDto.Id),
            nameof(Application.Addresses.Queries.GetMyAddresses.AddressDto.Line1),
            summary.Addresses.FirstOrDefault(a => a.IsDefault)?.Id);

        return View(new CheckoutViewModel
        {
            Cart = summary.Cart,
            Addresses = summary.Addresses,
            ShippingAddressId = summary.Addresses.FirstOrDefault(a => a.IsDefault)?.Id ?? summary.Addresses[0].Id
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Place(CheckoutViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please check the form and try again.";
            return RedirectToAction(nameof(Index));
        }

        var r = await _mediator.Send(new PlaceOrderCommand(vm.ShippingAddressId, null, vm.PaymentMethod, vm.CouponCode));
        if (!r.Succeeded)
        {
            TempData["Error"] = string.Join(" ", r.Errors);
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Order placed successfully.";
        return RedirectToAction("Details", "Orders", new { id = r.Value });
    }
}
