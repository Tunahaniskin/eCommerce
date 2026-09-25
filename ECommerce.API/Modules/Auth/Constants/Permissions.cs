namespace ECommerce.API.Modules.Auth.Constants;

public static class Permissions
{
    // Wildcard (Joker) - Sadece SuperAdmin'de olur
    public const string Wildcard = "*";

    public static class Product
    {
        public const string ReadDeleted = "product.read_deleted";
        public const string Create = "product.create";
        public const string Update = "product.update";
        public const string Delete = "product.delete";
    }

    public static class Order
    {
        public const string ViewAll = "order.view_all";
        public const string Cancel = "order.cancel";
    }

    public static class Users
    {
        public const string Manage = "users.manage"; // Personel ekleme/yetkilendirme
        public const string Delete = "users.delete";
    }
}