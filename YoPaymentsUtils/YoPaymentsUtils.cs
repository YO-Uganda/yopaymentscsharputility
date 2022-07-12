using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace YoPaymentsUtils
{
    class YoPaymentsUtils
    {
        public string url = "";
        public string username = "";
        public string password = "";
        public string privateKeyFile = "";
        public string error = "";
        public string trace = "";

        public YoPaymentsUtils(string username, string password, string url, String privateKeyFile)
        {
            this.url = url;
            this.username = username;
            this.password = password;
            this.privateKeyFile = privateKeyFile;
        }

        public string getUniqueId()
        {
            Guid myuuid = Guid.NewGuid();
            return myuuid.ToString();
        }

        /*
        * Runs an acwithdrawfunds API with the base64 signature included.
        * You must have generated a key pair following the API instructions, 
        * and shared your public key with Yo Uganda team for configuration.
        * 
        * @Param account:   This is the mobile money number.
        * @Param amount:    This the amount to be sent to the account.
        * @Param narrative: Text description of the transaction. 
        * @Param reference: This is the unique reference from our side.
        * 
        * Return YoPaymentsResponse object
        * 
        */
        public YoPaymentsResponse runWithdrawFundsAPi(string account, string amount, string narrative, string reference)
        {
            String nounce = getUniqueId();
            String dataToSign = this.username + amount + account + narrative + reference + nounce;
            String signatureBase64 = signData(dataToSign);
            if (signatureBase64 == null)
            {
                this.error = "Failed to generate the signature";
                return null;
            }

            string xml_data = "<?xml version='1.0' encoding='UTF-8'?>"
                + "<AutoCreate>"
                + "<Request>"
                + "<APIUsername>" + this.username + "</APIUsername >"
                + "<APIPassword>" + this.password + "</APIPassword>"
                + "<Method>acwithdrawfunds</Method>"
                + "<Amount>" + amount + "</Amount>"
                + "<Account>" + account + "</Account>"
                + "<Narrative>" + narrative + "</Narrative>"
                + "<ExternalReference>" + reference + "</ExternalReference>"
                + "<PublicKeyAuthenticationNonce>" + nounce + "</PublicKeyAuthenticationNonce>"
                + "<PublicKeyAuthenticationSignatureBase64>" + signatureBase64 + "</PublicKeyAuthenticationSignatureBase64>"
                + "</Request>"
                + "</AutoCreate>";


            trace = "-----URL-----\n" + url + "\n\n";
            trace += "-----Request Boday-----\n";
            trace += xml_data + "\n\n";

            string response_data = runHttpRequestToYo(xml_data);

            trace += "-----Response Boday-----\n";
            trace += response_data + "\n\n";

            YoPaymentsResponse responseObject = parseYoResponse(response_data);

            return responseObject;
        }

        /*
        * Runs acwithdrawfunds API without the base64 signature and nonce.
        * Use this method if you never submitted your public key to Yo Uganda 
        * for configuration.
        *        
        * @Param account:   This is the mobile money number.
        * @Param amount:    This the amount to be sent to the account.
        * @Param narrative: Text description of the transaction. 
        * @Param reference: This is the unique reference from our side.
        * 
        */
        public YoPaymentsResponse runWithdrawFundsAPiWithoutSignature(string account, string amount, string narrative, string reference)
        {
            String nounce = getUniqueId();

            string xml_data = "<?xml version='1.0' encoding='UTF-8'?>"
                + "<AutoCreate>"
                + "<Request>"
                + "<APIUsername>" + this.username + "</APIUsername >"
                + "<APIPassword>" + this.password + "</APIPassword>"
                + "<Method>acwithdrawfunds</Method>"
                + "<Amount>" + amount + "</Amount>"
                + "<Account>" + account + "</Account>"
                + "<Narrative>" + narrative + "</Narrative>"
                + "<ExternalReference>" + reference + "</ExternalReference>"
                + "</Request>"
                + "</AutoCreate>";


            trace = "-----URL-----\n" + url + "\n\n";
            trace += "-----Request Boday-----\n";
            trace += xml_data + "\n\n";

            string response_data = runHttpRequestToYo(xml_data);

            trace += "-----Response Boday-----\n";
            trace += response_data + "\n\n";

            YoPaymentsResponse responseObject = parseYoResponse(response_data);

            return responseObject;
        }


        /*
        * Runs acdepositfunds API to initiate a mobile money transaction 
        * against the user's phone number.
        *        
        * @Param account:   This is the mobile money number.
        * @Param amount:    This the amount to be sent to the account.
        * @Param narrative: Text description of the transaction. 
        * @Param reference: This is the unique reference from our side.
        * @Param ipn_url:   Set this to your callback URL. See document for more.
        * @Param failure_url:   Set this to failure notificaiton url. See documentation.
        * 
        */
        public YoPaymentsResponse runDepositFundsApi(string account, string amount, string narrative,
            string reference, String ipn_url, string failure_url)
        {
            String nounce = getUniqueId();

            string xml_data = "<?xml version='1.0' encoding='UTF-8'?>"
                + "<AutoCreate>"
                + "<Request>"
                + "<APIUsername>" + this.username + "</APIUsername >"
                + "<APIPassword>" + this.password + "</APIPassword>"
                + "<Method>acdepositfunds</Method>"
                + "<NonBlocking>TRUE</NonBlocking>"
                + "<Amount>" + amount + "</Amount>"
                + "<Account>" + account + "</Account>"
                + "<Narrative>" + narrative + "</Narrative>"
                + "<ExternalReference>" + reference + "</ExternalReference>";
            xml_data += (ipn_url.Length > 0 ? "<InstantNotificationUrl>" + ipn_url + "</InstantNotificationUrl>" : "");
            xml_data += (failure_url.Length > 0 ? "<FailureNotificationUrl>" + failure_url + "</FailureNotificationUrl>" : "");

            xml_data += "</Request>"
                + "</AutoCreate>";


            trace = "-----URL-----\n" + url + "\n\n";
            trace += "-----Request Boday-----\n";
            trace += xml_data + "\n\n";

            string response_data = runHttpRequestToYo(xml_data);

            trace += "-----Response Boday-----\n";
            trace += response_data + "\n\n";

            YoPaymentsResponse responseObject = parseYoResponse(response_data);

            return responseObject;
        }

        /*
        * Runs actransactioncheckstatus to obtain details of an 
        * earlier submitted transaction.       
        *        
        * @Param reference: This is your external reference for the transaction submitted earlier.
        * 
        * 
        */
        public YoPaymentsResponse runTransactionCheckstatusApi(string reference)
        {
            String nounce = getUniqueId();

            string xml_data = "<?xml version='1.0' encoding='UTF-8'?>"
                + "<AutoCreate>"
                + "<Request>"
                + "<APIUsername>" + this.username + "</APIUsername >"
                + "<APIPassword>" + this.password + "</APIPassword>"
                + "<Method>actransactioncheckstatus</Method>"
                + "<PrivateTransactionReference>" + reference + "</PrivateTransactionReference>"
                + "</Request>"
                + "</AutoCreate>";


            trace = "-----URL-----\n" + url + "\n\n";
            trace += "-----Request Boday-----\n";
            trace += xml_data + "\n\n";

            string response_data = runHttpRequestToYo(xml_data);

            trace += "-----Response Boday-----\n";
            trace += response_data + "\n\n";

            YoPaymentsResponse responseObject = parseYoResponse(response_data);

            return responseObject;
        }


        /*
        * @Param dataString:    This is the string to of data to sign.
        */
        public string signData(string dataString)
        {
            if (!File.Exists(privateKeyFile))
            {
                this.error = "PrivateKey File " + privateKeyFile + " does not exist";
                return null;
            }
            //Read the privateKeyFile
            string privateKeyString = File.ReadAllText(privateKeyFile);

            SHA1Managed sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(dataString));
            string hash_sha1 = BitConverter.ToString(hash).Replace("-", "").ToLower();

            RSACryptoServiceProvider provider = new RSACryptoServiceProvider();
            provider.FromXmlString(getKeyInXml(privateKeyString));
            byte[] signedBytes = provider.SignData(Encoding.UTF8.GetBytes(hash_sha1), new SHA1CryptoServiceProvider());

            return Convert.ToBase64String(signedBytes, 0, signedBytes.Length);
        }

        public static string Hash(string stringToHash)
        {
            using (var sha1 = new SHA1Managed())
            {
                return BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(stringToHash)));
            }
        }

        /*
        * @Param privateKeyInPemFormat: This is the content of key file (in xxxx.pem)
        */
        public string getKeyInXml(string privateKeyInPemFormat)
        {
            return RsaKeyConverter.PemToXml(privateKeyInPemFormat);
        }

        /*
        * @Param responseText:  This is the entire response text from Yo
        */
        private YoPaymentsResponse parseYoResponse(string responseText)
        {
            YoPaymentsResponse r = new YoPaymentsResponse("", "", "");
            if (responseText.Length < 1)
            {
                return null;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(responseText);

            //Get First child: Response
            XmlNode Response = doc.DocumentElement.FirstChild;

            if (Response.HasChildNodes)
            {
                for (int i = 0; i < Response.ChildNodes.Count; i++)
                {
                    XmlNode node = Response.ChildNodes[i];
                    switch (node.Name)
                    {
                        case "Status":
                            r.setStatus(node.InnerText);
                            break;
                        case "StatusCode":
                            r.setStatusCode(node.InnerText);
                            break;
                        case "StatusMessage":
                            r.setStatusMessage(node.InnerText);
                            break;
                        case "TransactionReference":
                            r.setTransactionReference(node.InnerText);
                            break;
                        case "TransactionStatus":
                            r.setTransactionStatus(node.InnerText);
                            break;
                        case "ErrorMessage":
                            r.setErrorMessage(node.InnerText);
                            break;
                        case "MNOTransactionReferenceId":
                            r.setNetworkRef(node.InnerText);
                            break;

                        default:
                            break;
                    }

                }
            }


            return r;
        }

        /*
        * @Param xml_data:  This is the xml_data to send to Yo
        */
        private string runHttpRequestToYo(string xml_data)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(this.url);
                req.ContentType = "text/xml";
                req.Method = "post";

                byte[] byteArray = Encoding.UTF8.GetBytes(xml_data);
                req.ContentLength = byteArray.Length;

                Stream dataStream = req.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse response = req.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);

                string responseString = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                response.Close();

                return responseString;
            }
            catch (IOException e)
            {
                return e.Message;
            }
        }
    }

    public class YoPaymentsResponse
    {
        public string status;
        public string statusCode;
        public string transactionReference;
        public string transactionStatus;
        public string errorMessage;
        public string statusMessage;
        public string networkReference;

        public YoPaymentsResponse(string status, string statusCode, string statusMessage)
        {
            this.status = status;
            this.statusCode = statusCode;
            this.statusMessage = statusMessage;
        }

        public void setNetworkRef(String network_ref)
        {
            this.networkReference = network_ref;
        }
        public string getNetworkRef()
        {
            return this.networkReference;
        }

        public void setStatus(String status)
        {
            this.status = status;
        }
        public string getStatus()
        {
            return this.status;
        }

        public void setStatusCode(String statusCode)
        {
            this.statusCode = statusCode;
        }
        public string getStatusCode()
        {
            return this.statusCode;
        }

        public void setTransactionStatus(String txStatus)
        {
            this.transactionStatus = txStatus;
        }

        public string getTransactionStatus()
        {
            return this.transactionStatus;
        }

        public void setTransactionReference(String txReference)
        {
            this.transactionReference = txReference;
        }

        public string getTransactionReference()
        {
            return this.transactionReference;
        }

        public void setErrorMessage(String error)
        {
            this.errorMessage = error;
        }

        public string getErrorMessage()
        {
            return this.errorMessage;
        }

        public void setStatusMessage(String statusMessage)
        {
            this.statusMessage = statusMessage;

        }

        public string getStatusMessage()
        {
            return this.statusMessage;
        }

        public String toString()
        {
            return "Status: " + status + ", "
                + "StatusCode: " + statusCode + ", "
                + "StatusMessage: " + statusMessage + ", "
                + "TransactionReference: " + transactionReference + ", "
                + "TransactionStatus: " + transactionStatus + ""
                + "ErrorMessage: " + errorMessage + ", ";
        }

    }

    public static class RsaKeyConverter
    {
        public static string XmlToPem(string xml)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(xml);

                AsymmetricCipherKeyPair keyPair = rsa.GetKeyPair(); // try get private and public key pair
                if (keyPair != null) // if XML RSA key contains private key
                {
                    PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
                    return FormatPem(privateKeyInfo.GetEncoded().ToBase64(), "RSA PRIVATE KEY");
                }

                RsaKeyParameters publicKey = rsa.GetPublicKey(); // try get public key
                if (publicKey != null) // if XML RSA key contains public key
                {
                    SubjectPublicKeyInfo publicKeyInfo =
                        SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
                    return FormatPem(publicKeyInfo.GetEncoded().ToBase64(), "PUBLIC KEY");
                }
            }

            throw new InvalidKeyException("Invalid RSA Xml Key");
        }

        public static async Task<string> XmlToPemAsync(string xml)
        {
            return await Task.Run(() => XmlToPem(xml));
        }

        private static string FormatPem(string pem, string keyType)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("-----BEGIN {0}-----\n", keyType);

            int line = 1, width = 64;

            while ((line - 1) * width < pem.Length)
            {
                int startIndex = (line - 1) * width;
                int len = line * width > pem.Length
                              ? pem.Length - startIndex
                              : width;
                sb.AppendFormat("{0}\n", pem.Substring(startIndex, len));
                line++;
            }

            sb.AppendFormat("-----END {0}-----\n", keyType);
            return sb.ToString();
        }

        public static string PemToXml(string pem)
        {
            if (pem.StartsWith("-----BEGIN RSA PRIVATE KEY-----")
                || pem.StartsWith("-----BEGIN PRIVATE KEY-----"))
            {
                return GetXmlRsaKey(pem, obj =>
                {
                    if ((obj as RsaPrivateCrtKeyParameters) != null)
                        return DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)obj);
                    var keyPair = (AsymmetricCipherKeyPair)obj;
                    return DotNetUtilities.ToRSA((RsaPrivateCrtKeyParameters)keyPair.Private);
                }, rsa => rsa.ToXmlString(true));
            }

            if (pem.StartsWith("-----BEGIN PUBLIC KEY-----"))
            {
                return GetXmlRsaKey(pem, obj =>
                {
                    var publicKey = (RsaKeyParameters)obj;
                    return DotNetUtilities.ToRSA(publicKey);
                }, rsa => rsa.ToXmlString(false));
            }

            throw new InvalidKeyException("Unsupported PEM format...");
        }

        public static async Task<string> PemToXmlAsync(string pem)
        {
            return await Task.Run(() => PemToXml(pem));
        }

        private static string GetXmlRsaKey(string pem, Func<object, RSA> getRsa, Func<RSA, string> getKey)
        {
            using (var ms = new MemoryStream())
            using (var sw = new StreamWriter(ms))
            using (var sr = new StreamReader(ms))
            {
                sw.Write(pem);
                sw.Flush();
                ms.Position = 0;
                var pr = new PemReader(sr);
                object keyPair = pr.ReadObject();
                using (RSA rsa = getRsa(keyPair))
                {
                    var xml = getKey(rsa);
                    return xml;
                }
            }
        }
    }

    public static class RsaExtensions
    {
        public static AsymmetricCipherKeyPair GetKeyPair(this RSA rsa)
        {
            try
            {
                return DotNetUtilities.GetRsaKeyPair(rsa);
            }
            catch
            {
                return null;
            }
        }

        public static RsaKeyParameters GetPublicKey(this RSA rsa)
        {
            try
            {
                return DotNetUtilities.GetRsaPublicKey(rsa);
            }
            catch
            {
                return null;
            }
        }
    }

    public static class BytesExtensions
    {
        public static string ToBase64(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }

    class YoSignatureVerifier
    {
        private String dateTime;
        private String networkRef;
        private String externalRef;
        private String msisdn;
        private String narrative;
        private String amount;
        private String signature;

        private String publicKeyFile;

        private String errorMessage = "";

        public YoSignatureVerifier(String publicKeyFile_)
        {
            this.publicKeyFile = publicKeyFile_;
        }

        /*
        * Sets the base64 signature field        
        * @Param String dateTime: This is the datetime
        *        
        */
        public void SetDateTime(String dateTime_)
        {
            this.dateTime = dateTime_;
        }
        /*
        *       
        * @Param String signature: This is the network reference.
        *        
        */
        public void SetNetworkRef(String networkRef_)
        {
            this.networkRef = networkRef_;
        }
        /*
        * Sets the base64 signature field        
        * @Param String signature: This is the external ref
        *        
        */
        public void SetExternalRef(String externalRef_)
        {
            this.externalRef = externalRef_;
        }
        /*
        *    
        * @Param String signature: This is the msisdn field of the transaction
        *        
        */
        public void SetMsisdn(String msisdn_)
        {
            this.msisdn = msisdn_;
        }
        /*
        *       
        * @Param String narrative: This is the narrative of the transaction.
        *        
        */
        public void SetNarrative(String narrative_)
        {
            this.narrative = narrative_;
        }
        /*
        * Sets the amount field      
        * @Param String amount: This is the amount of the transaction.
        *        
        */
        public void SetAmount(String amount_)
        {
            this.amount = amount_;
        }

        /*
        * Sets the base64 signature field        
        * @Param String signature: This is the base64 encoded signature.
        *        
        */
        public void SetSignature(String signature_)
        {
            this.signature = signature_;
        }


        /*
        * Returns: True if the signature verification passed or False if it failed
        * or an error occurred.       
        */
        public bool verify()
        {
            if (dateTime == null)
            {
                this.errorMessage = "dateTime Field is null";
                return false;
            }
            if (amount == null)
            {
                this.errorMessage = "amount Field is null";
                return false;
            }
            if (narrative == null)
            {
                this.errorMessage = "narrative Field is null";
                return false;
            }
            if (networkRef == null)
            {
                this.errorMessage = "networkRef Field is null";
                return false;
            }
            if (externalRef == null)
            {
                this.errorMessage = "externalRef Field is null";
                return false;
            }
            if (msisdn == null)
            {
                this.errorMessage = "msisdn Field is null";
                return false;
            }

            String signedData = dateTime + amount + narrative + networkRef + externalRef + msisdn;
            bool v = VerifyTheSignature(signature, signedData);
            if (v)
            {
                return true;
            }
            else
            {
                this.errorMessage = "Signature verification failed.";
                return false;
            }
        }

        public String GetError()
        {
            return this.errorMessage;
        }

        /*
        * @Param signature: This is the base64 string of data data to be verified.
        * @Param data: This is the string data that was signed       
        *        
        * Returns Bool: True if signature passed or false if failed | an error.       
        */

        private bool VerifyTheSignature(String base64Signature, String signedData)
        {
            try
            {
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();

                byte[] signature_ = Convert.FromBase64String(base64Signature);

                byte[] data = Encoding.UTF8.GetBytes(signedData);

                if (!File.Exists(publicKeyFile))
                {
                    this.errorMessage = "Verification failed: PublicKey Certificate: "
                        + publicKeyFile + " does not exists";
                    return false;
                }

                var x509 = new X509Certificate2(publicKeyFile);

                if (!(x509.PublicKey.Key is RSACryptoServiceProvider))
                {
                    this.errorMessage = "Verification failed: Failed to load x509.PublicKey";
                    return false;
                }

                string sha1O_id = CryptoConfig.MapNameToOID("SHA1");

                //use the certificate to verify data against the signature
                bool sha1_valid = rsa.VerifyData(data, sha1O_id, signature_);

                return sha1_valid;
            }
            catch (Exception e)
            {
                this.errorMessage = "Verification failed: " + e.Message;
                return false;
            }
        }
    }
}
