using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Common;
using Microsoft.Dynamics.GP;
using System.ServiceModel;
//using TestDynGPConnect.DynamicsGPService;

public class DynConnection
{   
        public bool IsConnected = false;
        public string CurrentServer = "";
        public string CurrentPort = "";
        public Company[] companyList;
        public Batch[] batchList;
        public SalesProcessHoldSetup[] Holds;
        public ShippingMethod[] ShipMethods;
        public DynamicsGPClient wsDynamicsGP = null;

        private string currentcompany = "";  
        public Context context = new Context();
        private CompanyKey companyKey = new CompanyKey();
    
        public int IndexOfCompanyName(string companyname)
        {
            int idx = -1;
            for (int i = 0; i < companyList.Length; i++)
                    {
                        if (companyList[i].Name == companyname)
                        {
                           idx = i;
                           break;
                        }

                    }
            return idx;
        }

       
        public string CurrentCompany
        {
            get
            {
                return currentcompany;
            }
            set
            {
                if (value != currentcompany)
                {
                    
                    int j = IndexOfCompanyName(value);
                    if (j < 0)
                        throw new System.Exception("Company "+value+" not found");
                    companyKey.Id = companyList[j].Key.Id;                        
                    // Set up the context
                    context.OrganizationKey = (OrganizationKey)companyKey;
                    // Set up the context object
           
                    currentcompany = value;
                }
            }
        }

       
        public void CheckConnection(string GPServer, string GPPort)
        {
            bool testfailed = false;
            if (wsDynamicsGP.State != CommunicationState.Faulted)
            {
                try
                {
                }
                catch
                {
                    testfailed = true;
                }
            }
            if ((wsDynamicsGP.State == CommunicationState.Faulted) || (testfailed))
            {
                IsConnected = false;
                DoConnect(GPServer, GPPort);                
                IsConnected = true;                
            }
        }

        private void DoConnect(string GPServer, string GPPort)
        {
            //BasicHttpBinding binding = new BasicHttpBinding();  <-- doesn't work
            WSHttpBinding binding = new WSHttpBinding();
            binding.MessageEncoding = WSMessageEncoding.Text;
            binding.TextEncoding = System.Text.Encoding.UTF8;

            binding.ReaderQuotas.MaxDepth = 2147483647;
            binding.ReaderQuotas.MaxStringContentLength = 2147483647;
            binding.ReaderQuotas.MaxArrayLength = 2147483647;
            binding.ReaderQuotas.MaxBytesPerRead = 2147483647;
            binding.ReaderQuotas.MaxNameTableCharCount = 2147483647;
            binding.MaxReceivedMessageSize = 2147483647;

            binding.Security.Mode = SecurityMode.Message;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
            binding.Security.Message.NegotiateServiceCredential = true;
            // binding.Security.Message.AlgorithmSuite = default;
            binding.Security.Message.EstablishSecurityContext = true;

            // service identity - default is blank DNS but user can change in registry
            // blank dns - http://msdn.microsoft.com/en-us/library/bb628618.aspx
            EndpointIdentity endpointIdentity;
            //endpointIdentity = EndpointIdentity.CreateDnsIdentity("");
            endpointIdentity = EndpointIdentity.CreateSpnIdentity("");

            //EndpointAddress epa = new EndpointAddress("http://DEV00:48620/Dynamics/GPService/GPService");
            //EndpointAddress epa = new EndpointAddress("http://" + GPServer + ":" + GPPort + "/Dynamics/GPService/GPService");                        
            System.ServiceModel.Channels.AddressHeaderCollection addressHeaderColl = null;
            Uri uri = new Uri("http://" + GPServer + ":" + GPPort + "/Dynamics/GPService/GPService");
            EndpointAddress epa = new EndpointAddress(uri, endpointIdentity, addressHeaderColl);

            wsDynamicsGP = new DynamicsGPClient(binding, epa);
        }

        public void Connect(string GPServer, string GPPort)
        {
            if (IsConnected)
            {
                if ((GPServer != CurrentServer) || (GPPort != CurrentPort))
                {
                    Disconnect();
                }
            }
            // Create an instance of the service
            if ((IsConnected == false) && (GPServer.Trim().Length > 0) &&
                (GPPort.Trim().Length > 0))
            {
                try
                {
                    if (wsDynamicsGP != null)
                    {
                        if (wsDynamicsGP.State != CommunicationState.Faulted)
                        {
                            wsDynamicsGP.Close();
                            wsDynamicsGP = null;
                        }
                    }


                    //gplog.Log("Endpoint address = " + "http://" + GPServer + ":" + GPPort + "/Dynamics/GPService/GPService");
                    //gplog.Log("  Identity = " + DocList.Identity + " Type = " + DocList.IdentityType.ToString());
                    if (wsDynamicsGP == null)
                    {

                        DoConnect(GPServer, GPPort);
                        
                        LoadCompanyList();
                        IsConnected = true;
                    }


                }
                catch (System.Exception ex)
                {
                    throw new Exception("Error connecting to Dynamics Web Services : " + ex.Message);
                    
                }
            }
        }


