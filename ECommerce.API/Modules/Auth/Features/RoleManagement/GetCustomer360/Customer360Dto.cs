using System;
using System.Collections.Generic;

namespace ECommerce.API.Modules.Auth.Features.RoleManagement.GetCustomer360;

public record Customer360ResponseDto(
    Customer360ProfileDto Profile,
    Customer360StatsDto Stats,
    List<Customer360AddressDto> Addresses,
    List<Customer360RecentOrderDto> RecentOrders,
    List<Customer360WalletTransactionDto> WalletTransactions
);

public record Customer360ProfileDto(
    Guid Id,
    string FullName,
    string Email,
    DateTime CreatedAt,
    bool IsDeleted,
    string RoleName
);

public record Customer360StatsDto(
    decimal CurrentBalance,
    decimal TotalSpent,
    int TotalOrdersCount,
    int RefundedOrdersCount
);

public record Customer360AddressDto(
    Guid Id,
    string Title,
    string RecipientName,
    string PhoneNumber,
    string City,
    string District,
    string FullAddress
);

public record Customer360RecentOrderDto(
    Guid OrderId,
    DateTime OrderDate,
    decimal TotalAmount,
    int ItemCount,
    string Status
);

public record Customer360WalletTransactionDto(
    Guid TransactionId,
    string Type,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    DateTime CreatedAt,
    string? CreatedByAdminEmail // Optional
);
