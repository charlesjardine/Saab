using System;
using TicketManagementSystem.Enums;

namespace TicketManagementSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var Message = string.Empty;

            try
            {
                Console.WriteLine("Ticket Service Test Harness");

                var service = new TicketService();

                //Parameters passed into this method could come from main args[0] ...
                var ticketId = service.CreateTicket(
                    "System Crash",
                    Priority.Medium,
                    "Johan",
                    "The system crashed when user performed a search",
                    DateTime.UtcNow,
                    true);

                service.AssignTicket(ticketId, "Michael");
                Message = "Done";

            }
            catch (InvalidTicketException e)
            {
                Message = e.Message;
                Console.WriteLine(e.Message);
            }
            catch (UnknownUserException e)
            {
                Message = e.Message;
                Console.WriteLine(e.Message);
            }
            catch (ApplicationException e)
            {
                Message = e.Message;
                Console.WriteLine(e.Message);
            }
            catch (NotImplementedException e)
            {
                Message = e.Message;
                Console.WriteLine(e.Message);
            }
            finally  
            {
                Console.WriteLine(Message);
            }
        }
    }
}
