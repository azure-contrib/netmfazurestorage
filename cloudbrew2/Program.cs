using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using NetMf.CommonExtensions;
using System.IO;
using System.Text;
using netmfazurestorage.Blob;
using netmfazurestorage.Helper;

namespace cloudbrew2
{
    /// <summary>
    /// This is a basic demo of NETMF and IO.
    /// </summary>
    public class Program
    {
        static InterruptPort _onBoardButton;
        static AnalogInput _analogInput;
        private static object Padlock = new object();
        private static string _macAddress;

        private static BlobClient _blobClient;

        private const string AccountName = "netmftest";  // please upload pictures of cats and other larger animals to this storage account.
        private const string AccountKey = "gYU/Nf/ib97kHQrVkxhdD3y0lKz6ZOVaR1FjeDpESecGuqZOEq1TE+5+SXfZ/DBzKsXH3m0NDsLxTbTqQxL9yA==";

        public static void Main()
        {
            DeviceAssist.SetupDefault();

            try
            {
                if (File.Exists("\\SD\\Data.csv"))
                {
                    File.Delete("\\SD\\Data.csv");
                }

                _onBoardButton = new InterruptPort(Pins.ONBOARD_SW1, true,
                                                                    Port.ResistorMode.Disabled,
                                                                    Port.InterruptMode.InterruptEdgeHigh);
                _onBoardButton.OnInterrupt += onBoardButton_OnInterrupt;

                _macAddress = "holding";

                _blobClient = new BlobClient(new netmfazurestorage.Account.CloudStorageAccount(AccountName, AccountKey));
            }
            catch (Exception ex)
            {
                //there was an error setting up the device
                throw;
            }

            _analogInput = new AnalogInput(AnalogChannels.ANALOG_PIN_A0);

            int counter = 0;
            while (true)
            {
                counter++;
                var data = _analogInput.Read() * 40D;

                lock (Padlock)
                {
                    using (FileStream fs = File.Open("\\SD\\Data.csv", FileMode.Append, FileAccess.Write))
                    {
                        Debug.Print(data.ToString());
                        var dataBytes = Encoding.UTF8.GetBytes(
                            StringUtility.Format("{0}, {1}, {2}\r\n",
                                                 _macAddress, DateTime.Now.ToString(),
                                                 data)
                            );

                        fs.Write(dataBytes, 0, dataBytes.Length);
                        fs.Flush();
                    }
                }

                System.Threading.Thread.Sleep(1000);
                Debug.Print("Working");
            }
        }


        static void onBoardButton_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            try
            {
                lock (Padlock)
                {
                    var filename = StringUtility.Format("{0}.csv", time.ToString("yyyyMMddhhmmss"));
                    Debug.Print(filename);

                    var success = _blobClient.PutBlockBlob("demo",
                                                     filename,
                                                      "\\SD\\Data.csv");

                    if (success)
                    {
                        Debug.Print("Files uploaded to netmf.blob.core.windows.net");
                    }
                    else
                    {
                        Debug.Print("There was an error, check debug output");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Critical error: " + ex);
            }
        }
    }
}
