﻿using AsyncAwaitBestPractices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kit.Services.Web
{
    public class ScanIpAddress
    {
        static int upCount = 0;
        static object lockObj = new object();
        public List<string> Hosts { get; private set; }
        public bool ResolveNames { get; set; } = true;
        public async Task FindHosts(string ipBase)
        {
            await Task.Yield();
            Hosts = new List<string>();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 1; i < 255; i++)
            {
                string ip = $"{ipBase}.{i}";
                PingReply reply = await PingOrTimeout(ip, 250);
                OnPingReply(reply, ip);
            }
            sw.Stop();
            TimeSpan span = new TimeSpan(sw.ElapsedTicks);
            Console.WriteLine("Took {0} milliseconds. {1} hosts active.", span, upCount);
            Console.ReadLine();
        }

        public async Task<PingReply> PingOrTimeout(string hostname, int timeOut=250)
        {
            await Task.Yield();
            PingReply result = null;
            var cancellationTokenSource = new CancellationTokenSource();
            var timeoutTask = Task.Delay(timeOut, cancellationTokenSource.Token);

            var actionTask = Task.Factory.StartNew(() =>
             {
                 result = NormalPing(hostname, timeOut);
             }, cancellationTokenSource.Token);

            await Task.WhenAny(actionTask, timeoutTask).ContinueWith(t =>
             {
                 cancellationTokenSource.Cancel();
             });
            return result;
        }
        private static PingReply ForcePingTimeoutWithThreads(string hostname, int timeout)
        {
            PingReply reply = null;
            Thread a = new Thread(() => reply = NormalPing(hostname, timeout));
            a.Start();
            a.Join(timeout);
            return reply;
        }

        private static PingReply NormalPing(string hostname, int timeout)
        {
            try
            {
                return new Ping().Send(hostname, timeout);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "normalPing");
                return null;
            }
        }

        private void OnPingReply(PingReply reply, string ip)
        {
            if (reply != null && reply.Status == IPStatus.Success)
            {
                if (ResolveNames)
                {
                    string name;
                    try
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                        name = hostEntry.HostName;
                        Hosts.Add(name);
                    }
                    catch (SocketException ex)
                    {
                        Log.Logger.Error(ex, "SocketException");
                        name = "?";
                    }
                    Console.WriteLine("{0} ({1}) is up: ({2} ms)", ip, name, reply.RoundtripTime);
                }
                else
                {
                    Console.WriteLine("{0} is up: ({1} ms)", ip, reply.RoundtripTime);
                    Hosts.Add(ip);
                }
                lock (lockObj)
                {
                    upCount++;
                }
            }
            else if (reply == null)
            {
                Console.WriteLine("Pinging {0} failed. (Null Reply object?)", ip);
            }
        }

    }
}
