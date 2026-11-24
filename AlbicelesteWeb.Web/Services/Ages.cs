namespace AlbicelesteWeb.Web.Services;

// One shared age calculation for the whole web app (DRY).
// Optional tidy-up: make DashboardService use Ages.From too, and delete its private AgeOf.
public static class Ages
{
    public static int From(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        int age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age;
    }
}
