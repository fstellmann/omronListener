using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mobile.Communication.Client;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace omronListener
{
    public class Worker : BackgroundService
    {
        public readonly ILogger<Worker> _logger;
        public struct action
        {
            public int id;
            public string ip;
            public int port;
            public string command;
            public int orderID;
        }

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            internalSQL._logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                List<action> lActions = new List<action>();
                lActions = internalSQL.getCommandList();
                

                lActions.Sort((s1, s2) => s1.orderID.CompareTo(s2.orderID));
                foreach(action a in lActions)
                {
                    bool done = executeAction(a);
                    if (done) internalSQL.updateRow(a);
                    else reportError(a);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private void reportError(action a)
        {
            _logger.LogWarning("Error while executing command {command} for machine {ip}:{port}", a.command, a.ip, a.port);
        }
        private void reportError(Exception exc)
        {
            _logger.LogError("{exc}", exc);
        }
        private bool executeAction(action a)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(a.ip), a.port);
            RobotClient robot = new RobotClient(a.ip, endPoint);
            robot.Password = Program.robotPassword;
            try
            {
                robot.Connect();
                robot.RefreshOutputCollection();
                robot.SendCommand(a.command);
                while (robot.Outputs[Program.reportOutput] != Program.desiredOutput)
                {
                    Task.Delay(30);
                }
                return true;
            }
            catch(Exception exc)
            {
                reportError(exc);
                return false;
            }
            finally
            {
                robot.Disconnect();
            }
        }
    }
}
