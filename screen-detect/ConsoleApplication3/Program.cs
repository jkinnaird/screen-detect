using System;
using System.IO;                        //File operations
using System.Diagnostics;               //CPU counter
using System.IO.Compression;            //Zip file
using System.Drawing;                   //Color
using System.Threading;                 //Sleep
using System.Windows.Forms;             //Screen
using System.Management;                //Managementobject, managementobjectsearcher, getpropertyvalue. Used for detecting EDID.
//using System.Net;                       //Web client

namespace ConsoleApplication3
{
    public class Program
    {
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDesktopWindow();
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindowDC(IntPtr window);
        [System.Runtime.InteropServices.DllImportAttribute("gdi32.dll", SetLastError = true)]
        public static extern uint GetPixel(IntPtr dc, int x, int y);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll", SetLastError = true)]
        public static extern int ReleaseDC(IntPtr window, IntPtr dc);

        static void callASC()
        {
            //upload json to asc
            /*
             string uri = //someURIvalue;
             WebClient server = new WebClient();
             string filePath = Path.Combine(source, "asc.json");
             myWebClient.UploadFile(uriString,fileName);
             */
        }   //change channel via ASC

        static void reboot()
        {
            //Process.Start("shutdown", "/r /c "BATS reboot" /t 0");
        }   //reboot player

        static void uploadFiles()
        {
            //aggregate files
            string logPath= @"D:\FTProot\InfoChannel_Player_5_Config_and_DATA\Application Data\Logs\IC.log";         //change
            //string ICpath= @"C:\Users\jkinnaird\Documents\Visual Studio 2015\Projects\ConsoleApplication3\IC.log";
            string planPath= @"D:\FTProot\InfoChannel_Player_5_Config_and_DATA\Application Data\Network\plan.xml";
            string source = Directory.GetCurrentDirectory();
            //string source= @"D:\BATS";
            string dest = Path.Combine(source, "Temp");
            //string dest = @"D:\BATS\Temp";

            File.Copy(Path.Combine(logPath, @"Logs\IC.log"), Path.Combine(dest, "IC.log"));
            File.Copy(planPath, Path.Combine(dest, "plan.xml"));                            //for player
            //File.Copy(ICpath, Path.Combine(dest, "IC.log"));
            //File.Copy(Path.Combine(source, "plan.xml"), Path.Combine(dest, "plan.xml"));  //for PC
            File.Copy(Path.Combine(source, "metrics.log"), Path.Combine(dest, "metrics.log"));
            File.Copy(Path.Combine(source, "error.log"), Path.Combine(dest, "error.log"));
            File.Copy(Path.Combine(source, "events.log"), Path.Combine(dest, "events.log"));
            File.Copy(Path.Combine(source, "config.ini"), Path.Combine(dest, "config.ini"));

            //create zip file
            ZipFile.CreateFromDirectory(dest, Path.Combine(source, "logs.zip"));
            //Thread.Sleep(5000); //temporary, to reduce errors when the subsequent lines delete this file.

            //upload files
            /*
             string uri = //someURIvalue;
             WebClient server = new WebClient();
             string filePath = Path.Combine(source, "logs.zip");
             myWebClient.UploadFile(uriString,fileName);
             */
            DateTime time = DateTime.Now;
            string renamedFile = time.Year.ToString() + time.Month.ToString() + time.Day.ToString() + time.Hour.ToString() + time.Minute.ToString() + time.Second.ToString() + "logs.zip";
            File.Copy(Path.Combine(source, "logs.zip"), Path.Combine(source, renamedFile));

            //delete all aggregated files and the zip file, so as to reduce HDD space needed
            System.IO.DirectoryInfo dir = new DirectoryInfo(dest);
            foreach (FileInfo file in dir.GetFiles())
            {
                file.Delete();
            }
            File.Delete(Path.Combine(source, "logs.zip"));
        }   //upload logs and other info

        static bool isScalaRunning()
        {
            bool isRunning = false;
            Process[] localByName = Process.GetProcessesByName("teamviewer");   //change
            //Process[] localByName = Process.GetProcessesByName("RunIC");
            if (localByName.Length>0)
            {
                if (localByName[0].Responding) { isRunning = true; }
                else { isRunning = false; }
            }
            return isRunning;
        }   //check if scala is running

