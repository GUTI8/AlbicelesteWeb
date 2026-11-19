namespace AlbicelesteWeb.Playground;

// Small helpers shared by the lessons
public static class SqlLog
{
    // Increased by the LogTo callback in Program.cs every time a SQL command runs
    public static int Count;

    // Prints a section title, so the output is easy to read
    public static void Section(string title) => Console.WriteLine($"\n===== {title} =====");
}