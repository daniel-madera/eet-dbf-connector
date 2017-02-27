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
                Console.WriteLine(String.Format("Nové heslo: {0}", PasswordCipher.Encrypt(args[1])));
                Console.WriteLine("Přepište hodnotu položky CRT_PASS v souboru .config na nové heslo a spusťte program znovu.");
                Console.WriteLine("Pokračujte libovolnou klávesou.");
                Console.ReadKey();
                return 0;
            }

            String bkp, pkp = "", fik, requestBody, response, datPrij;
            TextWriter writer = null;

            try
            {
                // log console output
                writer = File.CreateText("log.txt");
                Console.SetOut(writer);

                String dbf = Settings.Default.DBF_PATH;
                String dic = Settings.Default.DIC;
                String idProvoz = Settings.Default.ID_PROVOZ;
                String certPath = Settings.Default.CRT_PATH;
                String certPassword = PasswordCipher.Decrypt(Settings.Default.CRT_PASS);

                EetDbfConnector dbfConnector = new EetDbfConnector(dbf);

                Mod prod = new Mod("https://prod.eet.cz:443/eet/services/EETServiceSOAP/v3", dic, certPath, certPassword);
                Mod pg = new Mod("https://pg.eet.cz:443/eet/services/EETServiceSOAP/v3", "CZ1212121218", @"certificates\pg_cert.p12", "eet");

                Boolean isPgMode = Settings.Default.EET_TEST;

                Mod mod = isPgMode ? pg : prod;

                Dictionary<String, String> data = dbfConnector.ReadData();

                EetRegisterRequest request = EetRegisterRequest.builder()
                   .dic_popl(mod.dic)
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
                   .pkcs12(mod.certPath)
                   .pkcs12password(mod.certPassword)
                   .build();

                pkp = request.formatPkp();
                if (pkp == null) throw new ApplicationException("Špatně vypočtená PKP hodnota.");

                bkp = request.formatBkp();
                if (bkp == null) throw new ApplicationException("Špatně vypočtená BKP hodnota.");

                requestBody = request.generateSoapRequest();
                if (requestBody == null) throw new ApplicationException("Nepodařilo se spojit se serverem.");
                // throw new ApplicationException("Nepodařilo se spojit se serverem.");

                response = request.sendRequest(requestBody, mod.requestUrl);

                String search = "<eet:Potvrzeni fik=\"";
                int indexOf = response.IndexOf(search);
                if (indexOf < 0) throw new ApplicationException("FIK nebyl v odpovědi nalezen.");
                fik = response.Substring(indexOf + search.Length, 39);

                search = " dat_prij=\"";
                indexOf = response.IndexOf(search);
                datPrij = DateTime.Parse(response.Substring(indexOf + search.Length, 25)).ToString("yyyy-MM-dd HH:mm:ss");

                dbfConnector.Write(fik, bkp, datPrij);
                Console.WriteLine("FIK, BKP a DAT_PRIJ byly úspěšně zapsány.");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Chyba: " + e.Message);
                if (pkp != null && !pkp.Equals(""))
                {
                    try
                    {
                        String dbf = Properties.Settings.Default.DBF_PATH;
                        EetDbfConnector dbfConnector = new EetDbfConnector(dbf);
                        dbfConnector.Write(pkp);
                        Console.WriteLine("PKP bylo úspěšně zapsáno.");
                    }
                    catch (Exception) {}
                }
                return -1;
            }
            finally 
            {
                try
                {
                    writer.Flush();
                    writer.Close();
                }
                catch (Exception) { }
            }
        }
    }

    public struct Mod
    {
        public String requestUrl;
        public String dic;
        public String certPath;
        public String certPassword;

        public Mod(String requestUrl, String dic, String certPath, String certPassword)
        {
            this.requestUrl = requestUrl;
            this.dic = dic;
            this.certPath = certPath;
            this.certPassword = certPassword;
        }
    }
}
