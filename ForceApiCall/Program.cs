using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ForceApiCall
{
    class Program
    {
        static async Task Main(string[] _args)
        {
            string userdatajson = File.ReadAllText("Userdata.json");
            dynamic data = JsonConvert.DeserializeObject(userdatajson);
            string client_id = data.User;
            string client_secret = data.Password;

            string jsonresponse = DoAuthenticationRequest(client_id, client_secret);
            Console.WriteLine("Authetication Request completed");

            AuthResponseObject responseObject = JsonConvert.DeserializeObject<AuthResponseObject>(jsonresponse);

            Console.WriteLine("Start Requesting all Workplace Ids");
            (List<string> allWorkplacesIds, Dictionary<string, string> wpNoWpDescDict) = GetWorkplaceIds(responseObject.access_token);
            Console.WriteLine("Workplace Id Request completed");

            Console.WriteLine("Get Setup Data for each workplace Id");
            List<Task<IRestResponse>> TaskObject = await DoIndividualRequestAsync(responseObject.access_token, allWorkplacesIds);
            Console.WriteLine("Request for setup data complete");

            List<Workplace> workplaces = CreateWorkplaceObjects(TaskObject, wpNoWpDescDict);

            //Console.WriteLine("Create Json file");
            //CreateJsonFile(workplaces);
            //Console.WriteLine("Json File created");

            //string json = File.ReadAllText(@"C:\Users\alexa\Documents\workplaces.json");
            //List<Workplace> workplaces = JsonConvert.DeserializeObject<List<Workplace>>(json);
            Console.WriteLine("Update Database with new Data");
            UpdateDatabase(workplaces);
            Console.WriteLine("Update complete, program exit");
        }

        public static string DoAuthenticationRequest(string client_id, string client_secret)
        {
            Console.WriteLine("Authentication Request started");

            var client = new RestClient("https://fcfhws.forcam.university/ffauth/oauth2.0/accessToken");
            var request = new RestRequest(Method.POST);

            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "client_id="
                + client_id + "&client_secret=" + client_secret
                + "&grant_type=client_credentials&scope=read+write", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            return response.Content;
        }

        public static (List<string>, Dictionary<string, string>) GetWorkplaceIds(string bearer)
        {
            var client = new RestClient();
            client.Authenticator = new JwtAuthenticator(bearer);
            var request = new RestRequest("https://fcfhws.forcam.university/ffwebservices/api/v3/" +
                "workplaces?limit=100&offset=0&workplaceType=MACHINE_WORKPLACE", Method.GET);
            IRestResponse response = client.Execute(request);
            DeserializeClasses.Root allWorkplaces;

            allWorkplaces = JsonConvert.DeserializeObject<DeserializeClasses.Root>(response.Content);
            List<string> allWorkplacesIds = new List<string>();
            Dictionary<string, string> wpNoWpDescDict = new Dictionary<string, string>();

            foreach (var wp in allWorkplaces._embedded.workplaces)
            {
                allWorkplacesIds.Add(wp.properties.id);
                wpNoWpDescDict.Add(wp.properties.number, wp.properties.description);
            }
            return (allWorkplacesIds, wpNoWpDescDict);
        }

        public static async Task<List<Task<IRestResponse>>> DoIndividualRequestAsync(string bearer, List<string> workplaceIds)
        {
            var client = new RestClient();
            client.Authenticator = new JwtAuthenticator(bearer);
            var cancellationTokenSource = new CancellationTokenSource();

            var requestList = new List<RestRequest>();
            RestRequest genericRequest;
            string resource;

            foreach (var id in workplaceIds)
            {
                resource = "https://fcfhws.forcam.university/ffwebservices/customized/v3/AVODetailsRuesten-WS/APL-TT/" +
                    "?limit=100&formatted=true&timeZoneId=UTC&workplace=" + id + "&timeType=DAY&past=10&operationOperatingStatus=94505";
                genericRequest = new RestRequest(resource, Method.GET);
                genericRequest.AddHeader("Accept", "application/hal+json;charset=UTF-8");
                requestList.Add(genericRequest);
            }
            Console.WriteLine("Requests generated");

            var tasks = new List<Task<IRestResponse>>();
            Task<IRestResponse> response;

            int j = 5;

            for (int i = 0; i < requestList.Count; i++)
            {
                RestRequest request = requestList[i];
                response = client.ExecuteAsync(request, cancellationTokenSource.Token);
                tasks.Add(response);

                if (tasks.Count == j)
                {
                    await Task.WhenAll(tasks);
                    j += 3;
                    //await Task.Delay(5000);
                }
            }

            // Pause execution here until both tasks are complete
            await Task.WhenAll(tasks);
            Console.WriteLine("All Responses collected");
            foreach (var task in tasks)
            {
                if (task.Result.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("Error during request: " + task.Result.StatusCode.ToString());
                    Environment.Exit(1);
                }
                
            }
            return tasks;
        }

        public static List<Workplace> CreateWorkplaceObjects(List<Task<IRestResponse>> tasks, Dictionary<string, string> wpNoWpDescDict)
        {
            List<Root> WorkplaceObjects = new List<Root>();
            List<Workplace> workplaces = new List<Workplace>();
            Workplace workplace;
            SetupData setup;

            Root myDeserializedClass;
            foreach (var task in tasks)
            {
                myDeserializedClass = JsonConvert.DeserializeObject<Root>(task.Result.Content);
                WorkplaceObjects.Add(myDeserializedClass);
            }

            foreach (var obj in WorkplaceObjects)
            {
                if (obj._embedded != null)
                {
                    if (obj._embedded.AVODetailsSetupJVFC.Count > 0)
                    {
                        workplace = new Workplace();
                        workplace.Id = obj._embedded.AVODetailsSetupJVFC[0].properties.workplaceId;

                        foreach (var entry in wpNoWpDescDict)
                        {
                            if (entry.Key == workplace.Id)
                            {
                                workplace.Description = entry.Value;
                                break;
                            }
                        }

                        workplace.TargetSetupTime = obj._embedded.AVODetailsSetupJVFC[0].properties.targetSetupTime;
                        workplace.Setups = new List<SetupData>();

                        foreach (var prop in obj._embedded.AVODetailsSetupJVFC)
                        {
                            setup = new SetupData();
                            setup.setupDuration = prop.properties.setupDuration;
                            DateTime time = DateTime.ParseExact(prop.properties.setupEndTs, "M/d/yy, h:mm tt", CultureInfo.InvariantCulture);
                            setup.setupEndTs = MyExtensionClass.ToFormat24h(time);
                            time = DateTime.ParseExact(prop.properties.setupStartTs, "M/d/yy, h:mm tt", CultureInfo.InvariantCulture);
                            setup.setupStartTs = MyExtensionClass.ToFormat24h(time);
                            setup.setupTimeDeviationAbs = prop.properties.setupTimeDeviationAbs;
                            setup.setupTimeDeviationRel = prop.properties.setupTimeDeviationRel;
                            time = DateTime.ParseExact(prop.properties.targetStartDate, "M/d/yy, h:mm tt", CultureInfo.InvariantCulture);
                            setup.targetStartDate = MyExtensionClass.ToFormat24h(time);
                            setup.setupRate = prop.properties.setupRate;
                            workplace.Setups.Add(setup);
                            setup = null;
                        }
                        workplaces.Add(workplace);
                        workplace = null;
                    }
                }

            }
            return workplaces;
        }
        public static void CreateJsonFile(List<Workplace> workplaces)
        {
            var jsonString = JsonConvert.SerializeObject(workplaces);
            File.WriteAllText(@"C:\Users\alexa\Documents\workplaces.json", jsonString);
        }

        public static void UpdateDatabase(List<Workplace> workplaces)
        {
            using var context = new WorkplaceContext();
            List<SetupData> setupData = null;

            for (int i = 0; i < workplaces.Count; i++)
            { 
                //Prüfen ob workplace existiert
                if (context.Workplaces.Any(e => e.Description == workplaces[i].Description))
                {
                    //Prüfen ob Setups ungleich null
                    if (context.Workplaces.Where(e => e.Description == workplaces[i].Description).First().Setups != null)
                    {
                        setupData = context.Workplaces.Where(e => e.Description == workplaces[i].Description).First().Setups;
                    }
                    else context.Workplaces.Where(e => e.Description == workplaces[i].Description).First().Setups = new List<SetupData>();

                    foreach (var setup in workplaces[i].Setups)
                    {
                        if (setupData == null)
                        {
                            //context.Workplaces.Where(e => e.Description == workplaces[i].Description).First().Setups = new List<SetupData>();
                            context.Workplaces.Where(e => e.Description == workplaces[i].Description).First().Setups.Add(setup);
                        }
                        else if (!(context.Workplaces.Where(e => e.Description == workplaces[i].Description).First().Setups.Any(e => e.setupStartTs == setup.setupStartTs)))
                        {
                            context.Workplaces.Where(e => e.Description == workplaces[i].Description).First().Setups.Add(setup);
                        }
                    }
                }
                else
                {
                    context.Workplaces.Add(workplaces[i]);
                }
            }
            context.SaveChanges();
        }
    }

    public static class MyExtensionClass
    {
        public static DateTime ToFormat24h(this DateTime dt)
        {
            string datetime = dt.ToString("dd/MM/yyyy, HH:mm");
            return DateTime.Parse(datetime);
        }
    }
}
