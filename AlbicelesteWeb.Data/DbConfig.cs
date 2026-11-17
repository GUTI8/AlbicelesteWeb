namespace AlbicelesteWeb.Data;

public static class DbConfig
{
    // Local development only: Windows authentication, so there is no password
    public const string LocalConnectionString =
        @"Server=localhost\SQLEXPRESS;Database=Albiceleste;Trusted_Connection=True;TrustServerCertificate=True";
}