        static bool isTransmissionRunning()
        {
            bool isRunning = false;
            Process[] localByName = Process.GetProcessesByName("teamviewer");   //change
            //Process[] localByName = Process.GetProcessesByName("NetIC_Service");
            if (localByName.Length > 0)
            {
                if (localByName[0].Responding) { isRunning = true; }
                else { isRunning = false; }
            }
            return isRunning;
        }

        static void performanceCounter()
        {
            DateTime init = DateTime.Now;
            DateTime current = DateTime.Now;
            TimeSpan difference;
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            PerformanceCounter memCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            PerformanceCounter diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
            //string path = @"D:\BATS\\metrics.log";      //change
            string path = Path.Combine(Directory.GetCurrentDirectory(), "metrics.log");
            using (System.IO.StreamWriter metrics = new System.IO.StreamWriter(path))
            {
                do
                {
                    current = DateTime.Now;
                    metrics.WriteLine(current + "\tCPU - " + cpuCounter.NextValue());
                    metrics.WriteLine("\t\t\tMemory - " + memCounter.NextValue());
                    metrics.WriteLine("\t\t\tDisk - " + diskCounter.NextValue());
                    metrics.WriteLine("");
                    difference = current - init;
                    metrics.Flush();
                    Thread.Sleep(500);
                } while (difference.Minutes <= 0);
            }
        }

        static player getData(player player)
        {
            Console.WriteLine("begin");
            IntPtr window = GetDesktopWindow();
            Process[] processes = Process.GetProcessesByName("runIC");
            foreach (Process p in processes)
            {
                window = p.MainWindowHandle;
                Console.WriteLine(window.ToString());
                //Thread.Sleep(5000);
            }
            IntPtr dc = GetWindowDC(window);
            int pixel = 0;

            int[,] points = new int[5, 2];
            /*int xQuad = player.getxRes() / 4;       //divide x axis into 4 areas
            int yQuad = player.getyRes() / 4;       //divide y axis into 4 areas
            points[0, 0] = xQuad; points[0, 1] = yQuad; //top left
            points[1, 0] = xQuad * 3; points[1, 1] = yQuad; //top right
            points[2, 0] = xQuad; points[2, 1] = yQuad * 3; //bottom left
            points[3, 0] = xQuad * 3; points[3, 1] = yQuad * 3; //bottom right
            points[4, 0] = xQuad * 2; points[4, 1] = yQuad * 2; //center*/

            /**
             * Set viewing region
            **/
            if(player.getResolution()==0 || player.getResolution() == 1)
            {                           //1080P
                if (player.getContentType() == 1)   //if DTV and 1080P
                {
                    points[0, 0] = 350 + 72; points[0, 1] = 153 + 220;
                    points[1, 0] = 1050 + 72; points[1, 1] = 153 + 220;
                    points[2, 0] = 350 + 72; points[2, 1] = 459 + 220;
                    points[3, 0] = 1050 + 72; points[3, 1] = 459 + 220;
                    points[4, 0] = 700 + 72; points[4, 1] = 306 + 220;
                }
                else                                //if 1080P and not DTV
                {
                    points[0, 0] = 480; points[0, 1] = 270;
                    points[1, 0] = 1440; points[1, 1] = 810;
                    points[2, 0] = 480; points[2, 1] = 270;
                    points[3, 0] = 1440; points[3, 1] = 810;
                    points[4, 0] = 960; points[4, 1] = 540;
                }
            }
            else if (player.getResolution()==2)
            {                           //720P
                if (player.getContentType() == 1)  //if DTV and 720P
                {
                    points[0, 0] = 243 + 50; points[0, 1] = 103 + 146;
                    points[1, 0] = 729 + 50; points[1, 1] = 103 + 146;
                    points[2, 0] = 243 + 50; points[2, 1] = 309 + 146;
                    points[3, 0] = 729 + 50; points[3, 1] = 309 + 146;
                    points[4, 0] = 486 + 50; points[4, 1] = 206 + 146;
                }
                else                                //if 720P and not DTV
                {
                    points[0, 0] = 320; points[0, 1] = 180;
                    points[1, 0] = 960; points[1, 1] = 540;
                    points[2, 0] = 320; points[2, 1] = 180;
                    points[3, 0] = 960; points[3, 1] = 540;
                    points[4, 0] = 640; points[4, 1] = 360;
                }
            }
            else
            {
                int xQuad = player.getxRes() / 4;       //divide x axis into 4 areas
                int yQuad = player.getyRes() / 4;       //divide y axis into 4 areas
                points[0, 0] = xQuad; points[0, 1] = yQuad; //top left
                points[1, 0] = xQuad * 3; points[1, 1] = yQuad; //top right
                points[2, 0] = xQuad; points[2, 1] = yQuad * 3; //bottom left
                points[3, 0] = xQuad * 3; points[3, 1] = yQuad * 3; //bottom right
                points[4, 0] = xQuad * 2; points[4, 1] = yQuad * 2; //center
            }

            for(int i=0; i<5; i++)
            {
                Console.WriteLine(points[i, 0] + " - " + points[i, 1]);
            }

            Thread metrics = new Thread(performanceCounter);
            metrics.Start();
            for (int pass = 0; pass < 4; pass++)   //run 4 passes in 1 minute
            {
                Console.WriteLine("pass: " + pass);
                for (int p = 0; p < 5; p++)     //check 5 points
                {
                    //Console.WriteLine("point: " + p);
                    bool black = true;
                    for (int d = 0; d < 2; d++)
                    {
                        for (int pi = 0; pi < 15; pi++)
                        {
                            if (d == 0) { pixel = (int)GetPixel(dc, points[p, 0], points[p, 1] + pi - 7); }
                            else { pixel = (int)GetPixel(dc, points[p, 0] + pi - 7, points[p, 1]); }

                            Color color = Color.FromArgb((int)(pixel & 0x000000FF),
                                (int)(pixel & 0x0000FF00) >> 8,
                                (int)(pixel & 0x00FF0000) >> 16);
                            byte r, g, b;
                            r = color.R;
                            g = color.G;
                            b = color.B;

                            if (r >= 10 && g >= 10 && b >= 10) { black = false; }   //if pixel is not mostly black, region is not black

                            player.values[p, d, pi, 0, pass] = (int)r;
                            player.values[p, d, pi, 1, pass] = (int)g;
                            player.values[p, d, pi, 2, pass] = (int)b;
                        }
                    }
                    player.isBlack[p, pass] = black;
                }
                if(pass<3)Thread.Sleep(15000);    //sleep 15 seconds between passes
            }
            metrics.Join(); //join the metrics thread
            Console.WriteLine("end");
            return player;
        }   //get all pixel data

