using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using openeet_lite;
using System.Data.Odbc;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using EetConnector.Properties;

namespace EetConnector
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 2 && args[0].Equals("encrypt_password")) 
            {                
                Logger.Log(String.Format("Nové heslo: {0}", PasswordCipher.Encrypt(args[1])), Logger.Out.CONSOLE);
                Logger.Log("Přepište hodnotu položky CRT_PASS v souboru .config na nové heslo a spusťte program znovu.", Logger.Out.CONSOLE);
                Logger.Log("Pokračujte libovolnou klávesou...", Logger.Out.CONSOLE);
                Console.ReadKey();
                return 0;
            }

            String bkp, pkp = "", fik, requestBody, response, datPrij;

            try
            {
                String dbf = Settings.Default.DBF_PATH;
                String dic = Settings.Default.DIC;
                String idProvoz = Settings.Default.ID_PROVOZ;
                String certPath = Settings.Default.CRT_PATH;
                String certPassword = PasswordCipher.Decrypt(Settings.Default.CRT_PASS);
                String reqUrl = Settings.Default.REQ_URL;

                EetDbfConnector dbfConnector = new EetDbfConnector(dbf);

                Dictionary<String, String> data = dbfConnector.ReadData();

                // generate XML request
                EetRegisterRequest request = EetRegisterRequest.builder()
                   .dic_popl(dic)
                   .id_provoz(idProvoz)
                   .id_pokl(data["id_pokl"])
                   .porad_cis(data["porad_cis"])
                   .dat_trzby(data["dat_trzby"])
                   .dan1(data["dan1"])
                   .dan2(data["dan2"])
                   .dan3(data["dan3"])
                   .zakl_dan1(data["zakl_dan1"])
                   .zakl_dan2(data["zakl_dan2"])
                   .zakl_dan3(data["zakl_dan3"])
                   .zakl_nepodl_dph(data["zakl_nepod"])
                   .celk_trzba(data["celk_trzba"])
                   .rezim(Rezim.ZJEDNODUSENY)
                   .prvni_zaslani(data["pkp1"].Equals("") && data["pkp2"].Equals("") ? PrvniZaslani.PRVNI : PrvniZaslani.OPAKOVANE)
                   .pkcs12(certPath)
                   .pkcs12password(certPassword)
                   .build();

                pkp = request.formatPkp();
                if (pkp == null) throw new ApplicationException("Špatně vypočtená PKP hodnota.");

                bkp = request.formatBkp();
                if (bkp == null) throw new ApplicationException("Špatně vypočtená BKP hodnota.");

                requestBody = request.generateSoapRequest();
                if (requestBody == null) throw new ApplicationException("Nepodařilo se vygenerovat SOAP zprávu.");

                response = request.sendRequest(requestBody, reqUrl);
                if (response == null) throw new ApplicationException("Nepodařilo se spojit se serverem.");

                // search for FIK
                String search = "<eet:Potvrzeni fik=\"";
                int indexOf = response.IndexOf(search);
                if (indexOf < 0) throw new ApplicationException("FIK nebyl v odpovědi nalezen.");
                fik = response.Substring(indexOf + search.Length, 39);

                // search fir DAT_PRIJ
                search = " dat_prij=\"";
                indexOf = response.IndexOf(search);
                datPrij = DateTime.Parse(response.Substring(indexOf + search.Length, 25)).ToString("yyyy-MM-dd' 'HH:mm:ss");

                dbfConnector.Write(fik, bkp, datPrij);
                Logger.Log("FIK, BKP a DAT_PRIJ byly úspěšně získány.", Logger.Out.FILE);
                return 0;
            }
            catch (Exception e)
            {
                Logger.Log(e.Message, Logger.Out.FILE, Logger.Type.ERROR);
                if (pkp != null && !pkp.Equals(""))
                {
                    try
                    {
                        String dbf = Properties.Settings.Default.DBF_PATH;
                        EetDbfConnector dbfConnector = new EetDbfConnector(dbf);
                        dbfConnector.Write(pkp);
                        var message = String.Format("PKP bylo úspěšně zapsáno. PKP: {0}", pkp);
                        Logger.Log(message, Logger.Out.FILE);
                    }
                    catch (Exception) 
                    {
                        var message = String.Format("Nepodařilo se zapsat PKP. PKP: {0}", pkp);
                        Logger.Log(message, Logger.Out.FILE, Logger.Type.ERROR);
                    }
                }
                return -1;
            }
        }
    }

    public static class Logger {
        
        public enum Type 
        {
            INFO, SUCCESS, ERROR, WARNING
        }

        public enum Out 
        {
            FILE, CONSOLE, BOTH
        }

        public static void Log(String message, Out output = Out.FILE, Type type = Type.INFO)
        {
            String timestamp = DateTime.Now.ToString("yyyy-MM-dd' 'HH:mm:ss");
            String logEntry = String.Format("[{0}][{1}] {2}", timestamp, type.ToString(), message);
            String consoleEntry = String.Format("{0}", message);

            if(output == Out.FILE || output == Out.BOTH) {
                logFile(logEntry);
            }

            if (output == Out.CONSOLE || output == Out.BOTH) 
            {
                logConsole(consoleEntry);
            }
        }

        private static void logFile(String entry) 
        {
            try
            {
                var logFilePath = Settings.Default.LOG_PATH;

                if (!File.Exists(logFilePath)) 
                {
                    File.Create(logFilePath).Close();
                }

                var length = new FileInfo(logFilePath).Length;

                if (length >= 20000)
                {
                    var lines = File.ReadAllLines(logFilePath);
                    File.WriteAllLines(logFilePath, lines.Skip(lines.Length / 2).ToArray());
                }

                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine(entry);
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine("Nepodařilo se zapsat do log souboru '{0}'.", Settings.Default.LOG_PATH);
                Console.WriteLine("Detail chyby: {0}", e.Message);
            }
        }

        private static void logConsole(String entry)
        {
            Console.WriteLine(entry);
        }
    }
}
