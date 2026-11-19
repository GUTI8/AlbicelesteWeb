using AlbicelesteWeb.Data;
using AlbicelesteWeb.Playground;
using AlbicelesteWeb.Playground.Lessons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

// =====================================================================
// AlbicelesteWeb.Playground
// Experiments for the EF Core lessons. Every SQL command is printed
// and counted (see SqlLog.cs).
// =====================================================================

var options = new DbContextOptionsBuilder<AlbicelesteDbContext>()
    .UseSqlServer(DbConfig.LocalConnectionString)
    .LogTo(message => { SqlLog.Count++; Console.WriteLine(message); },   // print and count every SQL command
        new[] { RelationalEventId.CommandExecuted })
    .EnableSensitiveDataLogging()                                        // show parameter values (local dev only!)
    .Options;

using var db = new AlbicelesteDbContext(options);

// Choose the lesson to run:
// LessonE3.Run(db);
//LessonE4.Run(db);
LessonE5.Run(db);