        static bool checkIfFrozen(player cPlayer)
        {
            //Console.WriteLine("Checking for frozen content...");
            bool frozen = true;
            for (int p = 0; p < 5; p++) //for each point
            {
                for (int d = 0; d < 2; d++) //and each direction
                {
                    for (int pi = 0; pi < 15; pi++) //and each pixel
                    {
                        for (int c = 0; c < 3; c++) //and each color
                        {
                            if (cPlayer.values[p, d, pi, c, 0] != cPlayer.values[p, d, pi, c, 1]) { frozen = false; }   //check if pixel has changed between pass 0 and 1
                            else if (cPlayer.values[p, d, pi, c, 1] != cPlayer.values[p, d, pi, c, 2]) { frozen = false; }   //check if pixel has changed between pass 1 and 2
                            else if (cPlayer.values[p, d, pi, c, 2] != cPlayer.values[p, d, pi, c, 3]) { frozen = false; }   //check if pixel has changed between pass 2 and 3
                        }
                    }
                }
            }
            //Console.WriteLine("Content frozen: " + frozen);
            return frozen;
        }   //check if content is frozen

        static bool checkIfBlack(player cPlayer)
        {
            //Console.WriteLine("Checking for black content...");
            bool black = true;
            for (int i = 0; i < 5; i++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (cPlayer.isBlack[i, k] == false) { black = false; break; } //check if all regions on all passes were black
                }
            }
            //Console.WriteLine("Content black: " + black);
            return black;
        }   //check if content is black

