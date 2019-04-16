using System;
using RGiesecke.DllExport;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;


public class DynGPConnector
{

    static List<IntPtr> pointers = new List<IntPtr>();

    static Logger logger = new Logger();


    [DllExport("UpdateSalesDocs", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
    [return: MarshalAs(UnmanagedType.LPStr)]
    static StringBuilder UpdateSalesDoc(string WriteBackXML, string LogFile, string GPServer, string GPPort)
    {
        string UpdateResponse = "";
        
        //IntPtr memPtr = IntPtr.Zero;
            
        try
        {
            logger.logfile = LogFile;
            logger.Log("Update Fulfilled Qty - START");
            logger.Log("Input XML = " + WriteBackXML);
            var DocList = new DocumentList();
            DocList.LoadFromXML(WriteBackXML);
            logger.Log("Update Fulfilled Qty - Successfully Loaded WriteBackXML");
            DocList.LogMe(logger);

            // exit if no documents are of type that need write-back!!!
            if (DocList.AnyNeedFulfillment())
            {
                var GPIF = new GPInterface(logger);
                GPIF.UpdateDocuments(DocList, GPServer, GPPort);
                UpdateResponse = "Successfully updated Fulfilled Qty!";
                for (int i = 0; i < DocList.Documents.Length; i++)
                {
                    if (DocList.Documents[i].RequiredUpdate)
                    {
                        UpdateResponse = UpdateResponse + "\t" + DocList.Documents[i].CombinedKey;
                    }
                }
            }
            else
            {
                logger.Log("Document type(s) do not require fulfillment.");
                UpdateResponse = "Successfully updated Fulfilled Qty!";
            }
    

        }
        catch (System.Exception ex)
        {
            UpdateResponse = ex.Message;
        }

        logger.Log(UpdateResponse);
          
        //memPtr = Marshal.StringToHGlobalAnsi(UpdateResponse);
        //pointers.Add(memPtr);
        //return memPtr;
        return new StringBuilder(UpdateResponse);
    }


    [DllExport("FreeUnmanagedMemory", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
    static void FreeUnmanagedMemory(IntPtr APointer)
    {
        if (pointers.Exists(t => t == APointer))
        {
            Marshal.ZeroFreeGlobalAllocAnsi(APointer);
            pointers.Remove(APointer);
        }
        else
        {
            logger.Log(string.Format("ATTENTION - possible memory leak - address {0} is freeing but it was not allocated.", APointer));
        }
    }

 
}
