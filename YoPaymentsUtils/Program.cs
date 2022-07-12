using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoPaymentsUtils
{
    class Program
    {
        //Set this to the right path of the ceritificate file.
        private static string publicKeyFile = "Yo_Uganda_Public_Certificate.crt";
        //Set this to the right path of your private key (in .pem format)
        private static string privateKeyFile = "private_key.key";
        /*
        * Replace the following URL with the production one if you are working in the production.
        */
        private static string url = "https://sandbox.yo.co.ug/services/yopaymentsdev/task.php";
        private static string username = "";//Your YoPayments APIUsername
        private static string password = "";//API Password

        public static void Main(string[] args)
        {
            //testAcDepositFunds();
            testAcWithdrawFunds();
            //testYoSignatureVerifier();
            //testAcTransactionCheckStatus();
        }

        public static void testAcWithdrawFunds()
        {
            /*
            * If you don't have a private key, set privateKeyFile to empty string 
            * and use runWithdrawFundsAPiWithoutSignature method instead.  
            */
            YoPaymentsUtils yoGw = new YoPaymentsUtils(username, password, url, privateKeyFile);

            String account = "256783086794";
            String amount = "1000";
            string narrative = "Sample Withdraw request";
            String reference = account + "-" + yoGw.getUniqueId();
            YoPaymentsResponse r = yoGw.runWithdrawFundsAPi(account, amount, narrative, reference);
            if (yoGw.error.Length > 0)
            {
                Console.WriteLine("Error: " + yoGw.error + "\n\n");
            }
            if (r == null)
                return;

            Console.WriteLine("Trace: " + yoGw.trace + "\n\n");
            Console.WriteLine(r.toString());

            //Now you can use the response fields
            Console.WriteLine("Status: " + r.getStatus());//This can be | OK | ERROR
            Console.WriteLine("StatusCode: " + r.getStatusCode());
            Console.WriteLine("StatusMessage: " + r.getStatusMessage());
            Console.WriteLine("ErrorMessage: " + r.getErrorMessage());
            Console.WriteLine("TransactionReference: " + r.getTransactionReference());
            Console.WriteLine("TransactionStatus: " + r.getTransactionStatus());
            Console.WriteLine("NetworkRefer: " + r.getNetworkRef());

            /*
            *            
            * If Status is ERROR, check StatusMessage and ErrorMessage for the error.
            * If TransactionStatus is set to SUCCEEDED, then the transaction was successful.
            * Obtain the Network reference from NetworkRefer
            *            
            */

        }

        public static void testAcDepositFunds()
        {

            YoPaymentsUtils yoGw = new YoPaymentsUtils(username, password, url, "");

            String account = "256783086794";
            String amount = "1000";
            string narrative = "Sample deposit request";
            String reference = account + "-" + yoGw.getUniqueId();
            YoPaymentsResponse r = yoGw.runDepositFundsApi(account,
                amount,
                narrative,
                reference,
                "",
                "");

            if (yoGw.error.Length > 0)
            {
                Console.WriteLine("Error: " + yoGw.error + "\n\n");
            }
            if (r == null)
                return;

            Console.WriteLine("Trace: " + yoGw.trace + "\n\n");
            Console.WriteLine(r.toString());

            //Now you can use the response fields
            Console.WriteLine("Status: " + r.getStatus());//This can be | OK | ERROR
            Console.WriteLine("StatusCode: " + r.getStatusCode());
            Console.WriteLine("StatusMessage: " + r.getStatusMessage());
            Console.WriteLine("ErrorMessage: " + r.getErrorMessage());
            Console.WriteLine("TransactionReference: " + r.getTransactionReference());
            Console.WriteLine("TransactionStatus: " + r.getTransactionStatus());
            Console.WriteLine("NetworkRefer: " + r.getNetworkRef());

            /*
            *            
            * If Status is ERROR, check StatusMessage and ErrorMessage for the error.
            * If TransactionStatus is set to SUCCEEDED, then the transaction was successful.
            * If TransactionStatus is set to PENING, you will have to check for the status later.
            * Obtain the Network reference from NetworkRefer
            *            
            */

        }

        public static void testAcTransactionCheckStatus()
        {

            YoPaymentsUtils yoGw = new YoPaymentsUtils(username, password, url, "");

            YoPaymentsResponse r = yoGw.runTransactionCheckstatusApi("256783086794-be1f2212-9c76-423d-9431-a1d4858df2b5");

            if (yoGw.error.Length > 0)
            {
                Console.WriteLine("Error: " + yoGw.error + "\n\n");
            }
            if (r == null)
                return;

            Console.WriteLine("Trace: " + yoGw.trace + "\n\n");
            Console.WriteLine(r.toString());

            //Now you can use the response fields
            Console.WriteLine("Status: " + r.getStatus());//This can be | OK | ERROR
            Console.WriteLine("StatusCode: " + r.getStatusCode());
            Console.WriteLine("StatusMessage: " + r.getStatusMessage());
            Console.WriteLine("ErrorMessage: " + r.getErrorMessage());
            Console.WriteLine("TransactionReference: " + r.getTransactionReference());
            Console.WriteLine("TransactionStatus: " + r.getTransactionStatus());
            Console.WriteLine("NetworkRefer: " + r.getNetworkRef());

            /*
            *            
            * If Status is ERROR, check StatusMessage and ErrorMessage for the error.
            * If TransactionStatus is set to SUCCEEDED, then the transaction was successful.
            * If TransactionStatus is set to PENING, you will have to check for the status later.
            * Obtain the Network reference from NetworkRefer
            *            
            */

        }

        public static void testYoSignatureVerifier()
        {
            /*In your code, obtain the following data from the POST fields of the request*/
            String date_time = "2019-06-21 10:09:21";
            String network_ref = "1462967855";
            String external_ref = "3-Joseph-Tabajjwa";
            String msisdn = "256783086794";
            String narrative = "Testing";
            String amount = "2000";
            String signature = "RTyGxIwp83Lb9Lo03yGOSyXKDjY3vmgvPjoOzFb79CWvUttvnnFh4Ln1/Ur71YucjXpkfTdhdz2GyLAWVtCxl3iqox3haZMIX/9JVcYh4tt5zipwUo0CLgRVehsyJlUs70ph7TJ1KU/qMcOz60HWLsJDPv95n4Dqdh3bTHg/f+XovxD5Qde7sGEeXWnAQBlq5Bb2dFtw9k6vyI+4BE5h++CKgCr/7wzvKM3hij4mTqIRW0Z+DtZK7cIgtmckr0w7F9eW+YCiymTRP4sdRqinEvDADW49/dDLq1gTnO83RxpSTmHw5NavvRGjszC3Fgub5t2gT52Kr9oNHZhgiBZIDg==";

            YoSignatureVerifier yoVerifierObj = new YoSignatureVerifier(publicKeyFile);
            yoVerifierObj.SetDateTime(date_time);
            yoVerifierObj.SetNetworkRef(network_ref);
            yoVerifierObj.SetExternalRef(external_ref);
            yoVerifierObj.SetMsisdn(msisdn);
            yoVerifierObj.SetNarrative(narrative);
            yoVerifierObj.SetAmount(amount);
            yoVerifierObj.SetSignature(signature);

            if (yoVerifierObj.verify())
            {
                Console.WriteLine("Signature verification passed!");
            }
            else
            {
                Console.WriteLine("Error");
                Console.WriteLine(yoVerifierObj.GetError());
            }
        }
    }
}
