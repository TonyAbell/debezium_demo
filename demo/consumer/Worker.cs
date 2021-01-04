using Confluent.Kafka;
using dal.Data;
using dal.Models;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace consumer
{
    public class Worker : IHostedService
    {
        private readonly SchoolContext context;
        private readonly ILogger<Worker> logger;
        private readonly KafkaSettings settings;
        public Worker(SchoolContext context, KafkaSettings settings,  ILogger<Worker> logger)
        {
            this.context = context;
            this.settings = settings;
            this.logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Starting Worker");
            
                var config = new ConsumerConfig
                {
                    BootstrapServers = settings.BootstrapServers,
                    GroupId = settings.GroupId,
                    AutoOffsetReset = AutoOffsetReset.Earliest

                };

                using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
                {
                    
                    var topics = new List<string>() { "postgres.public.Student", "postgres.public.Course" };
                    logger.LogInformation("Subscribing To:");
                    topics.ForEach(f => logger.LogInformation($"\t{f}"));
                    consumer.Subscribe(topics);
                    logger.LogInformation("Subscribed");
                    while (true)
                    {
                        var consumeResult = consumer.Consume();

                        if (!String.IsNullOrEmpty(consumeResult.Message.Value)) {


                            var json = JObject.Parse(consumeResult.Message.Value);
                            //var jsonString = JsonConvert.SerializeObject(json, Formatting.Indented, new JsonConverter[] { new StringEnumConverter() });
                            //Console.WriteLine(jsonString);

                            var name = json["schema"]["name"].ToString();
                            logger.LogInformation($"Processing Change For: {name}");
                            if (name == "postgres.public.Student.Envelope")
                            {

                                var op = json["payload"]["op"].ToString();
                                logger.LogInformation($"Change Op: {op}");
                                if (op == "c" || op == "r")
                                {
                                    var studentJsonStr = json["payload"]["after"].ToString();
                                    logger.LogInformation(studentJsonStr);
                                    var student = JsonConvert.DeserializeObject<Student>(studentJsonStr);
                                    if (context.Students.Any(w => w.ID == student.ID))
                                    {
                                        // Student Exists do nothing
                                        logger.LogInformation("Student Exists: Do Nothing");
                                    }
                                    else
                                    {
                                        logger.LogInformation($"Student Exists, Adding StudentID: {student.ID}");
                                        context.Add(new Student()
                                        {
                                            ID = student.ID,
                                            EnrollmentDate = student.EnrollmentDate,
                                            FirstMidName = student.FirstMidName,
                                            LastName = student.LastName
                                        });
                                        await context.SaveChangesAsync();
                                    }

                                }
                                else if (op == "u")
                                {
                                    var studentJsonStr = json["payload"]["after"].ToString();
                                    //Console.WriteLine(studentJsonStr);
                                    var student = JsonConvert.DeserializeObject<Student>(studentJsonStr);

                                    var toUpdate = context.Students.FirstOrDefault(w => w.ID == student.ID);
                                    toUpdate.EnrollmentDate = student.EnrollmentDate;
                                    toUpdate.FirstMidName = student.FirstMidName;
                                    toUpdate.LastName = student.LastName;
                                    await context.SaveChangesAsync();
                                    logger.LogInformation($"Student Updated, StudentID: {student.ID}");
                                    // update record
                                }
                                else if (op == "d")
                                {
                                    //Console.WriteLine(json["payload"]["before"].ToString());
                                    var studentId = int.Parse(json["payload"]["before"]["ID"].ToString());
                                    logger.LogInformation($"Student Removed, Student ID {studentId}");
                                    //var student = JsonConvert.DeserializeObject<Student>(studentJsonStr);
                                    var toRemove = context.Students.FirstOrDefault(w => w.ID == studentId);
                                    if (toRemove != null)
                                    {
                                        //Console.WriteLine("Found Student To Remvoe");
                                        context.Remove(toRemove);
                                        await context.SaveChangesAsync();
                                        //Console.WriteLine("Remvoed");
                                    }
                                }
                            }
                        } else
                        {
                            logger.LogWarning("Received An Empty Message: TODO"); // Why
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Exception");
                logger.LogError(ex.Message);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(0);
            logger.LogInformation("Stoping");
        }
    }
}