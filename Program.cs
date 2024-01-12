using System;
using System.Data.SqlClient;

class Program
{
    class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
    }

    static User signedInUser;

    static bool IsUserSignedIn()
    {
        return signedInUser != null;
    }

    static int GetSignedInUserID()
    {
        return signedInUser != null ? signedInUser.UserID : 0;
    }


    static void Main()
    {
        while (true)
        {
            Console.WriteLine("1. Signup");
            Console.WriteLine("2. Signin");
            Console.WriteLine("3. Ride Details");
            Console.WriteLine("4. Exit");

            Console.Write("Choose an option: ");
            int choice = int.Parse(Console.ReadLine());

            switch (choice)
            {
                case 1:
                    Signup();
                    break;
                case 2:
                    Signin();
                    break;
                case 3: 
                    RideDetails();
                    break;
                case 4:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    static void Signup()
    {
        Console.Write("Enter username: ");
        string username = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = Console.ReadLine();

        using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-FROJFGN\\SQLEXPRESS;Initial Catalog=OlaUber;Integrated Security=True;Encrypt=False"))
        {
            connection.Open();

            string query = $"INSERT INTO Users (Username, Password) VALUES ('{username}', '{password}')";
            SqlCommand command = new SqlCommand(query, connection);
            command.ExecuteNonQuery();

            Console.WriteLine("User signed up successfully!");
        }
    }

    static void Signin()
    {
        Console.Write("Enter username: ");
        string username = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = Console.ReadLine();

        using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-FROJFGN\\SQLEXPRESS;Initial Catalog=OlaUber;Integrated Security=True;Encrypt=False"))
        {
            connection.Open();

            string query = $"SELECT * FROM Users WHERE Username = '{username}' AND Password = '{password}'";
            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read(); 
                signedInUser = new User
                {
                    UserID = (int)reader["UserID"],
                    Username = (string)reader["Username"]
                };

                Console.WriteLine("Signin successful!");
                BookRide();
            }
            else
            {
                Console.WriteLine("Invalid username or password.");
            }
        }
    }

    static void BookRide()
    {
        // Hardcoded arrays for source, destination, vehicle, persons, and paymentMethod
        string[] sources = { "Thane", "Mulund", "CSMT" };
        string[] destinations = { "Thane", "Mulund", "CSMT" };
        string[] vehicles = { "Car", "Bike", "Auto" };
        int[] personsOptions = { 1, 2, 3, 4 };
        string[] paymentMethods = { "Card", "UPI", "Cash" };

        int userID = GetSignedInUserID();

        string source;
        string destination;
        string vehicle;
        int persons;
        string paymentMethod;
        string paymentDetails = "";

        do
        {
            Console.Write("Enter source location: ");
            Console.WriteLine(string.Join(", ", sources));
            source = Console.ReadLine();
        } while (!sources.Contains(source));

        do
        {
            do
            {
                Console.Write("Enter destination location: ");
                Console.WriteLine(string.Join(", ", destinations));
                destination = Console.ReadLine();

                if (source.Equals(destination, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Source and destination cannot be the same. Please enter again.");
                }
            } while (source.Equals(destination, StringComparison.OrdinalIgnoreCase));
        } while (!destinations.Contains(destination));

        do
        {
            Console.Write("Select a vehicle: ");
            Console.WriteLine(string.Join(", ", vehicles));
            vehicle = Console.ReadLine();
        } while (!vehicles.Contains(vehicle));

        switch (vehicle)
        {
            case "Car":
                personsOptions = new int[] { 1, 2, 3, 4 };
                break;
            case "Bike":
                personsOptions = new int[] { 1 };
                break;
            case "Auto":
                personsOptions = new int[] { 3 };
                break;
        }

        do
        {
            Console.Write("Enter the number of persons: ");
            Console.WriteLine(string.Join(", ", personsOptions));
            persons = int.Parse(Console.ReadLine());
        } while (!personsOptions.Contains(persons));

        do
        {
            Console.Write("Select payment method: ");
            Console.WriteLine(string.Join(", ", paymentMethods));
            paymentMethod = Console.ReadLine();

            switch (paymentMethod)
            {
                case "Card":
                    Console.Write("Enter 16-digit card number: ");
                    string cardNumber = Console.ReadLine();
                    if (cardNumber.Length == 16 && long.TryParse(cardNumber, out _))
                    {
                        paymentDetails = cardNumber;
                    }
                    else
                    {
                        Console.WriteLine("Invalid card number. Please enter again.");
                        paymentDetails = "";
                    }
                    break;

                case "UPI":
                    Console.Write("Enter UPI (should contain '@'): ");
                    string upi = Console.ReadLine();
                    if (upi.Contains("@"))
                    {
                        paymentDetails = upi;
                    }
                    else
                    {
                        Console.WriteLine("Invalid UPI. Please enter again.");
                        paymentDetails = "";
                    }
                    break;

                case "Cash":
                    paymentDetails = "Cash";
                    break;

                default:
                    Console.WriteLine("Invalid payment method. Please enter again.");
                    paymentDetails = "";
                    break;
            }
        } while (string.IsNullOrEmpty(paymentDetails));

        using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-FROJFGN\\SQLEXPRESS;Initial Catalog=OlaUber;Integrated Security=True;Encrypt=False"))
        {
            connection.Open();

            string insertQuery = $"INSERT INTO Rides3 (UserID, SourceLocation, DestinationLocation, VehicleType, Persons, PaymentMethod) " +
                                 $"VALUES ({userID}, '{source}', '{destination}', '{vehicle}', {persons}, '{paymentMethod}')";

            SqlCommand insertCommand = new SqlCommand(insertQuery, connection);

            try
            {
                insertCommand.ExecuteNonQuery();
                Console.WriteLine("Ride booked successfully!");

                // Fetch and display all details from the Rides3 table with the user's name
                string fetchQuery = $"SELECT TOP 1 Rides3.*, Users.Username FROM Rides3 " +
                     $"INNER JOIN Users ON Rides3.UserID = Users.UserID " +
                     $"WHERE Rides3.UserID = {userID} " +
                     $"ORDER BY Rides3.BookingTime DESC";

                SqlCommand fetchCommand = new SqlCommand(fetchQuery, connection);
                SqlDataReader reader = fetchCommand.ExecuteReader();

                if (reader.HasRows)
                {
                    Console.WriteLine("\nDetails of the ride booked by the user:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"Username: {reader["Username"]}");
                        Console.WriteLine($"SourceLocation: {reader["SourceLocation"]}");
                        Console.WriteLine($"DestinationLocation: {reader["DestinationLocation"]}");
                        Console.WriteLine($"VehicleType: {reader["VehicleType"]}");
                        Console.WriteLine($"Persons: {reader["Persons"]}");
                        Console.WriteLine($"PaymentMethod: {reader["PaymentMethod"]}");
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("No ride details found for the user.");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error booking ride: {ex.Message}");
            }
        }
    }

    static void RideDetails()
    {
        Console.Write("Enter username: ");
        string username = Console.ReadLine();

        Console.Write("Enter password: ");
        string password = Console.ReadLine();

        using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-FROJFGN\\SQLEXPRESS;Initial Catalog=OlaUber;Integrated Security=True;Encrypt=False"))
        {
            connection.Open();

            string query = $"SELECT Rides3.*, Users.Username FROM Rides3 " +
                           $"INNER JOIN Users ON Rides3.UserID = Users.UserID " +
                           $"WHERE Users.Username = '{username}' AND Users.Password = '{password}'";

            SqlCommand command = new SqlCommand(query, connection);
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                Console.WriteLine("\nDetails of all rides booked by the user:");
                while (reader.Read())
                {
                    Console.WriteLine($"Username: {reader["Username"]}");
                    Console.WriteLine($"SourceLocation: {reader["SourceLocation"]}");
                    Console.WriteLine($"DestinationLocation: {reader["DestinationLocation"]}");
                    Console.WriteLine($"VehicleType: {reader["VehicleType"]}");
                    Console.WriteLine($"Persons: {reader["Persons"]}");
                    Console.WriteLine($"PaymentMethod: {reader["PaymentMethod"]}");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No ride details found for the user.");
            }
        }
    }
}





