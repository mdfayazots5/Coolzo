namespace Coolzo.Shared.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string OperationsManager = "OperationsManager";
    public const string OperationsExecutive = "OperationsExecutive";
    public const string CustomerSupportExecutive = "CustomerSupportExecutive";
    public const string Technician = "Technician";
    public const string Helper = "Helper";
    public const string Customer = "Customer";

    public static IReadOnlyCollection<string> All =>
    [
        SuperAdmin,
        Admin,
        OperationsManager,
        OperationsExecutive,
        CustomerSupportExecutive,
        Technician,
        Helper,
        Customer
    ];
}
