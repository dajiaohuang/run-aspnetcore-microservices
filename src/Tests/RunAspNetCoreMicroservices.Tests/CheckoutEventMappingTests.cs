using Basket.API.Basket.CheckoutBasket;
using Basket.API.Dtos;
using Basket.API.Models;
using BuildingBlocks.Messaging.Events;
using Ordering.Application.Orders.EventHandlers.Integration;
using Xunit;

namespace RunAspNetCoreMicroservices.Tests;

public class CheckoutEventMappingTests
{
    [Fact]
    public void BasketCheckoutEvent_carries_the_actual_nonempty_cart_items()
    {
        var productOne = Guid.NewGuid();
        var productTwo = Guid.NewGuid();
        var cart = new ShoppingCart("customer")
        {
            Items =
            [
                new ShoppingCartItem { ProductId = productOne, ProductName = "one", Quantity = 2, Price = 12.50m },
                new ShoppingCartItem { ProductId = productTwo, ProductName = "two", Quantity = 1, Price = 4m }
            ]
        };
        var checkout = new BasketCheckoutDto { UserName = "customer", TotalPrice = 999m };

        var message = CheckoutBasketCommandHandler.MapToBasketCheckoutEvent(checkout, cart);

        Assert.NotNull(message);
        Assert.Equal(29m, message.TotalPrice);
        Assert.Equal(
            new[] { new BasketCheckoutItem(productOne, 2, 12.50m), new BasketCheckoutItem(productTwo, 1, 4m) },
            message.Items
        );
    }

    [Fact]
    public void Empty_cart_does_not_create_a_checkout_event()
    {
        var message = CheckoutBasketCommandHandler.MapToBasketCheckoutEvent(
            new BasketCheckoutDto { UserName = "customer" },
            new ShoppingCart("customer")
        );

        Assert.Null(message);
    }

    [Fact]
    public void Ordering_uses_the_items_from_the_checkout_event()
    {
        var productOne = Guid.NewGuid();
        var productTwo = Guid.NewGuid();
        var message = new BasketCheckoutEvent
        {
            UserName = "customer",
            Items =
            [
                new BasketCheckoutItem(productOne, 2, 12.50m),
                new BasketCheckoutItem(productTwo, 1, 4m)
            ]
        };

        var command = BasketCheckoutEventHandler.MapToCreateOrderCommand(message);

        Assert.Collection(
            command.Order.OrderItems,
            item =>
            {
                Assert.Equal(productOne, item.ProductId);
                Assert.Equal(2, item.Quantity);
                Assert.Equal(12.50m, item.Price);
            },
            item =>
            {
                Assert.Equal(productTwo, item.ProductId);
                Assert.Equal(1, item.Quantity);
                Assert.Equal(4m, item.Price);
            }
        );
    }
}
