using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EetConnector
{
    class EetDbfConnector
    {
        private String dbf;

        public EetDbfConnector(String dbf)
        {
            this.dbf = dbf;
        }

        public Dictionary<String, String> ReadData()
        {
            Dictionary<String, String> data;

            using (var dbf = new OdbcConnection())
            {
                dbf.ConnectionString = this.buildDbfConnectionString();
                dbf.Open();

                using (var odbcCmd = dbf.CreateCommand())
                {
                    odbcCmd.CommandText = String.Format("SELECT * FROM {0}", this.dbf);

                    using (OdbcDataReader reader = odbcCmd.ExecuteReader())
                    {
                        int fieldCount = reader.FieldCount;
                        data = new Dictionary<String, String>();
                        reader.Read();

                        for (int i = 0; i < fieldCount; i++)
                        {
                            data.Add(reader.GetName(i).ToLower(), reader.GetValue(i).ToString());
                        }
                    }
                }
            }
            return data;
        }

        public void Write(String pkp)
        {
            using (var dbf = new OdbcConnection())
            {
                dbf.ConnectionString = this.buildDbfConnectionString();
                dbf.Open();

                using (var odbcCmd = dbf.CreateCommand())
                {
                    odbcCmd.CommandText = String.Format("UPDATE {0} SET pkp1='{1}', pkp2='{2}'", this.dbf, pkp.Substring(0, pkp.Length / 2), pkp.Substring(pkp.Length / 2));
                    odbcCmd.ExecuteNonQuery();
                }
            }
        }

        public void Write(String fik, String bkp, String datPrij)
        {
            using (var dbf = new OdbcConnection())
            {
                dbf.ConnectionString = this.buildDbfConnectionString();
                dbf.Open();

                using (var odbcCmd = dbf.CreateCommand())
                {
                    odbcCmd.CommandText = String.Format("UPDATE {0} SET fik='{1}', bkp='{2}', dat_prij='{3}'", this.dbf, fik, bkp, datPrij);
                    odbcCmd.ExecuteNonQuery();
                }
            }
        }

        private string buildDbfConnectionString()
        {
            return String.Format("Driver={0};SourceType=DBF;"
                + "SourceDB={1};Exclusive=No;NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO;",
                "{Microsoft dBase Driver (*.dbf)}",
                this.dbf);
        }
    }
}
