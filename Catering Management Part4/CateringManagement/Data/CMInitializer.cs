using CateringManagement.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Diagnostics;

namespace CateringManagement.Data
{
    public static class CMInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            CateringContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<CateringContext>();

            try
            {
                //We can use this to delete the database and start fresh.
                //context.Database.EnsureDeleted();
                //context.Database.EnsureCreated();
                context.Database.Migrate();

                #region Extra SQL

                //Create the Triggers
                string sqlCmd = @"
                    DROP TRIGGER IF EXISTS [SetFunctionTimestampOnUpdate];
                    CREATE TRIGGER SetFunctionTimestampOnUpdate
                    AFTER UPDATE ON Functions
                    BEGIN
                        UPDATE Functions
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END;
                ";
                context.Database.ExecuteSqlRaw(sqlCmd);

                sqlCmd = @"
                    DROP TRIGGER IF EXISTS [SetFunctionTimestampOnInsert];
                    CREATE TRIGGER SetFunctionTimestampOnInsert
                    AFTER INSERT ON Functions
                    BEGIN
                        UPDATE Functions
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ";
                context.Database.ExecuteSqlRaw(sqlCmd);

                sqlCmd = @"
                    Drop View IF EXISTS [FunctionRevenueSummary];
                    Create View FunctionRevenueSummary as
                    Select t.ID, t.Name, 
	                    ifnull(Count(f.ID),0) as TotalNumber,
	                    ifnull(Avg(f.PerPersonCharge),0) as AveragePPCharge,
	                    ifnull(Avg(f.GuaranteedNumber),0) as AverageGuarNo,
	                    ifnull(Sum(f.BaseCharge + f.SOCAN + (f.PerPersonCharge * f.GuaranteedNumber)),0) as TotalValue,
	                    ifnull(Avg(f.BaseCharge + f.SOCAN + (f.PerPersonCharge * f.GuaranteedNumber)),0) as AvgValue,
	                    ifnull(Max(f.BaseCharge + f.SOCAN + (f.PerPersonCharge * f.GuaranteedNumber)),0) as MaxValue
                    From FunctionTypes t left join Functions f
	                    on t.ID = f.FunctionTypeID
                    Group By t.ID, t.Name;
                ";
                context.Database.ExecuteSqlRaw(sqlCmd);

                #endregion

                //To randomly generate data
                Random random = new Random();

                // Look for any Customers.  Since we can't have Functions without Customers.
                if (!context.Customers.Any())
                {
                    context.Customers.AddRange(
                        new Customer
                        {
                            FirstName = "Gregory",
                            MiddleName = "A",
                            LastName = "House",
                            CompanyName = "JPMorgan Chase",
                            Phone = "4165551234",
                            CustomerCode = "C6555123",
                            EMail = "jkaluba@niagaracollege.ca"
                        },
                        new Customer
                        {
                            FirstName = "Doogie",
                            MiddleName = "R",
                            LastName = "Houser",
                            CompanyName = "Agriculture and Agri-Food Canada",
                            Phone = "5195551212",
                            CustomerCode = "G9555121",
                            EMail = "dstovell@niagaracollege.ca"
                        },
                        new Customer
                        {
                            FirstName = "Charles",
                            LastName = "Xavier",
                            CompanyName = null,
                            Phone = "9055552121",
                            CustomerCode = "I5555212",
                            EMail = "dtailor1@ncstudents.niagaracollege.ca"
                        });

                    context.SaveChanges();
                }
                // Seed data for Function Types if there aren't any.
                string[] functionTypes = new string[] { "Meeting", "Hospitality Room", "Exhibits", "Presentation", "Symposium", "Celebration" };
                if (!context.FunctionTypes.Any())
                {
                    foreach (string s in functionTypes)
                    {
                        FunctionType ft = new FunctionType
                        {
                            Name = s
                        };
                        context.FunctionTypes.Add(ft);
                    }
                    context.SaveChanges();
                }
                
                //Seed data for Meal Types
                string[] mealTypes = new string[] { "Breakfast", "Coffee Break(s)", "Lunch", "Dinner", "BBQ", "Pre Dinner Recpt.", "Reception", "Breakfast/AM", "Breakfast", "Breakfast/Lunch", "Light Snacks", "Wine", "Host Bar", "Cash Bar", "Brunch", "Reception/Dinner", "Boxed Lunch", "Boxed Breakfast" };
                if (!context.MealTypes.Any())
                {
                    foreach (string s in mealTypes)
                    {
                        MealType m = new MealType
                        {
                            Name = s
                        };
                        context.MealTypes.Add(m);
                    }
                    context.SaveChanges();
                }
                //Seed data for Rooms
                string[] rooms = new string[] { "Empire Room", "Empire I Room", "Empire II Room", "Empire Patio", "Bray Room", "Bray I Room", "Bray II Room", "Bray Patio", "Parlour Room", "Innkeepers Room", "Off Property", "Lobby", "Restaurant", "Tavern", "Guestroom" };
                if (!context.Rooms.Any())
                {
                    foreach (string s in rooms)
                    {
                        Room r = new Room
                        {
                            Name = s,
                            Capacity = random.Next(20, 200)
                        };
                        context.Rooms.Add(r);
                    }
                    context.SaveChanges();
                }

                //Create 5 notes from Bacon ipsum
                string[] baconNotes = new string[] { "Bacon ipsum dolor amet meatball corned beef kevin, alcatra kielbasa biltong drumstick strip steak spare ribs swine. Pastrami shank swine leberkas bresaola, prosciutto frankfurter porchetta ham hock short ribs short loin andouille alcatra. Andouille shank meatball pig venison shankle ground round sausage kielbasa. Chicken pig meatloaf fatback leberkas venison tri-tip burgdoggen tail chuck sausage kevin shank biltong brisket.", "Sirloin shank t-bone capicola strip steak salami, hamburger kielbasa burgdoggen jerky swine andouille rump picanha. Sirloin porchetta ribeye fatback, meatball leberkas swine pancetta beef shoulder pastrami capicola salami chicken. Bacon cow corned beef pastrami venison biltong frankfurter short ribs chicken beef. Burgdoggen shank pig, ground round brisket tail beef ribs turkey spare ribs tenderloin shankle ham rump. Doner alcatra pork chop leberkas spare ribs hamburger t-bone. Boudin filet mignon bacon andouille, shankle pork t-bone landjaeger. Rump pork loin bresaola prosciutto pancetta venison, cow flank sirloin sausage.", "Porchetta pork belly swine filet mignon jowl turducken salami boudin pastrami jerky spare ribs short ribs sausage andouille. Turducken flank ribeye boudin corned beef burgdoggen. Prosciutto pancetta sirloin rump shankle ball tip filet mignon corned beef frankfurter biltong drumstick chicken swine bacon shank. Buffalo kevin andouille porchetta short ribs cow, ham hock pork belly drumstick pastrami capicola picanha venison.", "Picanha andouille salami, porchetta beef ribs t-bone drumstick. Frankfurter tail landjaeger, shank kevin pig drumstick beef bresaola cow. Corned beef pork belly tri-tip, ham drumstick hamburger swine spare ribs short loin cupim flank tongue beef filet mignon cow. Ham hock chicken turducken doner brisket. Strip steak cow beef, kielbasa leberkas swine tongue bacon burgdoggen beef ribs pork chop tenderloin.", "Kielbasa porchetta shoulder boudin, pork strip steak brisket prosciutto t-bone tail. Doner pork loin pork ribeye, drumstick brisket biltong boudin burgdoggen t-bone frankfurter. Flank burgdoggen doner, boudin porchetta andouille landjaeger ham hock capicola pork chop bacon. Landjaeger turducken ribeye leberkas pork loin corned beef. Corned beef turducken landjaeger pig bresaola t-bone bacon andouille meatball beef ribs doner. T-bone fatback cupim chuck beef ribs shank tail strip steak bacon." };

                // Seed Functions if there aren't any.
                if (!context.Functions.Any())
                {
                    context.Functions.AddRange(
                        new Function
                        {
                            Name = "JPMorgan Chase Shareholders Meeting",
                            LobbySign = "JPMorgan Chase",
                            StartTime = new DateTime(2023, 11, 11, 13, 00, 00),
                            EndTime = new DateTime(2023, 11, 11, 15, 00, 00),
                            SetupNotes = baconNotes[random.Next(5)],
                            BaseCharge = 22000.00,
                            PerPersonCharge = 125.00,
                            GuaranteedNumber = 200,
                            SOCAN = 50.00,
                            Deposit = 50000.00,
                            Alcohol = false,
                            DepositPaid = true,
                            NoHST = false,
                            NoGratuity = false,
                            MealTypeID= context.MealTypes.FirstOrDefault(f => f.Name == "Coffee Break(s)").ID,
                            CustomerID = context.Customers.FirstOrDefault(d => d.FirstName == "Gregory" && d.LastName == "House").ID,
                            FunctionTypeID = context.FunctionTypes.FirstOrDefault(f => f.Name == "Meeting").ID
                        },
                        new Function
                        {
                            Name = "Xavier Birthday Party",
                            LobbySign = "Happy Birthday Mom!",
                            StartTime = new DateTime(2023, 12, 12, 15, 00, 00),
                            EndTime = new DateTime(2023, 12, 12, 19, 00, 00),
                            SetupNotes = baconNotes[random.Next(5)],
                            BaseCharge = 1000.00,
                            PerPersonCharge = 20.00,
                            GuaranteedNumber = 50,
                            SOCAN = 50.00,
                            Deposit = 500.00,
                            Alcohol = true,
                            DepositPaid = true,
                            NoHST = false,
                            NoGratuity = false,
                            MealTypeID = context.MealTypes.FirstOrDefault(f => f.Name == "Reception/Dinner").ID,
                            CustomerID = context.Customers.FirstOrDefault(c => c.FirstName == "Charles" && c.LastName == "Xavier").ID,
                            FunctionTypeID = context.FunctionTypes.FirstOrDefault(f => f.Name == "Celebration").ID
                        },
                        new Function
                        {
                            Name = "Behind the Numbers: What’s Causing Growth in Food Prices",
                            LobbySign = "Food Price Inflation",
                            StartTime = new DateTime(2023, 12, 25, 09, 00, 00),
                            EndTime = new DateTime(2023, 12, 25, 11, 30, 00),
                            SetupNotes = baconNotes[random.Next(5)],
                            BaseCharge = 2000.00,
                            PerPersonCharge = 50.00,
                            GuaranteedNumber = 40,
                            SOCAN = 50.00,
                            Deposit = 500.00,
                            Alcohol = true,
                            DepositPaid = false,
                            NoHST = true,
                            NoGratuity = true,
                            CustomerID = context.Customers.FirstOrDefault(c => c.FirstName == "Doogie" && c.LastName == "Houser").ID,
                            FunctionTypeID = context.FunctionTypes.FirstOrDefault(f => f.Name == "Presentation").ID
                        });

                    context.SaveChanges();
                }

                //Because we can, lets add a bunch more Customers and Functions
                string[] companyNames = new string[] { "Consolidated Credit Union", "PEI  Womens Institute", "Atlantic Psychiatric Conference", "TLC Laser Eye Centers", "Cooperators Insurance", "Department of Education", "PEI Teacher's Federation", "Credit Union Managers", "International Union of Operating Engineers", "Department of Transportation & Public Works", "Department of Fisheries & Oceans", "Summerside Tax Center", "Summerside Community Church", "Health & Social Services", "Department of Education", "Bridge Tournament Tour", "Malpeque bay Credit Union", "Atlantic provinces Chamber of  Commerce", "Child & Family Services", "Canadian mental Health assoc.", "PEI Institute if Agrologists", "Acadian Fishermans Coop", "Toronto Dominion Bank", "Highfield Construction", "Dept of Fisheries & Oceans", "Canadian Motor Veichle Arbitration Plan", "PEI Real Estate Assoc.", "Heart & Stroke Foundation", "East Prince Youth Development Centre", "PEI Assoc of Exhibitions", "PEI Potato Processing Council", "Union of Public sector Employees", "Cavendish Agri services", "PEI Pharmaceutical Assoc", "Zellers District Office", "UPEI  Faculty of Education", "College of Family Physicians-PEI chapter", "Dept of Education and Early Childhood Development", "Aerospace Industries Assoc of Canada", "Dept of Transportation", "Mark's Work Warehouse", "Cavendish Agri-Services", "Baie Acadienne Dev. Corp", "Primerica Financial Services", "Holland College-Adult Education", "Downtown  Summerside Inc.", "PEI Hairdressers Assoc", "Occupational Health & Safety Division-WCB", "BTC-Building Tennis Communities", "PEI Institute of Agrologists", "Agriculture Insurance Corp", "Island Health Training Center", "PEI Federation of Agriculture", "Cavendish Agri Services", "Public  Service Alliance", "Loyalist Lakeview Resort", "Consolidated Credit Union", "Cusource-Credit Un.Cen.of N.S.", "Callbecks  Home Hardware", "Summerside Tax Center", "Summerside Tax center", "Genworth Financial Canada", "East Prince Health board", "Agricultural Insurance Corp", "Family Resource Center", "The National Chapter of Canada IODE", "Summerside Tax Center", "Pro Trans Personal Services", "Girls & Women in Sports", "Dept of Agriculture, Fisheries &Aquaculture", "PEI Automobile Dealers Assoc", "Spreadsheet Solutions", "PC Association of PEI-22ND DISTRICT", "Can Society of Safety Engineering", "Canadian Assoc of Property & Home Inspectors", "Egg Producers Board of PEI", "Key, McKnight & Maynard", "Agriculture & Agri Food Canada", "Atlantic Canada Oppurtunities", "Premier World Tours Canadian Maritimes Pioneer" };
                string[] firstNames = new string[] { "Lyric", "Antoinette", "Kendal", "Vivian", "Ruth", "Jamison", "Emilia", "Natalee", "Yadiel", "Jakayla", "Lukas", "Moses", "Kyler", "Karla", "Chanel", "Tyler", "Camilla", "Quintin", "Braden", "Clarence" };
                string[] lastNames = new string[] { "Watts", "Randall", "Arias", "Weber", "Stone", "Carlson", "Robles", "Frederick", "Parker", "Morris", "Soto", "Bruce", "Orozco", "Boyer", "Burns", "Cobb", "Blankenship", "Houston", "Estes", "Atkins", "Miranda", "Zuniga", "Ward", "Mayo", "Costa", "Reeves", "Anthony", "Cook", "Krueger", "Crane", "Watts", "Little", "Henderson", "Bishop" };
                string[] companyLetters = new string[] { "C", "G", "I", "A" };
                int companyNameCount = companyNames.Length;
                int firstNamesCount = firstNames.Length;
                int lastNamesCount = lastNames.Length;

                if (context.Customers.Count()<4)
                {
                    foreach (string s in companyNames)
                    {
                        Customer c = new Customer
                        {
                             CompanyName = s,
                             FirstName = firstNames[random.Next(0, firstNamesCount)],
                             LastName = lastNames[random.Next(0, lastNamesCount)],
                             MiddleName = lastNames[random.Next(0, lastNamesCount)][1].ToString().ToUpper(),
                             Phone = random.Next(2, 10).ToString() + random.Next(213214131, 989898989).ToString(),
                             CustomerCode= companyLetters[random.Next(4)].ToUpper() + random.Next(2132141, 9898989).ToString()
                        };
                        context.Customers.Add(c);
                    }
                    context.SaveChanges();
                }

                //Create collections of the primary key
                int[] mealTypeIDs = context.MealTypes.Select(d => d.ID).ToArray();
                int mealTypeIDCount = mealTypeIDs.Length;
                int[] functionTypeIDs = context.FunctionTypes.Select(d => d.ID).ToArray();
                int functionTypeIDCount = functionTypeIDs.Length;

                //A bunch more functions - one for each of the new Customers we just added
                if (context.Functions.Count() < 4)
                {
                    foreach (string s in companyNames)
                    {
                        Function f = new Function
                        {
                            Name = s + " " + functionTypes[random.Next(functionTypeIDCount)],
                            LobbySign = s + " Event",
                            StartTime = DateTime.Today.AddDays((-1 * random.Next(1000)) + random.Next(1500)),
                            SetupNotes = baconNotes[random.Next(5)],
                            BaseCharge = (double)random.Next(5000),
                            PerPersonCharge = (double)random.Next(20,150),
                            GuaranteedNumber = random.Next(120),
                            SOCAN = 50.00,
                            Deposit = (double)random.Next(10) * 100d,
                            Alcohol = true,
                            DepositPaid = true,
                            NoHST = false,
                            NoGratuity = false,
                            MealTypeID = mealTypeIDs[random.Next(mealTypeIDCount)],
                            CustomerID = context.Customers.FirstOrDefault(c => c.CompanyName == s).ID,
                            FunctionTypeID = functionTypeIDs[random.Next(functionTypeIDCount)]
                        };
                        //StartTime will be at midnight so add hours to it so it is more reasonable
                        f.StartTime = f.StartTime.AddHours(random.Next(8, 17));
                        //Set a random EndTime between 10 and 360 minutes later
                        f.EndTime = f.StartTime + new TimeSpan(0, random.Next(1, 36) * 10, 0);
                        //Make some alcohol free and some have not paid the deposit
                        if (f.ID % 3 == 0) f.Alcohol=false;
                        if (f.ID % 2 == 0) f.DepositPaid = false;
                        context.Functions.Add(f);
                    }
                    context.SaveChanges();
                }

                //Now lets assign multiple rooms to Functions by creating FunctionRooms
                //Create collections of the primary key
                int[] functionIDs = context.Functions.Select(d => d.ID).ToArray();
                int functionIDCount = functionIDs.Length;
                int[] roomIDs = context.Rooms.Select(d => d.ID).ToArray();
                int roomIDCount = roomIDs.Length;

                //FunctionRooms - the Intersection
                //Add a few Rooms to each Function
                if (!context.FunctionRooms.Any())
                {
                    int k = 0;//Start with the first Room
                    foreach (int i in functionIDs)
                    {
                        //i loops through the primary keys of the Functions
                        //j is just a counter so we add some Rooms to a Patient
                        //k lets us step through all Rooms so we can make sure each gets used
                        int howMany = random.Next(1, roomIDCount);
                        for (int j = 1; j <= howMany; j++)
                        {
                            k = (k >= roomIDCount) ? 0 : k;
                            FunctionRoom r = new FunctionRoom()
                            {
                                FunctionID = i,
                                RoomID = roomIDs[k],
                            };
                            k++;
                            context.FunctionRooms.Add(r);
                        }
                        context.SaveChanges();
                    }
                }

                //Seed data for Rooms
                string[] equipments = new string[] { "TV", "Flipchart", "TV/VCR", "Overhead/Screen", "Slide Projector/Screen", "Podium & Mic.", "Table, Podium & Mic.", "Screen", "TV/DVD Player", "Whiteboard", "Headtable for 3", "Headtable for 4", "Headtable for 5", "Headtable for 6", "Headtable for 7", "Headtable for 8", "Headtable for 9", "Headtable for 10", "Own Audio/Visual" };
                if (!context.Equipments.Any())
                {
                    foreach (string name in equipments)
                    {
                        Equipment e = new Equipment
                        {
                            Name = name,
                            StandardCharge = random.Next(1, 10) * 10d
                        };
                        context.Equipments.Add(e);
                    }
                    context.SaveChanges();
                }
                //Seed data for Workers
                if (!context.Workers.Any())
                {
                    foreach (string s in lastNames)
                    {
                        Worker w = new Worker
                        {
                            FirstName = firstNames[random.Next(0, firstNamesCount)],
                            LastName = s,
                            MiddleName = lastNames[random.Next(0, lastNamesCount)][1].ToString().ToUpper(),
                            Phone = random.Next(2, 10).ToString() + random.Next(213214131, 989898989).ToString()
                        };
                        context.Workers.Add(w);
                    }
                    context.SaveChanges();
                }

                //Create collections of the primary key
                int[] workerIDs = context.Workers.Select(d => d.ID).ToArray();
                int workerIDCount = workerIDs.Length;
                int[] equipmentIDs = context.Equipments.Select(d => d.ID).ToArray();
                int equipmentIDCount = equipmentIDs.Length;

                //FunctionEquipment - the Intersection
                //Add a few Equipments to each Function
                if (!context.FunctionEquipments.Any())
                {
                    int k = 0;//Start with the first piece of Equipment
                    foreach (int i in functionIDs)
                    {
                        //i loops through the primary keys of the Functions
                        //j is just a counter so we add some Equipment to a Function
                        //k lets us step through all Equipment so we can make sure each gets used
                        int howMany = random.Next(1, equipmentIDCount);
                        for (int j = 1; j <= howMany; j++)
                        {
                            k = (k >= equipmentIDCount) ? 0 : k;
                            FunctionEquipment e = new FunctionEquipment()
                            {
                                FunctionID = i,
                                EquipmentID = equipmentIDs[k],
                                Quantity = random.Next(1, howMany),
                                PerUnitCharge = context.Equipments.Find(equipmentIDs[k]).StandardCharge
                            };
                            k++;
                            context.FunctionEquipments.Add(e);
                        }
                        context.SaveChanges();
                    }
                }

                //Works - the Intersection
                //Add a few Works records to each Function
                if (!context.Works.Any())
                {
                    int k = 0;//Start with the first worker
                    foreach (int i in functionIDs)
                    {
                        //i loops through the primary keys of the Functions
                        //j is just a counter so we add some Workers to a Function
                        //k lets us step through all Workers;
                        int howMany = random.Next(1, workerIDCount);
                        for (int j = 1; j <= howMany; j++)
                        {
                            k = (k >= workerIDCount) ? 0 : k;
                            Work w = new Work()
                            {
                                FunctionID = i,
                                WorkerID = workerIDs[k],
                                Points = random.Next(1, howMany)
                            };
                            k++;
                            context.Works.Add(w);
                        }
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.GetBaseException().Message);
            }
        }
    }
}