        static bool isEdidActive()
        {
            bool edid = true;
            SelectQuery s = new SelectQuery("Win32_DesktopMonitor");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(s);
            foreach (ManagementObject display in searcher.Get())
            {
                string name = display.GetPropertyValue("Name").ToString();
                // show the class
                if (name.Equals("Generic Non-PnP Monitor")) { edid = false; }
                else { edid = true; }
                //Console.WriteLine("Generic Non-PnP Monitor" + " - " + name + " - " + edid);
            }
            return edid;
        }   //check for active display

        static void foundError(bool frozen, bool black, bool edid, bool scala, bool transmission, player cPlayer)
        {
            string errorCode = "";
            if (frozen) { errorCode = errorCode + "1"; }
            else { errorCode = errorCode + "0"; }
            if (black) { errorCode = errorCode + "1"; }
            else { errorCode = errorCode + "0"; }
            if (!edid) { errorCode = errorCode + "1"; }
            else { errorCode = errorCode + "0"; }
            if (!scala) { errorCode = errorCode + "1"; }
            else { errorCode = errorCode + "0"; }
            if (!transmission) { errorCode = errorCode + "1"; }
            else { errorCode = errorCode + "0"; }

            callASC();
            //string command = Path.Combine(Directory.GetCurrentDirectory(), "runReports.bat"); ;
            //System.Diagnostics.Process.Start("CMD.exe",command);

            //create error status log
            String path = Path.Combine(Directory.GetCurrentDirectory(), "error.log");      //change
            //string path = @"D:\BATS\error.log";

            using (System.IO.StreamWriter error = new System.IO.StreamWriter(path))
            {
                error.WriteLine(errorCode);
                //error.WriteLine(ifChannelChanged)
                //error.WriteLine(originalChannel)
                error.WriteLine(cPlayer.getPlayerName());
                error.WriteLine(cPlayer.getContentType());
                error.WriteLine(cPlayer.getxRes());
                error.WriteLine(cPlayer.getyRes());
                error.WriteLine(cPlayer.getResolution());
                for (int pass = 0; pass < 2; pass++)
                {
                    for (int p = 0; p < 5; p++)
                    {
                        error.WriteLine("");
                        for (int d = 0; d < 2; d++)
                        {
                            if (d == 0) { error.WriteLine("Point " + p + " horizontal on pass" + pass); }
                            else { error.WriteLine("");  error.WriteLine("Point " + p + " vertical"); }
                            for (int c = 0; c < 3; c++)
                            {
                                for (int pi = 0; pi < 15; pi++)
                                {
                                    if (pi == 0)
                                    {
                                        if (c == 0) { error.Write("r - "); }
                                        if (c == 1) { error.Write(" g - "); }
                                        if (c == 2) { error.Write(" b - "); }
                                    }
                                    error.Write(cPlayer.values[p, d, pi, c, pass]);
                                }
                            }
                        }
                    }
                }
            }
            bool supportConnected = false;
            Process[] TeamViewer = Process.GetProcessesByName("TeamViewer_Desktop");
            Process[] MSP = Process.GetProcessesByName("BASupTSHelper");
            if (TeamViewer.Length > 0) { supportConnected = true; }
            if (MSP.Length > 0) { supportConnected = true; }

            if(supportConnected)
            {
                String eventPath = Path.Combine(Directory.GetCurrentDirectory(), "events.log");    //change
                //String eventPath = @"D:\BATS\events.log";
                using (StreamWriter appLog = File.AppendText(eventPath)) //output what program found
                {
                    appLog.WriteLine(DateTime.Now + " Support connection detected");
                }
            }
            else
            {
                //upload config.ini, plan.xml, IC log, CPU log, player name, time, date etc...
                uploadFiles();
                //once upload is complete, reboot machine
                reboot();
            }
        }   //report error and attempt to correct

