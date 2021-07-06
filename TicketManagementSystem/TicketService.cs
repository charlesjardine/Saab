using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using EmailService;
using TicketManagementSystem.Enums;

namespace TicketManagementSystem
{
    public class TicketService
    {
      
        public int CreateTicket(string t, Priority p, string assignedTo, string desc, DateTime d, bool isPayingCustomer)
        {
            // Check if t or desc are null or if they are invalid and throw exception
            if (string.IsNullOrEmpty(t) || string.IsNullOrEmpty(desc))
            {
                throw new InvalidTicketException("Title or description were null");
            }

            User user = null;
            using (var ur = new UserRepository())
            {
                if (!string.IsNullOrEmpty(assignedTo))
                {
                    user = ur.GetUser(assignedTo);
                }
            }
            //Test Code, uncomment the line below to allow code to run
            //user = CreateUser(assignedTo, string.Empty);

            if (user == null)
            {
                throw new UnknownUserException("User " + assignedTo + " not found");
            }

            p = SetPriority(t, p, assignedTo, d);
            
            double price = 0;
            User accountManager = null;
            if (isPayingCustomer)
            {
                // Only paid customers have an account manager.
                accountManager = new UserRepository().GetAccountManager();
                price = 50;

                if (p == Priority.High)
                {
                    price = 100;
                }
            }

            var ticket = new Ticket()
            {
                Title = t,
                AssignedUser = user,
                Priority = p,
                Description = desc,
                Created = d,
                PriceDollars = price,
                AccountManager = accountManager
            };

            var id = TicketRepository.CreateTicket(ticket);

            // Return the id
            return id;
        }

        public void ConsolePrint()
        {
            Console.WriteLine("Ticket Created");
        }


        /// <summary>
        /// Method not implemented, this should be removed or a use case statement declared in the specifications scope 
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        internal object CreateTicket(Action p)
        {
            throw new NotImplementedException();
        }

        public void AssignTicket(int id, string username)
        {
            User user = null;
            using (var ur = new UserRepository())
            {
                if (!string.IsNullOrEmpty(username))
                {
                    user = ur.GetUser(username);
                }
            }

            //Test Code, uncomment the line below to allow code to run
            // user = CreateUser(username, string.Empty);

            if (user == null)
            {
                throw new UnknownUserException("User not found");
            }

            var ticket = TicketRepository.GetTicket(id);

            if (ticket == null)
            {
                throw new ApplicationException("No ticket found for id " + id);
            }

            ticket.AssignedUser = user;

            TicketRepository.UpdateTicket(ticket);
        }

        /// <summary>
        /// Method not implemented, this should be removed or a use case statement declared in the specifications scope  
        /// </summary>
        /// <param name="ticket"></param>
        private void WriteTicketToFile(Ticket ticket)
        {
            var ticketJson = JsonSerializer.Serialize(ticket);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"ticket_{ticket.Id}.json"), ticketJson);
        }

        /// <summary>
        /// ErrorCheck Method added. Could be read from a database list of errors
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private bool ErrorCheck(string input)
        {
            var ListError = new List<string>
            {
                "Crash",
                "Important",
                "Failure"
            };

            foreach (string item in ListError)
            {
                if (input.Contains(item))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Send Email Method
        /// </summary>
        /// <param name="t"></param>
        /// <param name="assignedTo"></param>
        /// <returns></returns>
        private bool SendMail(string t,string assignedTo)
        {
            try
            {
                //Should return a success or fail
                var emailService = new EmailServiceProxy();
                emailService.SendEmailToAdministrator(t, assignedTo);
                return true;
            }
            catch (ArgumentNullException e)
            {
                //Log errors to logging system
                Console.WriteLine(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Sets the Ticket Priority
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        /// <param name="assignedTo"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private Priority SetPriority(string t,Priority p,string assignedTo,DateTime d)
        {
            var priorityRaised = false;
            if (d < DateTime.UtcNow - TimeSpan.FromHours(1))
            {
                switch (p)
                {
                    case Priority.Low:
                        p = Priority.Medium;
                        priorityRaised = true;
                        break;
                    case Priority.Medium:
                    case Priority.High:
                        p = Priority.High;
                        priorityRaised = true;
                        break;
                }
            }

            if (ErrorCheck(t) && !priorityRaised)
            {
                switch (p)
                {
                    case Priority.Low:
                        p = Priority.Medium;
                        break;
                    case Priority.Medium:
                    case Priority.High:
                        p = Priority.High;

                        if (!SendMail(t, assignedTo))
                            Console.WriteLine("Error in sending Mail!");

                        break;
                }
            }
            return p;
        }

        /// <summary>
        /// Test Code to Create User
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        private User CreateUser(string firstName, string lastName)
        {
            string FullName = firstName + " " + lastName;
            return new User { FirstName = firstName, LastName = lastName, Username = FullName };
        }
    }
}
