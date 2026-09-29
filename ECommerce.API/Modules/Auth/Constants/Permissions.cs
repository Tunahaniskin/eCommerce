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
        public const string Update = "order.update"; // Sipariş durumunu güncelleme (örn: kargoya verildi)
        public const string Cancel = "order.cancel";
    }

    public static class Users
    {
        public const string View = "users.view"; // Kullanıcı listesini görebilme
        public const string Manage = "users.manage"; // Personel/Kullanıcı güncelleme
        public const string Delete = "users.delete";
    }

    public static class Roles
    {
        public const string View = "roles.view"; // Rolleri ve yetkileri görebilme
        public const string Manage = "roles.manage"; // Yeni rol oluşturma, yetki atama
    }
}