        static int getContentType()
        {
            int contentType = 0;
            String path = @"D:\FTProot\InfoChannel_Player_5_Config_and_DATA\Application Data\Network\plan.xml";     //change
            //String path = Path.Combine(Directory.GetCurrentDirectory(), "plan.xml");
            if (File.Exists(path))
            {
                //Console.WriteLine("File exists");
                string temp = "";
                using (System.IO.StreamReader file = new System.IO.StreamReader(path))
                {
                    bool found = false;
                    do
                    {
                        temp = file.ReadLine();
                        if (temp != null)
                        {
                            if (temp.Contains("DTV")) { contentType = 1; found = true; }
                            //if (temp.Contains("GLOBAL_DSH") || temp.Contains("GLOBAL_DSV") || temp.Contains("GLOBAL_VWH")) { contentType = 0; found = true; }
                            //else if (temp.Contains("GLOBAL_MBH")) { contentType = 1; found = true; }
                            //else if (temp.Contains("BOTBAR") || temp.Contains("DTV_GLOBAL_CHANNEL_V1")) { contentType = 2; found = true; }
                            //else if (temp.Contains("TOPBAR")) { contentType = 3; found = true; }
                        }
                        else if (temp == null) { found = true; }
                    } while (found == false);
                }
            }
            return contentType;
        }   //gets the channel type

        static void createConfig()
        {
            String path = Path.Combine(Directory.GetCurrentDirectory(), "config.ini"); //change
            //String path = @"D:\BATS\config.ini";
            using (System.IO.StreamWriter config = new System.IO.StreamWriter(path))
            {
                string name = Environment.MachineName;
                int x = Screen.PrimaryScreen.Bounds.Width;
                int y = Screen.PrimaryScreen.Bounds.Height;
                int CT = getContentType();
                /*int offset = 0;
                if (CT == 3) { offset = y - 524; }
                else if (CT==2 || CT==3) { x = 932; y = 524; }
                int timer = 7000;
                if (CT == 0 || CT == 1) { timer = 8000; }
                */
                int res = 0;    //default assumes resolution of 1080P
                if (x == 1920 && y == 1080) { res = 1; }    //resolution is 1080P
                else if(x==1280 && y == 720) { res = 2; }   //resolution is 720P

                config.WriteLine(name);
                config.WriteLine(CT);
                config.WriteLine(x);
                config.WriteLine(y);
                config.WriteLine(res);
            }
        }   //creates configuration file

        static player readConfig()
        {
            player cPlayer = new player();
            bool configInitFailed = false;
            String path = Path.Combine(Directory.GetCurrentDirectory(), "config.ini"); //change
            //String path = @"D:\BATS\config.ini";
            using (System.IO.StreamReader config = new System.IO.StreamReader(path))
            {
                bool isANum = true;
                int tempInt;
                string temp = "";
                //get player name
                string name = config.ReadLine();
                //get content type
                int CT = 0;
                temp = config.ReadLine();
                isANum = Int32.TryParse(temp, out tempInt);
                if (isANum) { CT = tempInt; } else { configInitFailed = true; }
                //get x-resolution
                int x = 0;
                temp = config.ReadLine();
                isANum = Int32.TryParse(temp, out tempInt);
                if (isANum) { x = tempInt; } else { configInitFailed = true; }
                //get x-resolution
                int y = 0;
                temp = config.ReadLine();
                isANum = Int32.TryParse(temp, out tempInt);
                if (isANum) { y = tempInt; } else { configInitFailed = true; }
                //get resolution
                int res = 0;
                temp = config.ReadLine();
                isANum = Int32.TryParse(temp, out tempInt);
                if (isANum) { res = tempInt; } else { configInitFailed = true; }

                if (!configInitFailed)
                {
                    cPlayer.setPlayerName(name);
                    cPlayer.setContentType(CT);
                    cPlayer.setxRes(x);
                    cPlayer.setyRes(y);
                }
                else cPlayer.setPlayerName("failed");
            }
            return cPlayer;
        }   //reads configuration file and returns a basically configured object

