using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AtlasNetClient
{
    public static class BackgroundWorker
    {
        public delegate void ProgressMessage(bool complete, string message);
        public static ProgressMessage OnProgress = delegate { };
        private static bool retrieveInProgress = false;

        public static void SendMessage(Message message, bool sign)
        {
            var thread = new Thread(new ThreadStart(delegate
            {
                string package = new AtlasClient().PrepareMessage(message, sign, OnProgress);
                OnProgress(false, "Sending message");
                var c = App.Instance.ConnectionPool[App.Instance.Config.BootstrapNode];
                c.Send(message, package);
                OnProgress(true, null);
            }));
            thread.Start();
            thread.IsBackground = true;
        }

        public static void RetrieveMail()
        {
            if (retrieveInProgress)
                return;
            retrieveInProgress = true;
            var thread = new Thread(new ThreadStart(delegate
             {
                 OnProgress(false, "Retrieving listings");
                 var listings = App.Instance.ConnectionPool[App.Instance.Config.BootstrapNode].GetListings();
                 int index = 0;
                 foreach (var listing in listings)
                 {
                     OnProgress(false, string.Format("Retrieving message {0} of {1}", ++index, listings.Count));
                     var client = App.Instance.ConnectionPool[listing.Node];
                     OnProgress(false, string.Format("Decrypting message {0} of {1}", index, listings.Count));
                     var msg = new AtlasClient().DecryptMessage(client.RetrieveMessage(listing.Id));
                     App.Instance.Config.Messages.Add(msg);
                 }
                 OnProgress(true, null);
                 retrieveInProgress = false;
             }));
            thread.Start();
            thread.IsBackground = true;
        }
    }
}
