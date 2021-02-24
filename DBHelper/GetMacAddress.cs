﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBHelper
{
    public class GetMacAddress
    {
        static string docPath = "logConsoleServerApp.txt";
        public KeyValuePair<string, string> GetMac(string[] mac)
        {
            KeyValuePair<string, string> result = new KeyValuePair<string, string>();

            using (var context = new organisationdatabaseEntities())
            {
                try
                {
                    foreach (string m in mac)
                    {
                        var temp = m.Replace(':', ' ').ToUpper();
                        if (context.classdetails.Any(x => x.deskmac.ToUpper() == temp))
                        {
                            var d = context.classdetails.Where(x => x.deskmac.ToUpper() == temp).Select(y => y.ccmac).SingleOrDefault();

                            result = new KeyValuePair<string, string>(temp, d.ToUpper());
                            break;
                        }
                    }
                }
                finally
                {
                    context.Dispose();
                }
            }
            return result;
        }
       
        public async Task<int> SaveInactiveDesktopAsync(string deskmac,string action)
        {            
            using(var context = new organisationdatabaseEntities())
            {
                try
                {
                    var id = context.classdetails.Where(x => x.deskmac.ToUpper() == deskmac.ToUpper())
                                        .Select(x => x.classID).FirstOrDefault();
                    temp_desktopevents tmp = new temp_desktopevents()
                    {
                        Action = action,
                        ActionTime = DateTime.Now,
                        Deskmac = deskmac,
                        classid = id
                    };
                    context.temp_desktopevents.Add(tmp);
                    
                }
                catch (Exception ex)
                {
                    File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString()
                        + " " + DateTime.Now.ToLongTimeString() + "exception in recording machine logs: "
                        + ex.StackTrace + " error message " + ex.InnerException);
                }
                return await context.SaveChangesAsync();
            }
            
        }

        public int GetClassID(string mac)
        {
            int id = 0;
            using (var context = new organisationdatabaseEntities())
            {
                try
                {
                    id = context.classdetails.Where(x => x.ccmac.ToUpper() == mac.ToUpper())
                                        .Select(x => x.classID).FirstOrDefault();                   
                }
                catch (Exception ex)
                {
                    File.AppendAllText(docPath, Environment.NewLine + DateTime.Now.ToLongDateString()
                        + " " + DateTime.Now.ToLongTimeString() + "exception in getting classid from machine mac : "
                        + ex.StackTrace + " error message " + ex.InnerException);
                }
                return id;
            }
        }
         
        public int UpdateStatCardReg(string stat, string mac,string cid)
        {
            int result = 0;
           
            using(var context = new organisationdatabaseEntities())
            {
                var classid = context.classdetails.Where(x => x.ccmac == mac).Select(x => x.classID).FirstOrDefault();
                if(context.card_registration.Any(x=>x.calssId==classid && x.OneCardId == cid))
                {
                   var row= context.card_registration.Where(x => x.calssId == classid && x.OneCardId == cid)
                        .Select(x => x).FirstOrDefault();
                    if (row != null)
                        row.Status = stat;
                   result= context.SaveChanges();
                }
            }
            return result;
        }

        public int SaveReaderLog(string message, string mac, string cardid)
        {
            int result = 0;
            try
            {
                using (var context = new organisationdatabaseEntities())
                {
                    cardlog cardlog = new cardlog()
                    {
                        Message = message,
                        MachineMac = mac,
                        cardId = cardid,
                        ActionTime = DateTime.Now
                    };
                    context.cardlogs.Add(cardlog);
                    result = context.SaveChanges();

                }
            }
           catch(Exception ex)
            {

            }
            return result;
        }
    }
}
