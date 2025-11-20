using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using NorthStar.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northstar.Core
{
    public static class Utility
    {
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[200 * 1024];

            input.Position = 0; // Add this line to set the input stream position to 0

            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static string DataTableToCSVString(System.Data.DataTable table)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sb.Append(table.Columns[i]);
                if (i < table.Columns.Count - 1)
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine();

            foreach (DataRow dr in table.Rows)
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        value = string.Format("\"{0}\"", value.Replace("\"", "\"\""));
                        sb.Append(value);
                        // SH on 9/1/2016.  Put EVERYTHIN in quotes... comments are messing up the CSVs with unprintable characters

                        //if (value.Contains(','))
                        //{
                        //    value = string.Format("\"{0}\"", value.Replace("\"", "\"\""));
                        //    sb.Append(value);
                        //}
                        //else
                        //{
                        //    sb.Append(dr[i].ToString());
                        //}
                    }
                    if (i < table.Columns.Count - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public static void AddNewImportQueueMessage(int jobId, NSConstants.Azure.JobType jobType)
        {
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureBlobStorage"].ConnectionString);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference(NSConstants.Azure.JobQueue);

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExists();

            CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(new NSAzureJob { JobId = jobId, JobType = jobType }));
            queue.AddMessage(message);
        }
    }
}
