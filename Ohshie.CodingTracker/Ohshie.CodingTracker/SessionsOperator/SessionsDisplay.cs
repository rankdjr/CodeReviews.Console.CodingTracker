using ConsoleTableExt;

namespace Ohshie.CodingTracker;

public class SessionsDisplay
{
    private readonly DbOperations _dbOperations = new();
    public bool ShowSessions()
    {
        List<Session> allSessions = _dbOperations.FetchAllSessions();
        if (allSessions == null)
        {
            Console.WriteLine("You have no sessions tracked, go create some.");
            return false;
        }
        
        ConsoleTableBuilder.From(allSessions).ExportAndWriteLine();
        return true;
    }

    public bool ShowSessions(int amount)
    {
        List<Session> allSessions = _dbOperations.FetchAllSessions();
        if (allSessions == null)
        {
            Console.WriteLine("You have no sessions tracked, go create some.");
            return false;
        }
        
        List<Session> packedSessions = allSessions
            .OrderByDescending(s => s.Id)
            .Take(amount)
            .ToList();

        ConsoleTableBuilder.From(packedSessions).ExportAndWriteLine();
        return true;
    }

    public bool ShowSession(int id)
    {
        Session session = _dbOperations.FetchSession(id);
        if (session == null)
        {
            Console.WriteLine("Session does not exist");
            return false;
        }

        Console.WriteLine($"{session.Description}\n" +
                          $"Session was conducted on: {session.Date}\n" +
                          $"Lasted for: {session.Length}");
        if (!string.IsNullOrEmpty(session.Note)) Console.WriteLine($"You left a note: {session.Note}");
        
        return true;
    }
}