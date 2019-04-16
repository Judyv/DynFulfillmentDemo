using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


public class Logger
{
  
    public string logpath { get; set; }
    private string logfile { get; set; }
    
    public Logger(string apath)
    {
        logpath = apath;
        logfile = apath + DateTime.Today.ToString("yyyyMMdd") + ".txt";
    }
    

    public void Log(string logMessage)
    {
        if (logfile != "")            
        using (StreamWriter w = File.AppendText(logfile))
        {
            w.WriteLine("[{0} {1}] : {2}", System.Environment.MachineName, DateTime.Now.ToLongTimeString(),
                    logMessage);
                
        }
    }

    
}
