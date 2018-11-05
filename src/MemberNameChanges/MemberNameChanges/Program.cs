namespace STCU.MemberNameChanges.Console
{
    using System;
    using System.IO;
    using Hyland.Unity;
    using Hyland.Unity.UnityForm;
    using Serilog;
    using Serilog.Core;
    using Serilog.Core.Enrichers;
    using Serilog.Enrichers;

    class LogConfiguration
    {
        #region Methods

        /// <summary>
        /// Configure a generic logging instance to be used across the whole application.
        /// </summary>
        public static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .Enrich.With(GetEnrichers())
                .CreateLogger();
        }

        public static void FlushSerilog()
        {
            Log.CloseAndFlush();
        }

        #endregion

        #region PrivateMethods

        private static ILogEventEnricher[] GetEnrichers()
        {
            var enrichers = new ILogEventEnricher[]
            {
                new MachineNameEnricher(),
                new PropertyEnricher("ApplicationName", System.Configuration.ConfigurationManager.AppSettings["Application.Name"]),
                new PropertyEnricher("Environment", System.Configuration.ConfigurationManager.AppSettings["Environment"])
            };

            return enrichers;
        }

        #endregion

    }

    class ProgArgs
    {
        public ProgArgs() { }

        public string InputFile { get; set; }
        public string OutputFile { get; set; }
    }

    class Member
    {
        public Member() { }
        public Member(String csvLine)
        {
            string[] values = csvLine.Split(',');
            MemberNo = values[0].Trim('"');
            LastName = values[1].Trim('"');
            FirstName = values[2].Trim('"');
        }

        public string MemberNo { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            LogConfiguration.ConfigureSerilog();
            Log.Logger.Information("Start Processing {AppName}!", System.Configuration.ConfigurationManager.AppSettings["Application.Name"]);


            //Exit code that indicates that the preprocessor ran successfully
            Environment.ExitCode = 0;

            //Ensure that two (3) parameters have been specified
            if (!args.Length.Equals(2))
            {
                Log.Logger.Error("Wrong number of parameters! Invoke with InputFile, ExportFile, TargetDirectory");
                Environment.ExitCode = -1;
            }
            else
            {
                ProgArgs progArgs = new ProgArgs
                {
                    InputFile = args[0],                // Example: PP_Member_Info.csv
                    OutputFile = args[1]                // Example: PP_Member_Info_out.csv
                };

                if (!File.Exists(progArgs.InputFile))
                {
                    //                    Log.Logger.Error("Input File {InputFile} does NOT exist!", progArgs.TargetDirectory + progArgs.InputFile);
                    Log.Logger.Error("Input File {InputFile} does NOT exist!", progArgs.InputFile);
                    Environment.ExitCode = -3;

                }
                else
                {
                    if (!processFile(progArgs))
                    {
                        Log.Logger.Error("A processing error occurred!");

                        // Exit code that indicates that an error occurred
                        // while running the preprocessor
                        Environment.ExitCode = -4;
                    }
                }
            }

            Log.Logger.Information("Exiting with code: {ExitCode}", Environment.ExitCode);
            LogConfiguration.FlushSerilog();
        }

        static bool processFile(ProgArgs args)
        {
            bool success = false; // initiate succss variable

            try
            {
                string line;
                //                StreamReader InFile = new StreamReader(args.TargetDirectory + args.InputFile);
                StreamReader InFile = new StreamReader(args.InputFile);

                //                using (StreamWriter OutFile = new StreamWriter(args.TargetDirectory + args.OutputFile, false))
                using (StreamWriter OutFile = new StreamWriter(args.OutputFile, false))
                {
                    using (Application obApp = OnBaseConnect())
                    {
                        // Get the Unity Form Template (by name or id).
                        FormTemplate nameChangeTemplate = obApp.Core.UnityFormTemplates.Find(@"Member Name Add/Modify");

                        if (nameChangeTemplate != null)
                        {
                            Log.Logger.Debug("FormTemplate Found: {FormTemplateName}", nameChangeTemplate.Name);

                            while ((line = InFile.ReadLine()) != null)
                            {
                                Member member = new Member(line);

                                // Create the Unity Form properties for the template
                                StoreNewUnityFormProperties uFormProps = obApp.Core.Storage.CreateStoreNewUnityFormProperties(nameChangeTemplate);

                                // Add form keywords
                                uFormProps.AddKeyword("Member No.", Convert.ToInt32(member.MemberNo));
                                uFormProps.AddKeyword(@"Last Name (or Business/Trust Name)", member.LastName);
                                uFormProps.AddKeyword("First Name", member.FirstName);

                                // Create the new Unity Form
                                Document newUnityForm = obApp.Core.Storage.StoreNewUnityForm(uFormProps);
                                Log.Logger.Debug(string.Format("Form Created: docid={0}.", newUnityForm.ID));

                                OutFile.WriteLine(line);

                            }
                            success = true;
                        }
                        else
                        {
                            Log.Logger.Error("Unity Form Template Not found!");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Processing file failed! {ExceptionMessage}", ex.Message);
            }

            return success;
        }

        private static Application OnBaseConnect()
        {
            Application app = null;

            try
            {
                String url = System.Configuration.ConfigurationManager.AppSettings["OnBase.Url"];
                String username = System.Configuration.ConfigurationManager.AppSettings["OnBase.Username"];
                String pwd = System.Configuration.ConfigurationManager.AppSettings["OnBase.Password"];
                String datasource = System.Configuration.ConfigurationManager.AppSettings["OnBase.Datasource"];

                OnBaseAuthenticationProperties authProps = Application.CreateOnBaseAuthenticationProperties(url, username, pwd, datasource);
                //app = Application.Connect(OnBase.OnBaseProperties.Authentication);
                app = Application.Connect(authProps);
            }
            catch (MaxLicensesException e)
            {
                Log.Logger.Error("Error: All available licenses have been consumed.");
                throw e;
            }
            catch (SystemLockedOutException e)
            {
                Log.Logger.Error("Error: The system is currently in lockout mode.");
                throw e;
            }
            catch (InvalidLoginException e)
            {
                Log.Logger.Error("Error: Invalid Login Credentials.");
                throw e;
            }
            catch (AuthenticationFailedException e)
            {
                Log.Logger.Error("Error: NT Authentication Failed.");
                throw e;
            }
            catch (MaxConcurrentLicensesException e)
            {
                Log.Logger.Error("Error: All concurrent licenses for this user group have been consumed.");
                throw e;
            }
            catch (InvalidLicensingException e)
            {
                Log.Logger.Error("Error: Invalid Licensing.");
                throw e;
            }
            catch (Exception e)
            {
                Log.Logger.Error("Error! {0}", e.Message);
                throw e;
            }

            if (app != null)
            {
                Log.Logger.Information("Connection Successful. Connection ID: {0}", app.SessionID);
            }

            return app;
        }

    }
}
