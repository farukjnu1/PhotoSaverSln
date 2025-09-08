using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoSaver
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("The process started at: {0}", DateTime.Now);
            Console.WriteLine("running...");

            // Create a Timer object that knows to call our TimerCallback
            // method once every 200 milliseconds.
            Timer t = new Timer(ReadFiles, null, 0, 300);

            //ReadFiles(null);

            // Wait for the user to hit <Enter>
            Console.ReadLine();
        }

        private static void TimerCallback(Object o)
        {
            // Display the date/time when this method got called.
            Console.WriteLine("In TimerCallback: " + DateTime.Now);
            // Force a garbage collection to occur for this demo.
            GC.Collect();
        }

        private static void ReadFiles(Object o)
        {
            try
            {
                var rootFTP = @System.Configuration.ConfigurationManager.AppSettings["rootFTP"];
                var rootPhoto = @System.Configuration.ConfigurationManager.AppSettings["rootPhoto"];
                string[] teidS = Directory.GetDirectories(rootFTP, "*", SearchOption.TopDirectoryOnly);
                
                foreach (string teid in teidS)
                {
                    string[] cidS = Directory.GetDirectories(teid, "*", SearchOption.TopDirectoryOnly);
                    
                    foreach (string cid in cidS)
                    {
                        string[] dates = Directory.GetDirectories(cid, "*", SearchOption.TopDirectoryOnly);

                        foreach (string date in dates)
                        {
                            string[] img50 = Directory.GetDirectories(date, "*", SearchOption.TopDirectoryOnly);

                            foreach (string img01 in img50)
                            {
                                string[] imgS = Directory.GetFiles(img01, "*.jpg", SearchOption.TopDirectoryOnly);

                                foreach (var img in imgS)
                                {
                                    var createTime = File.GetCreationTime(img);

                                    if (DateTime.Now.AddMinutes(-3) > createTime)
                                    {
                                        Int32 unixTimestamp = (Int32)(createTime.Subtract(new DateTime(1970, 1, 1, 6, 0, 0))).TotalSeconds;
                                        var rel = img.Substring(rootFTP.Length + 1);
                                        var relPath = img.Substring(rootFTP.Length);

                                        string strTEID = rel.Split('\\')[0];
                                        int nTime = unixTimestamp;
                                        int nPhotoType = 2;
                                        string strAbsolutePath = rootPhoto + relPath;
                                        relPath = relPath.Replace("\\", "/");
                                        string strRelativePath = relPath;
                                        int nCameraID = 0;

                                        string insert = @"INSERT INTO 
                                [Table_Photo]([strTEID]
                                ,[nTime]
                                ,[nPhotoType]
                                ,[strRelativePath]
                                ,[strAbsolutePath]
                                ,[nCameraID])
                                VALUES('" + strTEID + @"'
                                ,'" + nTime + @"'
                                ,'" + nPhotoType + @"'
                                ,'" + strRelativePath + @"'
                                ,'" + strAbsolutePath + @"'
                                ,'" + nCameraID + @"')";

                                        var db = new Services.MsSqlService();
                                        var isInsert = db.Set("", insert);
                                        if (isInsert)
                                        {
                                            // Determine whether the directory exists.
                                            if (!Directory.Exists(strAbsolutePath))
                                            {
                                                Directory.CreateDirectory(Path.GetDirectoryName(strAbsolutePath));
                                            }

                                            if (!File.Exists(strAbsolutePath))
                                            {
                                                File.Copy(img, strAbsolutePath);
                                            }

                                            File.Delete(img);
                                        }

                                    }
                                }

                            }
                            
                        }

                        //Console.WriteLine("deleted: {0}", imgDel);

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("The process failed: {0}", ex.ToString());
            }

            GC.Collect();

        }


    }
}