        static void mainThread()
        {
            player cPlayer = new player();
            cPlayer = readConfig();             //initialize player object
            DateTime playerTime = DateTime.Now; //hold time information
            String eventPath = Path.Combine(Directory.GetCurrentDirectory(), "events.log");    //change
           // String eventPath = @"D:\BATS\events.log";

            // This text is added only once to the file.
            if (!File.Exists(eventPath))
            {
                // Create a file to write to.
                using (StreamWriter appLog = File.CreateText(eventPath))
                {
                    appLog.WriteLine(DateTime.Now + " opening log");
                    if (cPlayer.getResolution() == 0) { appLog.WriteLine(DateTime.Now + " using default resolution settings"); }
                }
            }


            int hour = 0;
            String errorPath = Path.Combine(Directory.GetCurrentDirectory(), "error.log"); //change
            //String errorPath = @"D:\BATS\error.log";
            while (File.Exists(errorPath)) //if player is already in error
            {
                Console.WriteLine("Recovering from error...");
                cPlayer = getData(cPlayer);             //check screen and store data
                bool frozen = checkIfFrozen(cPlayer);   //check the data for frozen content
                bool black = checkIfBlack(cPlayer);     //check the data for black content
                bool activeEDID = isEdidActive();       //check for an active display
                bool scalaActive = isScalaRunning();    //check to make sure scala is running
                
                Console.WriteLine(DateTime.Now + "\t Frozen: " + frozen + "\t Black: " + black + "\t Active Edid: " + activeEDID + "\t Scala running: " + scalaActive);
                using (StreamWriter appLog = File.AppendText(eventPath)) //output what program found
                {
                    appLog.WriteLine(DateTime.Now + " Frozen: " + frozen + " Black: " + black + " Active Edid: " + activeEDID + " Scala running: " + scalaActive);
                }
                if (!frozen && !black && activeEDID && scalaActive) { File.Delete(errorPath); Console.WriteLine("Error recovered."); }
            }
            playerTime = DateTime.Now;
            hour = playerTime.Hour;
            do
            {
                while (hour >= 6 && hour < 22)
                {
                    playerTime = DateTime.Now;
                    hour = playerTime.Hour;
                    cPlayer = getData(cPlayer);             //check screen and store data
                    bool frozen = checkIfFrozen(cPlayer);   //check the data for frozen content
                    bool black = checkIfBlack(cPlayer);     //check the data for black content
                    bool activeEDID = isEdidActive();       //check for an active display
                    bool scalaActive = isScalaRunning();    //check to make sure scala is running
                    bool transmissionActive = isTransmissionRunning();

                    Console.WriteLine(DateTime.Now + "\t Frozen: " + frozen + "\t Black: " + black + "\t Active Edid: " + activeEDID + "\t Scala running: " + scalaActive);
                    using (StreamWriter appLog = File.AppendText(eventPath)) //output what program found
                    {
                        appLog.WriteLine(DateTime.Now + " Frozen: " + frozen + " Black: " + black + " Active Edid: " + activeEDID + " Scala running: " + scalaActive);
                    }

                    String metricsPath = Path.Combine(Directory.GetCurrentDirectory(), "metrics.log");     //change
                    //String metricsPath = @"D:\BATS\metrics.log";
                    if (frozen || black || !activeEDID || !scalaActive || !transmissionActive) { foundError(frozen, black, activeEDID, scalaActive, transmissionActive, cPlayer); } //if there was an error, report the error and act accordingly
                    else if (File.Exists(metricsPath))
                    {
                        File.Delete(metricsPath);   //otherwise delete the metrics log
                    }
                    Thread.Sleep(60000);
                    playerTime = DateTime.Now;
                    hour = playerTime.Hour;
                }
                Thread.Sleep(60000);    //Wait 1 minute and check time again
                playerTime = DateTime.Now;
                hour = playerTime.Hour;
            } while (hour <= 6 || hour >= 22);
        }

        static void Main()  //runs on service startup
        {
            player cPlayer = new player();
            /*begin config*/
            bool configInitFailed = false;
            String path = Path.Combine(Directory.GetCurrentDirectory(), "config.ini");  //change
            //String path = @"D:\BATS\config.ini";
            String eventPath = Path.Combine(Directory.GetCurrentDirectory(), "events.log");    //change
            //String eventPath = @"D:\BATS\events.log";

            if (File.Exists(eventPath)) { File.Delete(eventPath); }     //start with a new log each day
            if (File.Exists(path)) { cPlayer = readConfig(); }          //if a config file exists, attempt to read it
            if (cPlayer.getPlayerName() == "failed") { configInitFailed = true; }   //if reading file failed, create a new one
            //create config
            if (!File.Exists(path) || configInitFailed == true)         //create config file if read filed or first run
            {
                if (configInitFailed) { File.Delete(path); }            //delete original if it exists
                createConfig();                                         //create new one
            }
            /*end config*/
            mainThread();
        }
    }
}