        protected void LoadCompanyList()
        {
            Context context;
            BetweenRestrictionOfNullableOfint companyRestriction;
            CompanyCriteria companyCriteria;

            try
            {

                // Create a context object
                context = new Context();

                // Set up the context object
                // To retrieve from the system database set the organization key to null
                context.OrganizationKey = null;

                // Create a restriction object to query by company ID
                // Query for all possible company ID values
                companyRestriction = new BetweenRestrictionOfNullableOfint();
                companyRestriction.From = -32768;
                companyRestriction.To = 32767;

                // Create a company criteria object and add the restriction object 
                companyCriteria = new CompanyCriteria();
                companyCriteria.Id = companyRestriction;

                // Retrieve the list of companies
                companyList = wsDynamicsGP.GetCompanyList(companyCriteria, context);

            }

            catch (System.Exception ex)
            {
                throw new System.Exception("Error loading company list : " + ex.Message);
            }

        }

        private string PolicyForType(String DocType)
        {
            if (DocType == "Order")
            {
                return "UpdateSalesOrder";
            }
            else if (DocType == "Invoice")
            {
                return "UpdateSalesInvoice";
            }
            else if (DocType == "Fulfillment Order")
            {
                return "UpdateSalesFulfillmentOrder";
            }
            else
                return "UpdateSalesOrder";
        }

        public void UpdateSalesDocument(SalesDocument salesdocument)
        {
            Policy salesPolicy = wsDynamicsGP.GetPolicyByOperation("UpdateSalesOrder", context);

            if (salesdocument.Type == SalesDocumentType.Order)
                wsDynamicsGP.UpdateSalesOrder(salesdocument as SalesOrder, context, salesPolicy);
            else if (salesdocument.Type == SalesDocumentType.Invoice)
                wsDynamicsGP.UpdateSalesInvoice(salesdocument as SalesInvoice, context, salesPolicy);
            else if (salesdocument.Type == SalesDocumentType.FulfillmentOrder)
                wsDynamicsGP.UpdateSalesFulfillmentOrder(salesdocument as SalesFulfillmentOrder, context, salesPolicy);
            else
                throw new System.InvalidOperationException("Invalid document type " + salesdocument.Type.ToString());

        }

        
        public void RemoveHold(SalesDocument salesdocument, string HoldKey)
        {
        
            for (int i = 0; i < salesdocument.ProcessHolds.Length; i++)
            {
                if ((salesdocument.ProcessHolds[i].Key.SalesProcessHoldSetupKey.Id == HoldKey) &&
                    (salesdocument.ProcessHolds[i].IsDeleted == false))
                {
                    salesdocument.ProcessHolds[i].DeleteOnUpdate = true;
                }
                else
                    salesdocument.ProcessHolds[i].DeleteOnUpdate = salesdocument.ProcessHolds[i].IsDeleted;
            }
        }

        public void Disconnect()
        {
            if (wsDynamicsGP == null)
            {
                //gplog.Log("Disconnecting : proxy was not instantiated");   
                IsConnected = false;
            }
            else
                      // Close the service
                    if (wsDynamicsGP.State != CommunicationState.Faulted)
                    {
                        wsDynamicsGP.Close();
                        //gplog.Log("Disconnecting : closed successfully");   
                    }
                    else
                    {
                        //gplog.Log("Disconnecting : was faulted");                   
                    }
                    IsConnected = false;

              

        }

        public SalesDocument GetSalesDocumentByKey(string Key)
        {
            SalesDocumentKey SalesKey;

            SalesKey = new SalesDocumentKey();
            SalesKey.Id = Key;

            SalesDocument tmpDoc = null;

            tmpDoc = wsDynamicsGP.GetSalesOrderByKey(SalesKey, context);
            if (tmpDoc != null)
                return tmpDoc;
            tmpDoc = wsDynamicsGP.GetSalesInvoiceByKey(SalesKey, context);
            if (tmpDoc != null)
                return tmpDoc;
            tmpDoc = wsDynamicsGP.GetSalesFulfillmentOrderByKey(SalesKey, context);
            if (tmpDoc != null)
                return tmpDoc;
            else
                return null;

        }
    }
