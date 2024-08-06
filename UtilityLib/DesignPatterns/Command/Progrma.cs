using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilityLib.DesignPatterns.Command
{

    internal class Progrma
    {
        Progrma()
        {
            Document document = new Document();
            CommandManager commandManager = new CommandManager();

            // Simulate text editing
            commandManager.ExecuteCommand(new InsertTextCommand(document, "Hello, ", 0));
            commandManager.ExecuteCommand(new InsertTextCommand(document, "World!", 7));
            Console.WriteLine(document); // Output: Hello, World!

            // Undo last insert
            commandManager.Undo();
            Console.WriteLine(document); // Output: Hello, 

            // Redo last insert
            commandManager.Redo();
            Console.WriteLine(document); // Output: Hello, World!

            // Perform delete
            commandManager.ExecuteCommand(new DeleteTextCommand(document, 0, 5));
            Console.WriteLine(document); // Output: , World!

            // Undo delete
            commandManager.Undo();
            Console.WriteLine(document); // Output: Hello, World!
        }    
    }
}
